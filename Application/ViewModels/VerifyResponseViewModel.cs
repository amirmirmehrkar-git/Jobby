namespace Application.ViewModels
{
    public class VerifyResponseViewModel
    {
        public int Code { get; set; }
        public int Ref_id { get; set; }
        public string Card_pan { get; set; }
        public string Card_hash { get; set; }
        public string Fee_type { get; set; }
        public int Fee { get; set; }
        public string Message { get; set; }
    }
}
