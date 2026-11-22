namespace Application.ViewModels
{
    public class PaymentResponseViewModel
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public string Verify { get; set; }
        public Guid ReturnId { get; set; }
    }
}
