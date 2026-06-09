using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Layout.Element;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;
using System.Text;
using System.Text.Json;
using WebAPI.ViewModels;
using static WebAPI.Controllers.SuggestController;

namespace WebUI.Services
{
    public class AI
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAiKey;
        private readonly string _chatModel;
        public AI(IHttpClientFactory httpClientFactory, string chatModel, string openAiKey)
        {
            _httpClientFactory = httpClientFactory;
            _openAiKey = openAiKey;
            _chatModel = chatModel;
        }


        public async Task<string> TranscribeWithOpenAiAsync(string filePath)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

            using var multipart = new MultipartFormDataContent();
            using var fs = System.IO.File.OpenRead(filePath);
            var fileContent = new StreamContent(fs);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            multipart.Add(fileContent, "file", Path.GetFileName(filePath));

            multipart.Add(new StringContent("whisper-1"), "model");
            //multipart.Add(new StringContent("gpt-4o-transcribe"), "model");
            //multipart.Add(new StringContent("gpt-4o-mini-transcribe"), "model");
            multipart.Add(new StringContent("en"), "language");

            var resp = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", multipart);
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Transcription API error: {resp.StatusCode} / {text}");

            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.TryGetProperty("text", out var txtEl))
                return txtEl.GetString() ?? "";
            else
                throw new Exception("پاسخ transcribe شامل 'text' نبود: " + text);
        }
        public async Task<JsonElement> ExtractSkillsJsonAsync(string transcript, string prompt)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

            var bodyObj = new
            {
                model = _chatModel,
                messages = new[]
                {
                new { role = "user", content = prompt }
            },
                temperature = 0.0,
                max_tokens = 800
            };

            var bodyJson = JsonSerializer.Serialize(bodyObj);
            using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var respText = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Chat API error: {resp.StatusCode} / {respText}");

            using var doc = JsonDocument.Parse(respText);
            var contentText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            try
            {
                var parsed = JsonDocument.Parse(contentText).RootElement;
                return parsed;
            }
            catch (JsonException)
            {
                var jsonOnly = ExtractJsonSubstring(contentText);
                var parsed = JsonDocument.Parse(jsonOnly).RootElement;
                return parsed;
            }
        }
        private string ExtractJsonSubstring(string s)
        {
            var start = s.IndexOf('[');
            var end = s.LastIndexOf(']');
            if (start >= 0 && end > start)
                return s.Substring(start, end - start + 1);
            throw new JsonException("Couldn't extract JSON substring from model output.");
        }


        public string ExtractTextFromPdf(Stream pdfStream)
        {
            StringBuilder sb = new StringBuilder();
            using (var reader = new PdfReader(pdfStream))
            using (var pdfDoc = new PdfDocument(reader))
            {
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var text = PdfTextExtractor.GetTextFromPage(page);
                    sb.AppendLine(text);
                }
            }
            return sb.ToString();
        }

        public async Task<ResumeResult> ExtractAndAnalyzeResume(string resumeText)
        {
            var prompt = $@"
شما یک استخراج‌کننده و تحلیل‌گر رزومه هستید. ورودی: متن کامل رزومه که از یک PDF استخراج شده است.

وظایف شما:

### 1) استخراج ساختار رزومه در قالب JSON زیر:
Skills:
  - {{Title: ...}}
Experiences:
  - {{Project: ...}}
  - {{Company: ...}}
  - {{Role: ...}}
  - {{Activity: ...}}
  - {{Benefit: ...}}
Educations:
  - {{Academy: ...}}
  - {{Degree: ...}}
Certificates:
  - {{Academy: ...}}
  - {{Title: ...}}
  - {{Skill: ...}}

### 2) زبان هر متن را مطابق رزومه حفظ کنید (انگلیسی/فارسی)

### 3) بر اساس اطلاعات به دست آمده آرایه ای از عنوان های شغلی که با اطلاعات تناسب دارند ایجاد کن.

### 4) آنالیز 10 بُعد کیفی رزومه (Resume Analysis)
خروجی باید شامل آرایه‌ای با 10 آبجکت باشد.  
هر آیتم شامل:
- Dimension  
- Score (0–10)  
- Comments  

ابعاد پیشنهادی:
1. Clarity
2. Structure
3. Experience Relevance
4. Technical Skill Strength
5. Communication Quality
6. Achievement Quantification
7. Seniority Level Fit
8. Language Quality
9. Completeness
10. Overall Professionalism

### 5) Summary
در 3–4 جمله، خلاصه‌ای از کیفیت کلی رزومه و پیشنهادهای بهبود بدهید.

### 6) خروجی نهایی فقط و فقط JSON باشد با قالب زیر:

{{
  ""Skills"": [],
  ""Experiences"": [],
  ""Educations"": [],
  ""Certificates"": [],
  ""JobTitles"":[],
  ""ResumeAnalysis"": [
      {{""Dimension"": ""..."", ""Score"": 0, ""Comments"": ""...""}}
  ],
  ""Summary"": ""...""
}}

متن رزومه:
{{" + resumeText + "}}";


            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _openAiKey);

            var bodyObj = new
            {
                model = _chatModel,
                messages = new[]
                {
            new { role = "user", content = prompt }
        },
                temperature = 0.0,
                max_tokens = 2000
            };

            var bodyJson = JsonSerializer.Serialize(bodyObj);
            using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var respText = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Chat API error: {resp.StatusCode} / {respText}");

            using var doc = JsonDocument.Parse(respText);
            var contentText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var jsonOnly = ExtractJsonSubstring2(contentText);
            var res = JsonSerializer.Deserialize<ResumeResult>(jsonOnly);

            return res;
        }


        //        public async Task<ResumeResult> ExtractAndAnalyzeResume(string resumeText)
        //        {
        //            var prompt = @"
        //شما یک استخراج‌کننده رزومه هستید. ورودی: متن کامل رزومه که به صورت PDF استخراج شده است.
        //وظیفه شما:
        //1) استخراج Skills, Experiences, Educations, Certificates طبق قالب زیر (هر فیلد موجود را پر کنید، در غیر این صورت رشته خالی باشد)
        //Skills:
        //  - {Title: ...}
        //Experiences:
        //  - {Project: ...}
        //  - {Company: ...}
        //  - {Role: ...}
        //  - {Activity: ...}
        //  - {Benefit: ...}
        //Educations:
        //  - {Academy: ...}
        //  - {Degree: ...}
        //Certificates:
        //  - {Academy: ...}
        //  - {Title: ...}
        //  - {Skill: ...}
        //2) خروجی باید JSON باشد و زبان هر متن را همانند متن رزومه حفظ کنید (انگلیسی یا فارسی)
        //3) علاوه بر JSON بالا، یک شیء JobTitle بسازید که عنوان شغلی انگلیسی ۳-۴ کلمه‌ای باشد.

        //متن رزومه:
        //""{" + resumeText + "}\"\"\"";

        //            using var client = _httpClientFactory.CreateClient();
        //            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

        //            var bodyObj = new
        //            {
        //                model = _chatModel,
        //                messages = new[]
        //                {
        //                new { role="user", content = prompt }
        //            },
        //                temperature = 0.0,
        //                max_tokens = 1200
        //            };

        //            var bodyJson = JsonSerializer.Serialize(bodyObj);
        //            using var content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

        //            var resp = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        //            var respText = await resp.Content.ReadAsStringAsync();

        //            if (!resp.IsSuccessStatusCode)
        //                throw new Exception($"Chat API error: {resp.StatusCode} / {respText}");

        //            using var doc = JsonDocument.Parse(respText);
        //            var contentText = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        //            var jsonOnly = ExtractJsonSubstring2(contentText);
        //            var res = JsonSerializer.Deserialize<ResumeResult>(jsonOnly);
        //            return res;
        //        }
        public string ExtractJsonSubstring2(string s)
        {
            var start = s.IndexOf('{');
            var end = s.LastIndexOf('}');
            if (start >= 0 && end > start)
                return s.Substring(start, end - start + 1);
            throw new Exception("Couldn't extract JSON substring");
        }

        public async Task<string> GenerateResponse(string Text)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

            string EmployerReplyPrompt = @"
