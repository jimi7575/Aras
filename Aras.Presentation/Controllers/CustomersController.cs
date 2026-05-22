using Aras.Application.Abstractions;
using Aras.Contracts;
using Aras.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aras.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(ICustomerRepository customers, ICustomerImportService importService) : ControllerBase
{
    [HttpPost("sync")]
    public async Task<ActionResult<object>> Sync(CancellationToken cancellationToken)
    {
        var imported = await importService.ImportAsync(cancellationToken);
        return Ok(new { imported });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await customers.GetAllAsync(cancellationToken));
    }
}
