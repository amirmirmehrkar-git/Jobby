using Application.Common.Interfaces;
using Application.CQRS.Suggest_CQRS;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using SkiaSharp;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebUI.Helper;
using static WebUI.ViewModels.SendApplicationViewModel.SendApplicationViewModel;

namespace WebUI.Services
{
    public class SendApplication
    {
        IApplicationDbContext DbContext;
        ApplicationUser User;
        SendApplicationCommand Command;
        Suggest Suggest;
        string Key;
        string PlaywrightPath;
        string WebRootPath;
        int DelayTime = 60000;

        string LinkedInUserName, LinkedInPassword, StepStoneUserName, StepStonePassword;

        public SendApplication(IApplicationDbContext _DbContext, ApplicationUser _User, SendApplicationCommand _Command, string _Key, string _PlaywrightPath, string _WebRootPath, Suggest _Suggest)
        {
            DbContext = _DbContext;
            User = _User;
            Command = _Command;
            Key = _Key;
            PlaywrightPath = _PlaywrightPath;
            WebRootPath = _WebRootPath;
            Suggest = _Suggest;
        }

        public async Task Send()
        {
            #region پیش نیازها
            LinkedInUserName = User.Linkdin.Split(',')[0];
            LinkedInPassword = User.Linkdin.Split(',')[1];
            StepStoneUserName = User.Stepstone.Split(',')[0];
            StepStonePassword = User.Stepstone.Split(',')[1];

            IPlaywright playwright = playwright = await Playwright.CreateAsync();
            var userDataDir = "";
            #endregion

            if (Command.Link.ToLower().Contains("stepstone"))
            {
                userDataDir = $@"C:\PlaywrightProfiles\Stepstone\" + StepStoneUserName;

                var browser = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions()
                {
                    ExecutablePath = PlaywrightPath,
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
                var page = await browser.NewPageAsync();

                await page.GotoAsync(Command.Link, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = DelayTime });
                await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = DelayTime });

