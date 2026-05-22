using Aras.Contracts;
using Aras.Domain;

namespace Aras.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken);
    void Add(Customer customer);
}
