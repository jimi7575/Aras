using System.Text.Json.Serialization;

namespace Aras.External;

public sealed class ArasTraderAuthResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expiresIn")]
    public int? ExpiresIn { get; set; }
}
