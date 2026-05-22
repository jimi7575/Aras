using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Aras.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aras.External;

public sealed class ArasTraderClient(
    HttpClient httpClient,
    IOptions<ArasTraderOptions> options,
    IDistributedCache cache,
    ILogger<ArasTraderClient> logger) : IArasTraderClient
{
    private const string TokenCacheKey = "aras-trader:access-token";
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _refreshAfterUtc;
    private DateTimeOffset _expiresAtUtc;

    public async Task<IReadOnlyList<ArasTraderCustomerDto>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken);

        return await SendWithRetryAsync(async ct =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/customers");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ArasTraderCustomerDto>>(ct) ?? [];
        }, cancellationToken);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (HasUsableToken() && !ShouldRefreshToken())
        {
            return _cachedToken!;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (HasUsableToken())
            {
                return await RefreshOrUseCachedTokenAsync(cancellationToken);
            }

            await LoadTokenFromCacheAsync(cancellationToken);
            if (HasUsableToken())
            {
                return await RefreshOrUseCachedTokenAsync(cancellationToken);
            }

            var token = _cachedToken is null
                ? await RequestTokenAsync(cancellationToken)
                : await RefreshTokenAsync(_cachedToken, cancellationToken) ?? await RequestTokenAsync(cancellationToken);

            SetToken(token);
            await SaveTokenToCacheAsync(cancellationToken);

            return _cachedToken!;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private bool HasUsableToken()
    {
        return !string.IsNullOrWhiteSpace(_cachedToken) && _expiresAtUtc > DateTimeOffset.UtcNow;
    }

    private bool ShouldRefreshToken()
    {
        return _refreshAfterUtc <= DateTimeOffset.UtcNow;
    }

    private async Task<string> RefreshOrUseCachedTokenAsync(CancellationToken cancellationToken)
    {
        if (!ShouldRefreshToken())
        {
            return _cachedToken!;
        }

        var token = await RefreshTokenAsync(_cachedToken!, cancellationToken);
        if (token is not null)
        {
            SetToken(token);
            await SaveTokenToCacheAsync(cancellationToken);
        }

        return _cachedToken!;
    }

    private void SetToken(TokenResult token)
    {
        var now = DateTimeOffset.UtcNow;
        _cachedToken = token.Token;
        _expiresAtUtc = now.AddSeconds(token.ExpiresIn ?? 3600);
        _refreshAfterUtc = _expiresAtUtc.AddMinutes(-2);

        if (_refreshAfterUtc <= now)
        {
            _refreshAfterUtc = now;
        }
    }

    private async Task LoadTokenFromCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            var json = await cache.GetStringAsync(TokenCacheKey, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var tokenCache = JsonSerializer.Deserialize<TokenCache>(json);
            _cachedToken = tokenCache?.Token;
            _refreshAfterUtc = tokenCache?.RefreshAfterUtc ?? default;
            _expiresAtUtc = tokenCache?.ExpiresAtUtc ?? default;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not read Aras Trader token from Redis.");
        }
    }

    private async Task SaveTokenToCacheAsync(CancellationToken cancellationToken)
    {
        var tokenCache = new TokenCache(_cachedToken!, _refreshAfterUtc, _expiresAtUtc);
        var ttl = _expiresAtUtc - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        await cache.SetStringAsync(
            TokenCacheKey,
            JsonSerializer.Serialize(tokenCache),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            cancellationToken);
    }

    private async Task<TokenResult> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var response = await SendWithRetryAsync(ct => httpClient.PostAsJsonAsync("api/auth/token", new
        {
            username = settings.Username,
            password = settings.Password
        }, ct), cancellationToken);

        if (response.StatusCode is HttpStatusCode.Conflict)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Aras Trader already has an active token for this user. Use the cached token in Redis or wait for it to expire before requesting a new token. Response: {error}");
        }

        response.EnsureSuccessStatusCode();
        return await ReadTokenAsync(response, cancellationToken);
    }

    private async Task<TokenResult?> RefreshTokenAsync(string previousToken, CancellationToken cancellationToken)
    {
        var response = await SendWithRetryAsync(ct => httpClient.PostAsJsonAsync("api/auth/refresh", new
        {
            token = previousToken
        }, ct), cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            logger.LogInformation("Aras Trader refresh failed with {StatusCode}; requesting a fresh token.", response.StatusCode);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await ReadTokenAsync(response, cancellationToken);
    }

    private static async Task<TokenResult> ReadTokenAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var auth = await response.Content.ReadFromJsonAsync<ArasTraderAuthResponse>(cancellationToken);
        var token = auth?.AccessToken ?? auth?.Token;

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Auth response did not contain a token.");
        }

        return new TokenResult(token, auth?.ExpiresIn);
    }

    private async Task<T> SendWithRetryAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action(cancellationToken);
            }
            catch (HttpRequestException) when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
        }
    }

    private sealed record TokenResult(string Token, int? ExpiresIn);

    private sealed record TokenCache(string Token, DateTimeOffset RefreshAfterUtc, DateTimeOffset ExpiresAtUtc);
}
