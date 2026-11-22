namespace Application.ViewModels
{
    public class ResponseDataViewModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public string authority { get; set; }
        public string fee_type { get; set; }
        public decimal fee { get; set; }
    }
}
