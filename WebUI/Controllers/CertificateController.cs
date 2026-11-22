using Application.CQRS.Certificate_CQRS;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using WebAPI.CustomFilters;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers;

public class CertificateController : ApiControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _openAiKey;
    private readonly string _chatModel;

    public CertificateController(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _openAiKey = config.GetConnectionString("OpenAiKey");
        _chatModel = config["OPENAI_CHAT_MODEL"] ?? "gpt-4o-mini";
    }

    [CustomAuthorize("Admin,User")]
    [HttpPost("AddOrUpdate")]
    public async Task<IActionResult> AddOrUpdate(CreateCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);

            var entity = await DbContext.Certificates
                .FindAsync(new object[] { command.Id });

            if (entity == null)
            {
                entity = new Certificate();
                entity.Title = command.Title;
                entity.Academy = command.Academy;
                entity.Skill = command.Skill;
                entity.UserId = userId;

                DbContext.Certificates.Add(entity);
            }
            else
            {
                entity.Title = command.Title;
                entity.Academy = command.Academy;
                entity.Skill = command.Skill;
            }

            await DbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpPost("GetAllByUserId")]
    public async Task<IActionResult> GetAllByUserId(FilterParamsViewModel model)
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);

            var query = await DbContext.Certificates.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == userId)
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Academy,
                        m.Skill,
                        m.CreateDate,
                    })
                    .ToListAsync();

            if (model.Filter != null)
            {
                //foreach (var item in model.Filter)
                //{
                //    if (item.Key == "Title")
                //    {
                //        var val = item.Value[0];
                //        query = query.Where(m => m.Title.Contains(val)).ToList();
                //    }
                //    else if (item.Key == "Number")
                //    {
                //        var val = item.Value[0];
                //        query = query.Where(m => m.Number == val).ToList();
                //    }
                //    else if (item.Key == "UserFullName")
                //    {
                //        var val = item.Value[0];
                //        query = query.Where(m => m.UserFullName == val).ToList();
                //    }
                //}
            }
            if (model.Sorting != null && model.Sorting.Count > 0)
                query = query.AsQueryable().OrderBy(model.Sorting.ElementAt(0).Key + (model.Sorting.ElementAt(0).Value == "desc" ? " descending" : "")).ToList();
            else
                query = query.OrderByDescending(m => m.CreateDate).ToList();

            var result = query.Skip((model.PageCount - 1) * model.PageSize).Take(model.PageSize);
            int count = query.Count();
            return Ok(new { items = result, count });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpGet("Delete/{Id}")]
    public async Task<IActionResult> Delete(Guid Id)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = await DbContext.Certificates.FindAsync(new object[] { Id });

            if (entity == null)
            {
                throw new Exception("Not Found");
            }

            entity.Deleted = true;
            await DbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost("process")]
    [RequestSizeLimit(21 * 1024 * 1024)] // Match the application-wide 21 MiB request ceiling.
    public async Task<IActionResult> Process([FromForm] IFormFile audio)
    {
        if (audio == null || audio.Length == 0)
            return BadRequest("هیچ فایل صوتی ارسال نشده.");

        var ai = new AI(_httpClientFactory, _chatModel, _openAiKey);

        var tmpFolder = Path.Combine(Path.GetTempPath(), "uploads");
        Directory.CreateDirectory(tmpFolder);
        var tmpFile = Path.Combine(tmpFolder, $"{Guid.NewGuid()}{Path.GetExtension(audio.FileName)}");
        using (var fs = System.IO.File.Create(tmpFile))
        {
            await audio.CopyToAsync(fs);
        }

        try
        {
            var transcript = await ai.TranscribeWithOpenAiAsync(tmpFile);

            var prompt = $@"
You are an information extractor.
You must respond **in English only**. 
Never use Persian (Farsi), Arabic, or any other non-English words in the output.

Input: an English transcript where the user verbally talks about their certifications or professional training. 
Your task is to extract the following pieces of information and create one JSON object for each certification with these fields:

- Academy: the name of the academy, organization, or institution that issued the certification (if mentioned, otherwise an empty string)
- Title: the name or title of the certification (if mentioned, otherwise an empty string)
- Skill: the skill or area of expertise covered by the certification (if mentioned, otherwise an empty string)

⚠️ Strict Output Rules:
- The output must be **valid JSON only** — no explanations, no commentary, no extra text.
- The output must be written entirely in **English only**.
- Return the result as an **array of JSON objects** (one per certification, if multiple exist).
- If a value is missing, return an empty string `""`.
- Do not translate, interpret, or localize any names or terms — return them exactly as they appear in the input.
- If any non-English words appear in the transcript, ignore them.

Expected output example:
[
  {{
    ""Academy"": ""Microsoft"",
    ""Title"": ""Azure Fundamentals"",
    ""Skill"": ""Cloud Computing""
  }}
]

Now process the following text and return **only JSON (English only, no extra text)**: ""{transcript}""
";

            var extractedJson = await ai.ExtractSkillsJsonAsync(transcript, prompt);

            List<Certificate> li = JsonSerializer.Deserialize<List<Certificate>>(extractedJson);

            var userId = CurrentUserService.GetUserId(HttpContext);
            foreach (var item in li)
            {
                var entity = new Certificate();
                entity.Title = item.Title;
                entity.Academy = item.Academy;
                entity.Skill = item.Skill;
                entity.UserId = userId;

                DbContext.Certificates.Add(entity);
            }
            await DbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
        finally
        {
            // حذف فایل موقت
            if (System.IO.File.Exists(tmpFile)) System.IO.File.Delete(tmpFile);
        }
    }
}
