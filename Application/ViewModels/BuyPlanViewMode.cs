namespace Application.ViewModels
{
    public class BuyPlanViewMode
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public double TotalPrice { get; set; }
        public string Title { get; set; }
        public string CallbackUrl { get; set; }
    }
}
