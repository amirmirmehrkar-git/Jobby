using Application.ViewModels;

namespace Application.Common.Interfaces
{
    public interface IPaymentService
    {
        Task<string> GenerateBuyUrl(BuyPlanViewMode model);
        Task<bool> VerifyBuy(string Authority);
    }
}
