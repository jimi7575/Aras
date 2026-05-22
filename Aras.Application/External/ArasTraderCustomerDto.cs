using System.Text.Json.Serialization;

namespace Aras.External;

public sealed class ArasTraderCustomerDto
{
    [JsonPropertyName("nationalCode")]
    public required string NationalCode { get; set; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; set; }

    [JsonPropertyName("fatherName")]
    public string? FatherName { get; set; }

    [JsonPropertyName("birthCertificationNumber")]
    public string? BirthCertificationNumber { get; set; }

    [JsonPropertyName("registerationNumber")]
    public string? RegisterationNumber { get; set; }

    [JsonPropertyName("birthDate")]
    public DateTime? BirthDate { get; set; }

    [JsonPropertyName("branchName")]
    public string? BranchName { get; set; }

    [JsonPropertyName("mobileNumber")]
    public string? MobileNumber { get; set; }
}