You are a professional assistant specialized in writing respectful and formal responses to employers.

Task:
- You will receive the text of a message or email sent by an employer to a job applicant.
- Analyze the tone and content of the employer’s message.
- Based on the content, generate a polite and professional response that helps the applicant reply appropriately.

Guidelines:
1. If the employer expresses interest or asks for further steps (like interviews, tests, documents, or scheduling calls), write a positive, cooperative reply confirming availability and appreciation.
2. If the employer rejects the application or says no position is available, write a respectful thank-you note that keeps the connection open for future opportunities.
3. If the employer asks for clarification or more information (e.g., portfolio, salary expectations, documents), write a clear, concise, and polite message providing what was requested.
4. The response must sound natural, professional, and courteous — avoid over-formality or robotic tone.
5. The message should be written in the same language as the employer’s message (detect automatically).
6. The generated reply must follow ONLY these rules and nothing else.

Additional classification task:
You MUST classify the employer’s message into EXACTLY one of the following types:
- Rejected  → Employer declines or no position available.
- Passed    → Employer expresses interest or positivity but does NOT request an interview.
- Interview → Employer requests or suggests a meeting, call, or interview.

The 'type' field MUST NOT be empty. If the message is ambiguous, choose the closest match.

Output format requirement:
You MUST return ONLY a single JSON object with EXACTLY these two fields:

{
  ""text"": ""<the generated reply>"",
  ""type"": ""<Rejected | Passed | Interview>""
}

- Absolutely no explanation.
- No commentary.
- No additional text outside the JSON.
- The 'text' value must contain ONLY the applicant's response.

Example input:
“The company appreciates your interest, but currently we do not have a suitable position available.”
Example output:
{
  ""text"": ""Thank you very much for considering my application. I truly appreciate your time and the opportunity. I hope we can stay in touch for future openings that match my background."",
  ""type"": ""Rejected""
}

Now analyze the following employer message and produce ONLY the JSON:
"
+ "\n\n" + Text;


            var bodyObj = new
            {
                model = _chatModel,
                messages = new[]
                {
            new { role = "user", content = EmployerReplyPrompt }
        },
                temperature = 0.0,
                max_tokens = 800
            };

            var json = JsonSerializer.Serialize(bodyObj);
            var response = await client.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return reply?.Trim() ?? "";
        }
    }
}
