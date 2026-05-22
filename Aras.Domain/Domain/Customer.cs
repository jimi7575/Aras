namespace Aras.Domain;

public sealed class Customer
{
    public int Id { get; set; }
    public required string NationalCode { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? FatherName { get; set; }
    public string? BirthCertificationNumber { get; set; }
    public string? RegisterationNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? BranchName { get; set; }
    public string? MobileNumber { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public List<Order> Orders { get; set; } = [];
}
