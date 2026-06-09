using Domain.Entities;
using Domain.Enums;
using HtmlAgilityPack;
using Microsoft.Playwright;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebAPI.ViewModels;
using WebUI.Helper;

namespace WebUI.Services
{
    public class Apply
    {
        private int Count = 4;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAiKey;
        private readonly string _chatModel;
        private readonly string _jobTitle;
        private readonly string _playwrightPath;
        public Apply(IHttpClientFactory httpClientFactory, string chatModel, string openAiKey,string jobTitle,string playwrightPath)
        {
            _httpClientFactory = httpClientFactory;
            _openAiKey = openAiKey;
            _chatModel = chatModel;
            _jobTitle = jobTitle;
            _playwrightPath = playwrightPath;
        }
        public async Task<List<JobInfoViewModel>> GetLinkedInJobs()
        {
            var li = new List<JobInfoViewModel>();

            try
            {
                var url = $"https://www.linkedin.com/jobs/search/?keywords={Uri.EscapeDataString(_jobTitle)}";

                // راه‌اندازی Playwright
                using var playwright = await Playwright.CreateAsync();

                // اجرای مرورگر در حالت headless
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    ExecutablePath = _playwrightPath,
                    Headless = true,
                    Args = new[]
                    {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--disable-software-rasterizer"
        }
                });

                // باز کردن یک تب جدید
                var context = await browser.NewContextAsync();
                var page = await context.NewPageAsync();

