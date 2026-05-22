using Aras.Application.Abstractions;
using Aras.Domain;
using Aras.External;

namespace Aras.Services;

public sealed class CustomerImportService(
    IArasTraderClient client,
    ICustomerRepository customers,
    IUnitOfWork unitOfWork) : ICustomerImportService
{
    public async Task<int> ImportAsync(CancellationToken cancellationToken)
    {
        var externalCustomers = await client.GetCustomersAsync(cancellationToken);
        var imported = 0;

        foreach (var item in externalCustomers.Where(x => !string.IsNullOrWhiteSpace(x.NationalCode)))
        {
            var customer = await customers.GetByNationalCodeAsync(item.NationalCode, cancellationToken);

            if (customer is null)
            {
                customer = new Customer
                {
                    NationalCode = item.NationalCode,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Wallet = new Wallet()
                };

                customers.Add(customer);
                imported++;
            }

            customer.FirstName = item.FirstName;
            customer.LastName = item.LastName;
            customer.FatherName = item.FatherName;
            customer.BirthCertificationNumber = item.BirthCertificationNumber;
            customer.RegisterationNumber = item.RegisterationNumber;
            customer.BirthDate = item.BirthDate;
            customer.BranchName = item.BranchName;
            customer.MobileNumber = item.MobileNumber;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return imported;
    }
}
