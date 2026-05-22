using Aras.Application.Abstractions;
using Aras.Contracts;
using Aras.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aras.Infrastructure.Data;

public sealed class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken)
    {
        return db.Customers
            .Include(x => x.Wallet)
            .FirstOrDefaultAsync(x => x.NationalCode == nationalCode, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return db.Customers.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.Customers
            .Include(x => x.Wallet)
            .OrderBy(x => x.Id)
            .Select(x => new CustomerResponse(
                x.Id,
                x.NationalCode,
                x.FirstName,
                x.LastName,
                x.FatherName,
                x.BirthCertificationNumber,
                x.RegisterationNumber,
                x.BirthDate,
                x.BranchName,
                x.MobileNumber,
                x.Wallet.Balance))
            .ToListAsync(cancellationToken);
    }

    public void Add(Customer customer)
    {
        db.Customers.Add(customer);
    }
}
