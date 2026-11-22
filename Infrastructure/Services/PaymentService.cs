using Application.Common.Interfaces;
using Application.ViewModels.Payment;
using MediatR;

namespace Infrastructure.Services
{
    // The payment-provider integration is not implemented. Never acknowledge
    // an unverified payment or keep provider credentials in this source file.
    public class PaymentService : IPaymentService
    {
        public PaymentService(ISender mediator)
        {
        }

        public Task<string> GenerateBuyUrl(BuyPlanViewMode model)
        {
            return Task.FromException<string>(new InvalidOperationException(
                "Payment integration is not configured."));
        }

        public Task<bool> VerifyBuy(string authority)
        {
            return Task.FromResult(false);
        }
    }
}