                // رفتن به صفحه مورد نظر
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 120000
                });

                // صبر برای لود شدن کارت‌های شغلی
                await page.WaitForSelectorAsync("div.base-card");

                // دریافت HTML صفحه
                string html = await page.ContentAsync();

                // تجزیه HTML با HtmlAgilityPack
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var jobCards = doc.DocumentNode.SelectNodes("//div[contains(@class,'base-card')]");

                int index = 0;
                foreach (var card in jobCards)
                {
                    if (index == Count)
                        break;

                    var title = card.SelectSingleNode(".//h3[contains(@class,'base-search-card__title')]")?.InnerText.Trim();
                    var company = card.SelectSingleNode(".//h4[contains(@class,'base-search-card__subtitle')]")?.InnerText.Trim();
                    var location = card.SelectSingleNode(".//span[contains(@class,'job-search-card__location')]")?.InnerText.Trim();
                    var link = card.SelectSingleNode(".//a[contains(@class,'base-card__full-link')]")?.GetAttributeValue("href", "");

                    li.Add(new JobInfoViewModel
                    {
                        Company = company,
                        Link = link,
                        Location = location,
                        Title = title,
                        JobType = JobType.LinkedIn
                    });

                    index++;
                }

                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                //return Problem(ex.Message);
            }

            return li;
        }
        public async Task<List<JobInfoViewModel>> GetXingJobs()
        {
            var li = new List<JobInfoViewModel>();

            try
            {
                var url = $"https://www.xing.com/jobs/search?keywords={Uri.EscapeDataString(_jobTitle)}";

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    ExecutablePath = _playwrightPath,
                    Headless = true,
                    Args = new[]
                      {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--disable-software-rasterizer"
        }
                });

                var context = await browser.NewContextAsync();
                var page = await context.NewPageAsync();

                // Navigate to page
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 120000
                });

                // Wait for page ready (equivalent to your WaitForPageReadySafe)
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Wait for job cards selector
                await page.WaitForSelectorAsync("article[data-testid='job-search-result']");

                // Collect job links
                var jobElements = await page.QuerySelectorAllAsync("article[data-testid='job-search-result']");

                int index = 0;
                foreach (var jobElement in jobElements)
                {
                    if (index == Count)
                        break;

                    try
                    {
                        var linkElement = await jobElement.QuerySelectorAsync("a.card-styles__CardLink-sc-313de809-2");
                        var titleElement = await jobElement.QuerySelectorAsync("h2[data-testid='job-teaser-list-title']");
                        var companyElement = await jobElement.QuerySelectorAsync("p.job-teaser-list-item-styles__Company-sc-f66c105-8");
                        var cityElement = await jobElement.QuerySelectorAsync("p.job-teaser-list-item-styles__City-sc-f66c105-9");

                        if (linkElement == null || titleElement == null || companyElement == null || cityElement == null)
                            continue;

                        var urlAttr = await linkElement.GetAttributeAsync("href");
                        if (!string.IsNullOrEmpty(urlAttr) && !urlAttr.StartsWith("http"))
                            urlAttr = "https://www.xing.com" + urlAttr; // در صورت لینک نسبی

                        li.Add(new JobInfoViewModel
                        {
                            Title = (await titleElement.InnerTextAsync()).Trim(),
                            Company = (await companyElement.InnerTextAsync()).Trim(),
                            Location = (await cityElement.InnerTextAsync()).Trim(),
                            Link = urlAttr,
                            JobType = JobType.Xing
                        });

                        index++;
                    }
                    catch
                    {
                        continue;
                    }
                }

                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                //return Problem(ex.Message);
            }

            return li;
        }
        public async Task<List<JobInfoViewModel>> GetStepStoneJobs()
        {
            var li = new List<JobInfoViewModel>();

            try
            {
                var url = $"https://www.stepstone.de/work/{Uri.EscapeDataString(_jobTitle)}?searchOrigin=Homepage_top-search";

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    ExecutablePath = _playwrightPath,
                    Headless = true,
                    Args = new[]
                      {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--disable-software-rasterizer"
        }
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = null
                });

                var page = await context.NewPageAsync();

                // Navigate to page
                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 60000
                });

                await page.EvaluateAsync("() => window.scrollBy(0, window.innerHeight)");
                await Task.Delay(700);

                await page.WaitForSelectorAsync("article[data-at='job-item'], article[data-testid='job-item']");

                var articles = await page.QuerySelectorAllAsync("article[data-at='job-item'], article[data-testid='job-item'], article.res-4cwuay");

                foreach (var art in articles)
                {
                    try
                    {
                        var job = await StepstoneHelpers.ParseStepstoneJobCardAsync(art);

                        if (!string.IsNullOrEmpty(job.Link) && job.Link.StartsWith("/"))
                        {
                            job.Link = new Uri(new Uri(page.Url), job.Link).ToString();
                        }

                        li.Add(job);

                        if (li.Count >= Count) break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error parsing job card: {ex.Message}");
                    }
                }

                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                //return Problem(ex.Message);
            }

            return li;
        }

        //        public async Task<List<JobInfoViewModel>> GenerateCoverLetter
        //            (List<JobInfoViewModel> Jobs, ResumeResult ResumeResult, ApplicationUser User)
        //        {
        //            var li = new List<JobInfoViewModel>();

        //            try
        //            {
        //                foreach (var item in Jobs)
        //                {
        //                    var coverLetter = "";

        //                    var prompt = $@"
        //You are a professional **cover letter writer** and career communication expert.
        //Your goal is to write a compelling, personalized, and ATS-friendly **cover letter** based on the information provided below.

        //Carefully analyze and use:
        //1. **Company information**:
        //   - Company Name: {item.Company}
        //   - Company Location: {item.Location}
        //   - Job Title: {item.Title}
        //   - Assume the recipient is 'Dear Manager' (no specific name).
        //   - Research the company's mission, vision, culture, and future plans.
        //   - Research this job title on the O*NET website to identify relevant skills, responsibilities, and qualifications.

        //2. **Applicant’s information**:
        //   - Full Name: {User.FirstName} {User.LastName}
        //   - Address: {User.Address}
        //   - Email: {User.Email}
        //   - Phone Number: {User.PhoneNumber}
        //   - Skills: {string.Join(", ", ResumeResult.Skills.Select(m => m.Title))}
        //   - Work Experiences: {string.Join(", ", ResumeResult.Experiences.Select(m => $"Role: {m.Role} - Company: {m.Company} - Project: {m.Project}"))}
        //   - Education: {string.Join(", ", ResumeResult.Educations.Select(m => $"Academy: {m.Academy} - Degree: {m.Degree}"))}
        //   - Certificates: {string.Join(", ", ResumeResult.Certificates.Select(m => $"Academy: {m.Academy} - Skill: {m.Skill}"))}

        //3. **Writing rules**:
        //   - Use the **exact job title** at the top of the letter.
        //   - Address it to **Dear Manager**.
        //   - Write naturally and persuasively in English.
        //   - Focus on how the applicant’s skills, experience, and education align with the company’s goals, culture, and the job’s responsibilities.
        //   - Keep the tone **professional, confident, and authentic**.
        //   - Structure it with clear paragraphs:
        //     - **Opening:** Express interest and mention the exact position.
        //     - **Middle:** Highlight relevant experience, skills, and achievements.
        //     - **Closing:** Emphasize enthusiasm and readiness for the role.
        //   - Do **not** include the applicant's address or date in the header.
        //   - Output should be **a complete English cover letter only** — no explanations, no formatting notes, no extra text before or after.

        //Return only the **final cover letter text**, ready to be sent.
        //";

        //                    using var client = _httpClientFactory.CreateClient();
        //                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

        //                    var bodyObj = new
        //                    {
        //                        model = _chatModel,
        //                        messages = new[]
        //                        {
        //                            new { role="user", content = prompt }
        //                        },
        //                        temperature = 0.0,
        //                        max_tokens = 1200
        //                    };

        //                    var bodyJson = JsonSerializer.Serialize(bodyObj);
        //                    using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

        //                    var resp = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        //                    var respText = await resp.Content.ReadAsStringAsync();

        //                    if (!resp.IsSuccessStatusCode)
        //                        throw new Exception($"Chat API error: {resp.StatusCode} / {respText}");

        //                    using var doc = JsonDocument.Parse(respText);
        //                    coverLetter = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        //                    li.Add(new JobInfoViewModel
        //                    {
        //                        Title = item.Title,
        //                        Company = item.Company,
        //                        Location = item.Location,
        //                        Link = item.Link,
        //                        Type = item.Type,
        //                        CoverLetter = coverLetter
        //                    });
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //return Problem(ex.Message);
        //            }

        //            return li;
        //        }
        public async Task<List<JobInfoViewModel>> GenerateCoverLetter(List<JobInfoViewModel> Jobs, ResumeResult ResumeResult, ApplicationUser User)
        {
            var li = new List<JobInfoViewModel>();

            if (Jobs == null || Jobs.Count == 0)
                return li;

            try
            {
                var jobsBlockBuilder = new StringBuilder();
                for (int i = 0; i < Jobs.Count; i++)
                {
                    var j = Jobs[i];
                    jobsBlockBuilder.AppendLine($"{{");
                    jobsBlockBuilder.AppendLine($"  \"Index\": {i},");
                    jobsBlockBuilder.AppendLine($"  \"Title\": \"{EscapeForJson(j.Title)}\",");
                    jobsBlockBuilder.AppendLine($"  \"Company\": \"{EscapeForJson(j.Company)}\",");
                    jobsBlockBuilder.AppendLine($"  \"Location\": \"{EscapeForJson(j.Location)}\",");
                    jobsBlockBuilder.AppendLine($"  \"Link\": \"{EscapeForJson(j.Link)}\",");
                    jobsBlockBuilder.AppendLine($"  \"JobType\": \"{EscapeForJson(Convert.ToInt32(j.JobType).ToString())}\"");
                    jobsBlockBuilder.AppendLine($"}},");
                }
                var jobsBlock = jobsBlockBuilder.ToString().TrimEnd('\r', '\n', ',');
                var skills = ResumeResult?.Skills != null
                    ? string.Join(", ", ResumeResult.Skills.Select(s => s.Title))
                    : "";
                var experiences = ResumeResult?.Experiences != null
                    ? string.Join(" | ", ResumeResult.Experiences.Select(e =>
                        $"Role: {e.Role}; Company: {e.Company}; Project: {e.Project}"))
                    : "";
                var educations = ResumeResult?.Educations != null
                    ? string.Join(" | ", ResumeResult.Educations.Select(ed =>
                        $"Academy: {ed.Academy}; Degree: {ed.Degree}; Score: {ed.Score}"))
                    : "";
                var certificates = ResumeResult?.Certificates != null
                    ? string.Join(" | ", ResumeResult.Certificates.Select(c =>
                        $"Academy: {c.Academy}; Title: {c.Title ?? ""}; Skill: {c.Skill}"))
                    : "";

                var prompt = $@"
You are a professional cover letter writer and career communication expert.
You must respond in English only.
Input Data:
- Applicant:
  FullName: ""{EscapeForJson(User.FirstName + " " + User.LastName)}""
  Address: ""{EscapeForJson(User.Address)}""
  Email: ""{EscapeForJson(User.Email)}""
  Phone: ""{EscapeForJson(User.PhoneNumber)}""

- Resume (compact):
  Skills: ""{EscapeForJson(skills)}""
  Experiences: ""{EscapeForJson(experiences)}""
  Educations: ""{EscapeForJson(educations)}""
  Certificates: ""{EscapeForJson(certificates)}""

- Jobs: an array of job objects (Indexed). Use the jobs list below exactly as given.
[
{jobsBlock}
]

Task:
For each job object above produce a tailored, professional, ATS-friendly cover letter in English.
Return the result as a JSON array. Each array element must be an object with the exact fields:
- Title (string)
- Company (string)
- Location (string)
- Link (string)
- JobType (int)
- CoverLetter (string)  <-- the full cover letter text for that specific job

Strict output rules:
- Return ONLY valid JSON; no explanations, no extra text, no markdown fences.
- The JSON must be an array with one object per input job (preserve Index ordering).
- Use the exact job Title and Company values in the cover letter where appropriate.
- Address each letter to ""Dear Manager"".
- Do NOT include applicant address or date in header.
- Keep tone professional, confident, and concise. Each CoverLetter should be ~180-350 words tailored to the job.
- If any input field is empty, still return the field with empty string.
- Do not translate or localize names or terms.

Now produce the JSON array.
";

                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

                var bodyObj = new
                {
                    model = _chatModel,
                    messages = new[]
                    {
                new { role="user", content = prompt }
            },
                    temperature = 0.0,
                    max_tokens = 3500
                };

                var bodyJson = JsonSerializer.Serialize(bodyObj);
                using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var respText = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"Chat API error: {resp.StatusCode} / {respText}");

                using var doc = JsonDocument.Parse(respText);
                var raw = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

                var jsonOnly = StripJsonFromMarkdown(raw);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var results = JsonSerializer.Deserialize<List<JobInfoViewModel>>(jsonOnly, options);
                if (results == null)
                    throw new Exception("Failed to deserialize cover letters JSON from model response. Raw response: " + raw);

                foreach (var res in results)
                {
                    li.Add(new JobInfoViewModel
                    {
                        Title = res.Title ?? "",
                        Company = res.Company ?? "",
                        Location = res.Location ?? "",
                        Link = res.Link ?? "",
                        JobType = res.JobType,
                        CoverLetter = res.CoverLetter ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return li;
        }

        private static string StripJsonFromMarkdown(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var trimmed = input.Trim();

            // if wrapped in triple backticks, extract inner content
            if (trimmed.StartsWith("```"))
            {
                var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
                if (lastFence > 3)
                {
                    var inner = trimmed.Substring(3, lastFence - 3).Trim();
                    return inner;
                }
            }

            // sometimes model returns JSON prefixed with language like ```json
            var tripleJsonFence = "```json";
            if (trimmed.StartsWith(tripleJsonFence))
            {
                var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
                if (lastFence > tripleJsonFence.Length)
                {
                    var inner = trimmed.Substring(tripleJsonFence.Length, lastFence - tripleJsonFence.Length).Trim();
                    return inner;
                }
            }

            // else return as-is
            return trimmed;
        }
        private static string EscapeForJson(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
        }

    }
}
