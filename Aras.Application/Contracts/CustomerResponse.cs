namespace Aras.Contracts;

public sealed record CustomerResponse(
    int Id,
    string NationalCode,
    string FirstName,
    string LastName,
    string? FatherName,
    string? BirthCertificationNumber,
    string? RegisterationNumber,
    DateTime? BirthDate,
    string? BranchName,
    string? MobileNumber,
    decimal WalletBalance);
