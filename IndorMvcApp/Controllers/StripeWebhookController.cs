using IndorMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/stripe")]
public class StripeWebhookController(
    IInsuranceStripeCheckoutService insuranceStripeCheckout,
    ILogger<StripeWebhookController> logger) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            var ok = await insuranceStripeCheckout.TryHandleWebhookAsync(json, signature, cancellationToken);
            if (!ok)
            {
                return BadRequest();
            }

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe webhook processing failed.");
            return StatusCode(500);
        }
    }
}