                #region انجام لاگین
                try
                {
                    try
                    {
                        var elemSelectorCookie = "button:has-text('Accept All'), button:has-text('Alles akzeptieren')";
                        await page.Locator(elemSelectorCookie).WaitForAsync();
                        await page.Locator(elemSelectorCookie).ClickAsync();
                    }
                    catch
                    { }

                    var elemSelector0 = "input[type='email'][name='email'][data-testid='email-input']";
                    await page.Locator(elemSelector0).WaitForAsync();
                    await page.Locator(elemSelector0).FillAsync(StepStoneUserName);

                    var elemSelector1 = "button[data-genesis-element='BUTTON'][aria-label='Continue button']" +
                        "[data-testid='rlac-primary-continue-btn'][data-dd-action-name='rla_forms_twoStep_continueWithEmailClick']";
                    await page.Locator(elemSelector1).WaitForAsync();
                    await page.Locator(elemSelector1).ClickAsync();

                    var elemSelector2 = "input[type='password'][name='password'][data-testid='password-input']";
                    await page.Locator(elemSelector2).WaitForAsync();
                    await page.Locator(elemSelector2).FillAsync(StepStonePassword);

                    var elemSelector3 = "";
                    try
                    {
                        elemSelector3 = "button[data-testid='register-submit-btn'][data-dd-action-name='rla_forms_twoStep_registerWithEmailPasswordClick'][aria-label='Log in button']";
                        await page.Locator(elemSelector3).WaitForAsync();
                        await page.Locator(elemSelector3).ClickAsync();
                    }
                    catch { }

                    elemSelector3 = "button[data-testid='login-submit-btn'][data-dd-action-name='rla_forms_twoStep_loginWithEmailPasswordClick'][aria-label='Log in button']";
                    await page.Locator(elemSelector3).WaitForAsync();
                    await page.Locator(elemSelector3).ClickAsync();

                    await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = DelayTime });
                    await Task.Delay(2000);
                }
                catch { }
                #endregion

                try
                {
                    var elemSelectorImInterested = "button[data-testid='harmonised-apply-button'][data-genesis-element='BUTTON']";
                    await page.Locator(elemSelectorImInterested).Nth(0).WaitForAsync(new LocatorWaitForOptions { Timeout = DelayTime });
                    await page.Locator(elemSelectorImInterested).Nth(0).ClickAsync();
                }
                catch
                { }

                await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = DelayTime });
                await Task.Delay(2000);
                page.WaitForNavigationAsync(new PageWaitForNavigationOptions()
                {
                    WaitUntil = WaitUntilState.Load,
                    Timeout = 600_000
                });

                var inputSelector = "input[type='text'][name='fullName.firstName'][id='genesis-form-element-:r1h:']";
                try
                {
                    await page.Locator(inputSelector).Nth(0).WaitForAsync();
                    await page.Locator(inputSelector).Nth(0).FillAsync(User.FirstName);
                }
                catch { }
                try
                {
                    inputSelector = "input[type='text'][name='fullName.lastName'][id='genesis-form-element-:r1i:']";
                    await page.Locator(inputSelector).Nth(0).WaitForAsync();
                    await page.Locator(inputSelector).Nth(0).FillAsync(User.LastName);
                }
                catch { }
                try
                {
                    inputSelector = "input[type='tel'][name='phoneNumber.number'][id='genesis-form-element-:r1n:-input']";
                    await page.Locator(inputSelector).Nth(0).WaitForAsync();
                    await page.Locator(inputSelector).Nth(0).FillAsync(User.PhoneNumber);
                }
                catch { }

                var elemSelectorContinue = "button[aria-label='Continue'][data-testid='sectionSubmit'][data-genesis-element='BUTTON']";
                try
                {
                    await page.Locator(elemSelectorContinue).Nth(0).WaitForAsync(new LocatorWaitForOptions { Timeout = DelayTime });
                    await page.Locator(elemSelectorContinue).Nth(0).ClickAsync();
                }
                catch
                { }

                await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = DelayTime });
                await Task.Delay(2000);
                page.WaitForNavigationAsync(new PageWaitForNavigationOptions()
                {
                    WaitUntil = WaitUntilState.Load,
                    Timeout = 600_000
                });

                try
                {
                    inputSelector = "input[type='file'][data-testid='upload-input'][style='display: none;']";
                    await page.Locator(inputSelector).Nth(0).SetInputFilesAsync("C:\\Resume.pdf");
                    await Task.Delay(10000);
                }
                catch { }

                try
                {
                    await page.Locator(elemSelectorContinue).Nth(0).ClickAsync();
                }
                catch
                { }

                await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = DelayTime });
                await Task.Delay(2000);
                page.WaitForNavigationAsync(new PageWaitForNavigationOptions()
                {
                    WaitUntil = WaitUntilState.Load,
                    Timeout = 600_000
                });

                try
                {
                    var elemSelectorFinall = "button[aria-label='Send application'][data-testid='sendApplication'][data-genesis-element='BUTTON']";
                    await page.Locator(elemSelectorFinall).Nth(0).WaitForAsync(new LocatorWaitForOptions { Timeout = DelayTime });
                    await page.Locator(elemSelectorFinall).Nth(0).ClickAsync();
                }
                catch
                { }
            }
            else
            {
                userDataDir = $@"C:\PlaywrightProfiles\LinkedIn\" + LinkedInUserName;

                var browser = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir, new BrowserTypeLaunchPersistentContextOptions()
                {
                    //Headless = false,
                    ExecutablePath = PlaywrightPath,
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
                var page = await browser.NewPageAsync();

                await page.GotoAsync(Command.Link, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = DelayTime });

                #region انجام لاگین
                try
                {
                    var elemSelector0 = "button[data-modal='base-sign-in-modal']";
                    await page.Locator(elemSelector0).WaitForAsync();
                    await page.Locator(elemSelector0
                        , new PageLocatorOptions() { HasText = "Sign in" }).ClickAsync();

                    var elemSelector1 = "#base-sign-in-modal_session_key";
                    await page.Locator(elemSelector1).WaitForAsync();
                    await page.Locator(elemSelector1).FillAsync(LinkedInUserName);

                    var elemSelector2 = "#base-sign-in-modal_session_password";
                    await page.Locator(elemSelector2).WaitForAsync();
                    await page.Locator(elemSelector2).FillAsync(LinkedInPassword);

                    var elemSelector3 = "div[data-id='sign-in-form__footer'] >> button[data-id='sign-in-form__submit-btn'][data-tracking-control-name='public_jobs_contextual-sign-in-modal_sign-in-modal_sign-in-submit-btn'][type='submit']:visible";
                    await page.Locator(elemSelector3).WaitForAsync();
                    await page.Locator(elemSelector3).ClickAsync();

                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = DelayTime });
                    await Task.Delay(2000);
                }
                catch { }
                #endregion

                #region کلیک روی گزینه اپلای لینکدین و باز شدن تب جدید
                var elemSelector4 = "#jobs-apply-button-id:visible";
                await page.Locator(elemSelector4).WaitForAsync(new LocatorWaitForOptions { Timeout = DelayTime });
                var newPageAfterLinkedinApplyClick = await page.RunAndWaitForPopupAsync(async () =>
                {
                    await page.Locator(elemSelector4).ClickAsync();

                }, new PageRunAndWaitForPopupOptions { Timeout = DelayTime });

                await newPageAfterLinkedinApplyClick.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = DelayTime });
                await Task.Delay(2000);

                string pageSource = await newPageAfterLinkedinApplyClick.ContentAsync();
                #endregion

                #region بررسی وجود دکمه ای مربوط به اپلای
                var res = await AnalyzeHtmlWithOpenAiAsync(pageSource);
                #endregion

                if (res.found)
                {
                    var elemSelector5 = res.selector;
                    await newPageAfterLinkedinApplyClick.Locator(elemSelector5).Nth(0).WaitForAsync(new LocatorWaitForOptions { Timeout = DelayTime });

                    if (!res.newTab)
                    {
                        var newPage2 = await newPageAfterLinkedinApplyClick.RunAndWaitForPopupAsync(async () =>
                        {
                            await newPageAfterLinkedinApplyClick.Locator(elemSelector5).Nth(0).ClickAsync();

                        }, new PageRunAndWaitForPopupOptions { Timeout = DelayTime });

                        string newPage2Source = await newPage2.ContentAsync();

                        #region بررسی مقادیر و سوالات
                        await Apply(newPageAfterLinkedinApplyClick, newPage2Source);
                        #endregion
                    }
                    else
                    {
                        await newPageAfterLinkedinApplyClick.Locator(elemSelector5).Nth(0).ClickAsync();
                        await newPageAfterLinkedinApplyClick.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = DelayTime });

                        string newPage1Source = await newPageAfterLinkedinApplyClick.ContentAsync();

                        #region بررسی مقادیر و سوالات
                        await Apply(newPageAfterLinkedinApplyClick, newPage1Source);
                        #endregion
                    }
                }
            }
        }
        private async Task Apply(IPage Page, string PageSource)
        {
        a:

            var fields = await AnalyzeFormFieldsWithOpenAiAsync(PageSource);

            foreach (var item in fields)
            {
                if (item.InputType.ToLower() == "email")
                {
                    try
                    {
                        await Page.Locator(item.InputSelector).Nth(0).WaitForAsync();
                        await Page.Locator(item.InputSelector).Nth(0).FillAsync(LinkedInUserName);
                    }
                    catch { }
                }
                else if (item.InputType.ToLower() == "text" && item.InputLabel.ToLower().Contains("first"))
                {
                    try
                    {
                        await Page.Locator(item.InputSelector).Nth(0).WaitForAsync();
                        await Page.Locator(item.InputSelector).Nth(0).FillAsync(User.FirstName);
                    }
                    catch { }
                }
                else if (item.InputType.ToLower() == "text" && item.InputLabel.ToLower().Contains("last"))
                {
                    try
                    {
                        await Page.Locator(item.InputSelector).Nth(0).WaitForAsync();
                        await Page.Locator(item.InputSelector).Nth(0).FillAsync(User.LastName);
                    }
                    catch { }
                }
                else if (item.InputType.ToLower() == "tel" && item.InputLabel.ToLower().Contains("phone"))
                {
                    try
                    {
                        await Page.Locator(item.InputSelector).Nth(0).WaitForAsync();
                        await Page.Locator(item.InputSelector).Nth(0).FillAsync(User.PhoneNumber);
                    }
                    catch { }
                }
                else if (item.InputType.ToLower() == "password" && item.InputLabel.ToLower().Contains("password"))
                {
                    try
                    {
                        await Page.Locator(item.InputSelector).Nth(0).WaitForAsync();
                        await Page.Locator(item.InputSelector).Nth(0).FillAsync(LinkedInPassword);
                    }
                    catch { }
                }
                else if (item.InputType.ToLower() == "file" &&
                    (item.InputLabel.ToLower().Contains("resume") || item.InputLabel.ToLower().Contains("cv")))
                {
                    try
                    {
                        var resumePath = await SendApplicationHelper.GenerateResume(DbContext, User, WebRootPath);

                        await Page.Locator(item.InputSelector).Nth(0).SetInputFilesAsync(resumePath);
                        await Task.Delay(10000);
                    }
                    catch { }
                }
                //else if (item.InputType.ToLower() == "file" &&
                //  (item.InputLabel.ToLower().Contains("cover") || item.InputLabel.ToLower().Contains("letter")))
                //{
                //    try
                //    {
                //        await Page.Locator(item.InputSelector).Nth(0).SetInputFilesAsync("C:\\CoverLetter.pdf");
                //        await Task.Delay(10000);
                //    }
                //    catch { }
                //}
                else if (item.InputType.ToLower() == "text" &&
                  (item.InputLabel.ToLower().Contains("cover") || item.InputLabel.ToLower().Contains("letter")))
                {
                    try
                    {
                        await Page.Locator(item.InputSelector).Nth(0).WaitForAsync();
                        await Page.Locator(item.InputSelector).Nth(0).FillAsync(Command.CoverLetter);
                    }
                    catch { }
                }
                else
                {
                    #region افزودن فیلدهای جدید
                    var q = new Question();

                    var options = JsonSerializer.Serialize(item.InputOptions);

                    q.Options = options;
                    q.Title = item.InputLabel;
                    q.Selector = item.InputSelector;
                    q.InputType = item.InputType;
                    q.SuggestId = Suggest.Id;

                    DbContext.Questions.Add(q);

                    await DbContext.Suggests
                        .Where(m => m.Id == Suggest.Id)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.SuggestType
                        , u => Domain.Enums.SuggestType.NewQuestions));

                    await DbContext.SaveChangesAsync();
                    #endregion
                }
            }

            #region بررسی وجود دکمه ای مربوط به ادامه یا دکمه نهایی
            var analyzeHtmlToFindNextOrFinalButton = await AnalyzeHtmlToFindNextOrFinalButton(PageSource);

            if (analyzeHtmlToFindNextOrFinalButton.IsFinal)
            {
                var elemSelectorSubmit = analyzeHtmlToFindNextOrFinalButton.selector;
                await Page.Locator(elemSelectorSubmit).Nth(0).WaitForAsync(new LocatorWaitForOptions { Timeout = DelayTime });

                await Page.Locator(elemSelectorSubmit).Nth(0).ClickAsync();
            }
            else
            {
                var elemSelectorNext = analyzeHtmlToFindNextOrFinalButton.selector;
                await Page.Locator(elemSelectorNext).Nth(0).WaitForAsync(new LocatorWaitForOptions { Timeout = DelayTime });

                await Page.Locator(elemSelectorNext).Nth(0).ClickAsync();

                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = DelayTime });
                await Task.Delay(2000);

                Page.WaitForNavigationAsync(new PageWaitForNavigationOptions()
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 600_000
                });

                PageSource = await Page.ContentAsync();

                goto a;
            }
            #endregion
        }
        public async Task<List<FormInputField>> AnalyzeFormFieldsWithOpenAiAsync(string htmlSource)
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentException("openAiKey is required", nameof(Key));
            if (string.IsNullOrWhiteSpace(htmlSource))
                throw new ArgumentException("htmlSource is required", nameof(htmlSource));

            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Key);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new object[]
                {
                new { role = "system", content = "You are an assistant that extracts form input fields from HTML and returns structured JSON." },
                new { role = "user", content = Prompts.PromptGetElements },
                new { role = "user", content = htmlSource }
                },
                temperature = 0.0,
                max_tokens = 8000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var respStr = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                // بهتره خطا را لاگ کنیم تا علت failure مشخص شود
                throw new Exception($"OpenAI API error: {resp.StatusCode}\n{respStr}");
            }

            var doc = JsonDocument.Parse(respStr);
            string assistantText;
            try
            {
                assistantText = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse assistant response: " + ex.Message + "\nRaw: " + respStr);
            }

            // استخراج JSON خالص از متن مدل (در صورت wrap شدن با backticks یا توضیحات)
            string jsonOnly = SendApplicationHelper.ExtractJsonFromTextFormFields(assistantText);
            if (string.IsNullOrWhiteSpace(jsonOnly))
            {
                // در صورتی که مدل خروجی غیرقابل‌استفاده برگشت داده، در خطای مفصل به کاربر اطلاع بده
                throw new Exception("No JSON array returned by model. Raw assistant text:\n" + assistantText);
            }

            // دِسریالایز به لیست مدل‌ها
            List<FormInputField> result;
            try
            {
                result = JsonSerializer.Deserialize<List<FormInputField>>(jsonOnly, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<FormInputField>();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to deserialize JSON into FormInputField list: " + ex.Message + "\nJSON was:\n" + jsonOnly);
            }

            return result;
        }
        public async Task<ContinueCheckResult> AnalyzeHtmlToFindNextOrFinalButton(string htmlSource)
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Key);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new object[]
                {
            new { role = "system", content = "You are an assistant that finds 'continue' or 'final submit' buttons in HTML and returns a JSON result." },
            new { role = "user", content = Prompts.PromptFindNextOrFinalButton },
            new { role = "user", content = htmlSource }
                },
                temperature = 0.0,
                max_tokens = 8000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var resp = await http.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var respStr = await resp.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(respStr);
            var assistantText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            string jsonOnly = SendApplicationHelper.ExtractJsonFromText(assistantText);
            if (string.IsNullOrWhiteSpace(jsonOnly))
                throw new Exception("No JSON returned by model. Raw: " + assistantText);

            var result = JsonSerializer.Deserialize<ContinueCheckResult>(jsonOnly, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null) result = new ContinueCheckResult { IsFinal = false, selector = "" };
            if (result.selector == null) result.selector = "";

            return result;
        }
        public async Task<ApplyCheckResult> AnalyzeHtmlWithOpenAiAsync(string htmlSource)
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Key);

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new object[]
                {
                new { role = "system", content = "You are an assistant that finds apply/submit buttons in HTML and returns a tiny JSON." },
                new { role = "user", content = Prompts.AnalyzeHtml },
                new { role = "user", content = htmlSource }
                },
                temperature = 0.0,
                max_tokens = 8000
            };

            var json = JsonSerializer.Serialize(requestBody);
            var resp = await http.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var respStr = await resp.Content.ReadAsStringAsync();
            //resp.EnsureSuccessStatusCode();

            // استخراج محتوای assistant
            var doc = JsonDocument.Parse(respStr);
            var assistantText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // استخراج JSON از متن دریافتی (حذف احتمالی backticks)
            string jsonOnly = SendApplicationHelper.ExtractJsonFromText(assistantText);
            if (string.IsNullOrWhiteSpace(jsonOnly))
                throw new Exception("No JSON returned by model. Raw: " + assistantText);

            var result = JsonSerializer.Deserialize<ApplyCheckResult>(jsonOnly, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // تضمین مقدار بازگشتی (مانند null -> false + "")
            if (result == null) result = new ApplyCheckResult { found = false, selector = "" };
            if (result.selector == null) result.selector = "";

            return result;
        }
    }
}
