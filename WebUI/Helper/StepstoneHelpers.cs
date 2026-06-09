using Domain.Enums;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using WebAPI.ViewModels;

namespace WebUI.Helper
{
    public class StepstoneHelpers
    {
        private static string NormalizeText(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return Regex.Replace(s.Trim(), @"\s+", " ");
        }

        private static async Task<string> QueryInnerTextAsync(IElementHandle ctx, string selector)
        {
            try
            {
                var el = await ctx.QuerySelectorAsync(selector);
                if (el == null) return null;
                var txt = await el.InnerTextAsync();
                return NormalizeText(txt);
            }
            catch { return null; }
        }
        private static async Task<string> QueryAttributeAsync(IElementHandle ctx, string selector, string attribute)
        {
            try
            {
                var el = await ctx.QuerySelectorAsync(selector);
                if (el == null) return null;
                var attr = await el.GetAttributeAsync(attribute);
                return NormalizeText(attr);
            }
            catch { return null; }
        }

        public static async Task<JobInfoViewModel> ParseStepstoneJobCardAsync(IElementHandle article)
        {
            var vm = new JobInfoViewModel();

            string href = null;
            try
            {
                href = await QueryAttributeAsync(article, "a[data-testid='job-item-title']", "href");
                if (string.IsNullOrWhiteSpace(href))
                {
                    href = await QueryAttributeAsync(article, "h2 a[href]", "href");
                }
                if (string.IsNullOrWhiteSpace(href))
                {
                    href = await QueryAttributeAsync(article, "a.res-1fudl87, a[data-genesis-element='COMPANY_LOGO_LINK']", "href");
                }
                vm.Link = href;
            }
            catch { vm.Link = null; }

            string title = null;
            try
            {
                title = await QueryInnerTextAsync(article, "a[data-testid='job-item-title']");
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = await QueryInnerTextAsync(article, "h2 a");
                }
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = await QueryInnerTextAsync(article, ".res-kjzl78, .res-ewgtgq, [data-at='jobcard-content']");
                }
                vm.Title = title ?? "(No title)";
            }
            catch { vm.Title = "(No title)"; }

            string company = null;
            try
            {
                company = await QueryInnerTextAsync(article, "span[data-at='job-item-company-name']");
                if (string.IsNullOrWhiteSpace(company))
                {
                    company = await QueryInnerTextAsync(article, ".res-ewgtgq[data-genesis-element='BASE'] .res-du9bhi, .res-du9bhi, .res-b5ez85 [alt]");
                }
                if (string.IsNullOrWhiteSpace(company))
                {
                    company = await QueryAttributeAsync(article, "a[data-at='company-logo'] img, a.res-r8bhfo img", "alt");
                }
                if (string.IsNullOrWhiteSpace(company))
                {
                    company = await QueryInnerTextAsync(article, ".company, .employer, .job-card-company");
                }
                vm.Company = company;
            }
            catch { vm.Company = null; }

            string location = null;
            try
            {
                location = await QueryInnerTextAsync(article, "span[data-at='job-item-location']");
                if (string.IsNullOrWhiteSpace(location))
                {
                    location = await QueryInnerTextAsync(article, ".res-12jlzgf [data-at='job-item-location'], .res-12jlzgf .res-du9bhi, .job-location, .location");
                }
                vm.Location = location;
            }
            catch { vm.Location = null; }

            vm.Title = NormalizeText(vm.Title);
            vm.Company = NormalizeText(vm.Company);
            vm.Location = NormalizeText(vm.Location);
            vm.Link = NormalizeText(vm.Link);
            vm.JobType = JobType.Stepstone;
            vm.CoverLetter = "";

            return vm;
        }
    }
}
