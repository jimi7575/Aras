using Aras.Contracts;
using Aras.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aras.Controllers;

[ApiController]
[Route("api/wallets")]
public sealed class WalletsController(IWalletService wallets) : ControllerBase
{
    [HttpGet("{customerId:int}")]
    public async Task<ActionResult<WalletResponse>> Get(int customerId, CancellationToken cancellationToken)
    {
        var wallet = await wallets.GetByCustomerIdAsync(customerId, cancellationToken);

        return wallet is null ? NotFound() : Ok(wallet);
    }
}
