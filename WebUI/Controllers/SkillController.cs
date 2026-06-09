using Application.CQRS.Skill_CQRS;
using Domain.Entities;
using FastReport.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using WebAPI.CustomFilters;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers;

public class SkillController : ApiControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _openAiKey;
    private readonly string _chatModel;

    public SkillController(IHttpClientFactory httpClientFactory, IConfiguration config)
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

            var entity = await DbContext.Skills
                .FindAsync(new object[] { command.Id });

            if (entity == null)
            {
                entity = new Skill();
                entity.Title = command.Title;
                //entity.Value = command.Value.ToString();
                entity.UserId = userId;

                DbContext.Skills.Add(entity);
            }
            else
            {
                entity.Title = command.Title;
                //entity.Value = command.Value.ToString();
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

            var query = await DbContext.Skills.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == userId)
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.CreateDate,
                        //Value = Convert.ToInt32(m.Value),
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

            var entity = await DbContext.Skills.FindAsync(new object[] { Id });

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
            Input: an English transcript where the user verbally talks about their skills and level of ability. 
            Your task is to extract all the skills mentioned in the text and create a JSON object for each skill with the   following     field:
            
            - Title: the skill name (like 'C#', 'AngularJS', 'SQL', 'PowerPoint', etc.). 

            Do NOT translate or localize skill names — keep them exactly as they appear in English.
            
            Expected output example (only JSON, no explanations):
            [
              {{ ""Title"": ""C#"" }},
              {{ ""Title"": ""AngularJS"" }}
            ]
            
            Now process the following text and return only JSON in the same format (no extra text or explanations): ""{transcript}""
            ";

            var extractedJson = await ai.ExtractSkillsJsonAsync(transcript, prompt);

            List<Skill> li = JsonSerializer.Deserialize<List<Skill>>(extractedJson);

            var userId = CurrentUserService.GetUserId(HttpContext);
            foreach (var item in li)
            {
                var hasExist = await DbContext.Skills.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == userId)
                    .AnyAsync(m => m.Title == item.Title);

                if (!hasExist)
                {
                    var entity = new Skill();
                    entity.Title = item.Title;
                    entity.UserId = userId;

                    DbContext.Skills.Add(entity);
                }
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
