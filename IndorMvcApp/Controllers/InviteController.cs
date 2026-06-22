using IndorMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

[AllowAnonymous]
[Route("invite")]
public class InviteController(IRealtorInviteClientService inviteService) : Controller
{
    [HttpGet("{token:guid}")]
    public async Task<IActionResult> Index(Guid token, CancellationToken cancellationToken)
    {
        var vm = await inviteService.GetPublicInvitationAsync(token, cancellationToken);
        if (vm == null)
        {
            return View("NotFound");
        }

        if (vm.AlreadyAccepted)
        {
            return View("Accepted", vm);
        }

        return View(vm);
    }

    [HttpPost("{token:guid}/accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid token, CancellationToken cancellationToken)
    {
        var vm = await inviteService.AcceptInvitationAsync(token, cancellationToken);
        if (vm == null)
        {
            return View("NotFound");
        }

        return View("Accepted", vm);
    }
}
