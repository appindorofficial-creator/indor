using IndorMvcApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

public class SharedQuoteController(RealtorSharedQuoteService sharedQuoteService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> View(Guid token, CancellationToken cancellationToken)
    {
        var model = await sharedQuoteService.BuildHomeownerViewAsync(token, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid token, CancellationToken cancellationToken)
    {
        var accepted = await sharedQuoteService.AcceptHomeownerAsync(token, cancellationToken);
        return accepted
            ? RedirectToAction(nameof(View), new { token })
            : NotFound();
    }
}
