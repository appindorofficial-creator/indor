using IndorMvcApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

public class SharedQuoteController(RealtorSharedQuoteService sharedQuoteService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> View(Guid id, CancellationToken cancellationToken)
    {
        var model = await sharedQuoteService.BuildHomeownerViewAsync(id, cancellationToken);
        return model == null ? NotFound() : View("View", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        var accepted = await sharedQuoteService.AcceptHomeownerAsync(id, cancellationToken);
        return accepted
            ? RedirectToAction(nameof(View), new { id })
            : NotFound();
    }
}
