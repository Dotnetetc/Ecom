using API.Errors;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    public class PaymentsController : BaseApiController
    {

        private const string  WhSecret = "whsec_accf048f6d571e3def6a06da6c9509f6533e40b58291c0f175a072c4548cfb00";
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("{basketId}")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket =  await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if(basket==null) return  BadRequest(new ApiResponse(400,"Problem with your basket"));

            return basket;
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json,
            Request.Headers["Stripe-Signature"],WhSecret,throwOnApiVersionMismatch: false);

            PaymentIntent intent;
            Order order;

            switch(stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    intent =(PaymentIntent) stripeEvent.Data.Object;
                    _logger.LogInformation("Payment succeeded: ", intent.Id);
                    order = await _paymentService.UpdateOrderPaymentSucceed(intent.Id);
                    _logger.LogInformation("Order updated to payment received", order.Id);
                    break;
                case "payment_intent.payment_failed":
                intent =(PaymentIntent) stripeEvent.Data.Object;
                _logger.LogInformation("Payment failed: ", intent.Id);
                order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                    _logger.LogInformation("Order updated to payment failed", order.Id);
                //todo : update the order with new status
                break;
               
            }

            return new EmptyResult();


        }

    }
}