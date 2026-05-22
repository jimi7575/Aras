namespace Aras.Domain;

public sealed class Customer
{
    private Customer()
    {
    }

    public Customer(string nationalCode, string firstName, string lastName)
    {
        NationalCode = nationalCode;
        FirstName = firstName;
        LastName = lastName;
        Wallet = new Wallet();
    }

    public int Id { get; private set; }
    public string NationalCode { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? FatherName { get; private set; }
    public string? BirthCertificationNumber { get; private set; }
    public string? RegisterationNumber { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? BranchName { get; private set; }
    public string? MobileNumber { get; private set; }
    public Wallet Wallet { get; private set; } = null!;
    public List<Order> Orders { get; private set; } = [];

    public void UpdateProfile(
        string firstName,
        string lastName,
        string? fatherName,
        string? birthCertificationNumber,
        string? registerationNumber,
        DateTime? birthDate,
        string? branchName,
        string? mobileNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        FatherName = fatherName;
        BirthCertificationNumber = birthCertificationNumber;
        RegisterationNumber = registerationNumber;
        BirthDate = birthDate;
        BranchName = branchName;
        MobileNumber = mobileNumber;
    }
}
