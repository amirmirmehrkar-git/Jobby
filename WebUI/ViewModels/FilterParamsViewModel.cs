using System.ComponentModel.DataAnnotations;

namespace WebAPI.ViewModels
{
    public class FilterParamsViewModel
    {
        [Display(Name = "اندازه صفحه")]
        [Required(ErrorMessage = "وارد کردن '{0}' الزامی است.")]
        public int PageSize { get; set; }
        [Display(Name = "شماره صفحه")]
        [Required(ErrorMessage = "وارد کردن '{0}' الزامی است.")]
        public int PageCount { get; set; }
        public Dictionary<string, string[]> Filter { get; set; }
        public Dictionary<string, string> Sorting { get; set; }
        public Dictionary<string, string> Group { get; set; }
    }
}
