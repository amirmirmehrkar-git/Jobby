using Application.Common.Interfaces;
using Application.ViewModels;
using Application.ViewModels.Payment;
using MediatR;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        protected ISender Mediator;
        //private string merchant_id = "";
        //private string _requestUrl = "https://api.zarinpal.com/pg/v4/payment/request.json";
        //private string _gateWayUrl = "https://www.zarinpal.com/pg/StartPay/";
        //private string _verifyUrl = "https://api.zarinpal.com/pg/v4/payment/verify.json";
        //private string PayStarMainURL = "https://core.paystar.ir";
        //private string gateway_id = "qxqkyg4xz35n1z";
        //private string signKey = "657470619E270C5F52CFA6847E898B955357BE884F2DCC67B6A7B1557D742E1C4D274C0A6BF1D80FBCF4F21552239BEE2E238512E3A0D8F039C751D1A69BE187C06965F2EAC0D3F50E4C0F0BA25F48106561F7C39C030D40584E38431334465B50734E22E3C31E067955BF17A28F50AF29CCE8A15511ABCE9F3A145DF052AC46";
        //private string callback = "https://nitroto.com/#!/callback/verifyPayment";

        string merchant = "5403bf52-00dd-4748-81d4-26d05171b911";
        string authority;
        string gatewayUrl = "";
        string refid = "";
        string callbackurl = "https://nitroto.com/#!/callback/verifyPayment";

        public PaymentService(ISender mediator)
        {
            Mediator = mediator;
        }

        public async Task<string> GenerateBuyUrl(BuyPlanViewMode model)
        {
            //var plan = Mediator.Send(new Application.CQRS.Plan_CQRS.Queries.Get.GetByTitleQuery(model.PlanTitle)).Result;

            //var amount = (plan.Price * 10).ToString();
            //var description = "خرید " + plan.Title;
            //var Parameters = new Application.ViewModels.Zarinpal.RequestParameters
            //    (merchant, amount, description, callbackurl, "", "");

            //var client = new RestClient(URLs.requestUrl);
            //Method method = Method.Post;
            //var request = new RestRequest("", method);
            //request.AddHeader("accept", "application/json");
            //request.AddHeader("content-type", "application/json");
            //request.AddJsonBody(Parameters);
            //var requestresponse = client.ExecuteAsync(request);
            //JObject jo = JObject.Parse(requestresponse.Result.Content);
            //string errorscode = jo["errors"].ToString();
            //JObject jodata = JObject.Parse(requestresponse.Result.Content);
            //string dataauth = jodata["data"].ToString();

            //if (dataauth != "[]")
            //{
            //    authority = jodata["data"]["authority"].ToString();

            //    gatewayUrl = URLs.gateWayUrl + authority;
            //}
            //else
            //{
            //    return "";
            //}

            //var planPlugin = Mediator.Send(new Application.CQRS.PlanPlugin_CQRS.Commands.Create.CreateCommand()
            //{
            //    Id = Guid.Empty,
            //    PlanId = plan.Id,
            //    UserId = model.UserId,
            //    //PluginIds = string.Join(",", model.Plugins.Select(m => m.Id))
            //    PluginIds = ""
            //}).Result;


            //var transaction = Mediator.Send(new Application.CQRS.Transaction_CQRS.Commands.Create.CreateCommand()
            //{
            //    Id = Guid.Empty,
            //    TotalPrice = plan.Price.Value,
            //    TransactionType = TransactionTypeDto.NotPaid,
            //    PlanPluginId = planPlugin,
            //    ZarinPalAuthority = authority,
            //    ZarinPalRefNum = ""
            //}).Result;

            //return authority;

            return null;
        }


        public async Task<bool> VerifyBuy(string Authority)
        {
            //var transaction = Mediator.Send(new GetByRefNumQuery(Authority)).Result;

            //var parameter = new VerifyParameters();
            //parameter.authority = Authority;
            //parameter.amount = (transaction.TotalPrice * 10).ToString();
            //parameter.merchant_id = merchant;

            //var client = new RestClient(URLs.verifyUrl);
            //Method method = Method.Post;
            //var request = new RestRequest("", method);

            //request.AddHeader("accept", "application/json");
            //request.AddHeader("content-type", "application/json");
            //request.AddJsonBody(parameter);
            //var response = client.ExecuteAsync(request);

            //JObject jodata = JObject.Parse(response.Result.Content);
            //string data = jodata["data"].ToString();
            //JObject jo = JObject.Parse(response.Result.Content);
            //string errors = jo["errors"].ToString();
            //if (data != "[]")
            //{
            //    refid = jodata["data"]["ref_id"].ToString();
            //}
            //else if (errors != "[]")
            //{
            //    string errorscode = jo["errors"]["code"].ToString();
            //}

            //var t = Mediator.Send(new Application.CQRS.Transaction_CQRS.Commands.Create.CreateCommand()
            //{
            //    Id = transaction.Id,
            //    TransactionType = TransactionTypeDto.Paid,
            //    ZarinPalRefNum = refid
            //}).Result;

            //var Case = Mediator.Send(new Application.CQRS.Case_CQRS.Commands.Create.CreateCommand()
            //{
            //    Id = Guid.Empty,
            //    TransactionId = transaction.Id,
            //    Description = ""
            //}).Result;

            return true;
        }
    }
}
