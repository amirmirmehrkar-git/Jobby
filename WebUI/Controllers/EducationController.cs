using Application.CQRS.Education_CQRS;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using WebAPI.CustomFilters;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers;

public class EducationController : ApiControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _openAiKey;
    private readonly string _chatModel;

    public EducationController(IHttpClientFactory httpClientFactory, IConfiguration config)
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

            var entity = await DbContext.Educations
                .FindAsync(new object[] { command.Id });

            if (entity == null)
            {
                entity = new Education();
                entity.Academy = command.Academy;
                entity.Degree = command.Degree;
                entity.Score = command.Score;
                entity.UserId = userId;

                DbContext.Educations.Add(entity);
            }
            else
            {
                entity.Academy = command.Academy;
                entity.Degree = command.Degree;
                entity.Score = command.Score;
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

            var query = await DbContext.Educations.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == userId)
                    .Select(m => new
                    {
                        m.Id,
                        m.Academy,
                        m.Degree,
                        //m.Score,
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

            var entity = await DbContext.Educations.FindAsync(new object[] { Id });

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
    [RequestSizeLimit(50_000_000)] // مثال: 50MB - تنظیم کن به نیازت
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
The output must be in **English only** — do not use or include any Persian (Farsi) words.

Input: an English transcript where the user verbally talks about their educational background. 
Your task is to extract the following pieces of information and create one JSON object with these fields:

- Academy: the name of the academy, university, or institution (if mentioned, otherwise an empty string)
- Degree: the degree, diploma, or level of education (if mentioned, otherwise an empty string)


⚠️ Output rules:
- Return **only** valid JSON (no explanations, no extra text).
- All keys and values must be written in English only.
- If a value is missing, return an empty string for it.
- Do not translate or localize any names or terms.
- The 'Score' field must contain only numeric values (digits, optional decimal point). If it is not numeric, return an empty string.

Expected output example:
[
  {{
    ""Academy"": ""Harvard University"",
    ""Degree"": ""Master of Computer Science""
  }}
]

Now process the following text and return only JSON (English only, no extra text): ""{transcript}""
";


            var extractedJson = await ai.ExtractSkillsJsonAsync(transcript, prompt);

            List<Education> li = JsonSerializer.Deserialize<List<Education>>(extractedJson);

            var userId = CurrentUserService.GetUserId(HttpContext);
            foreach (var item in li)
            {
                var entity = new Education();
                entity.Academy = item.Academy;
                entity.Degree = item.Degree;
                entity.Score = item.Score;
                entity.UserId = userId;

                DbContext.Educations.Add(entity);
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
