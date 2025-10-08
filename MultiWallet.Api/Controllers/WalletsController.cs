using Microsoft.AspNetCore.Mvc;
using MultiWallet.Api.Models;
using MultiWallet.Api.Requests;
using MultiWallet.Api.Services;

namespace MultiWallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletManagementService _walletService;

    public WalletsController(IWalletManagementService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost]
    public async Task<ActionResult<Wallet>> CreateWallet(CreateWalletRequest createWalletRequest)
    {
        var wallet = await _walletService.CreateWalletAsync(createWalletRequest.WalletName, createWalletRequest.Currencies);
        return Created($"/wallets/{wallet.Id}", wallet);
    }

    [HttpGet]
    public async Task<ActionResult<List<Wallet>>> GetWallets()
    {
        var wallets = await _walletService.GetWalletsAsync();
        return Ok(wallets);
    }

    [HttpGet("{walletId}")]
    public async Task<ActionResult<Wallet>> GetWallet(int walletId)
    {
        var wallet = await _walletService.GetWalletAsync(walletId);
        if (wallet == null)
        {
            return NotFound($"Nie znaleziono portfela o id {walletId}");
        }

        return Ok(wallet);
    }
}