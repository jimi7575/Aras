namespace Aras.Options;

public sealed class ArasTraderOptions
{
    public const string SectionName = "ArasTrader";
    public required string BaseUrl { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}
