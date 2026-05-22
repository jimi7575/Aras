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
                customer = new Customer(item.NationalCode, item.FirstName, item.LastName);

                customers.Add(customer);
                imported++;
            }

            customer.UpdateProfile(
                item.FirstName,
                item.LastName,
                item.FatherName,
                item.BirthCertificationNumber,
                item.RegisterationNumber,
                item.BirthDate,
                item.BranchName,
                item.MobileNumber);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return imported;
    }
}
