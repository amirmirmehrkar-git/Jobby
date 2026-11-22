using Application.CQRS.Suggest_CQRS;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using WebAPI.CustomFilters;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers;

public class SuggestController : ApiControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IConfiguration configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _openAiKey;
    private readonly string _chatModel;
    protected readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager;

    public SuggestController(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> UserManager,
        IHttpClientFactory httpClientFactory, IConfiguration config, IWebHostEnvironment hostingEnvironment)
    {
        _httpClientFactory = httpClientFactory;
        _openAiKey = config.GetConnectionString("OpenAiKey");
        _chatModel = config["OPENAI_CHAT_MODEL"] ?? "gpt-4o-mini";
        configuration = config;
        userManager = UserManager;
        _hostingEnvironment = hostingEnvironment;
    }

    [CustomAuthorize("Admin,User")]
    [HttpPost("AddResponse")]
    public async Task<IActionResult> AddResponse(CreateResponseCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);

            var entity = new ResponseSuggest();
            entity.ResponseText = command.ResponseText;
            entity.SuggestId = command.SuggestId;

            var ai = new AI(_httpClientFactory, _chatModel, _openAiKey);
            var ret = await ai.GenerateResponse(command.ResponseText);
            var res = JsonSerializer.Deserialize<GenerateResponseViewModel>(ret);

            entity.AiResult = res.text;
            DbContext.ResponseSuggests.Add(entity);

            var suggestType = res.type == "Rejected" ? SuggestType.Rejected
                : res.type == "Passed" ? SuggestType.Passed
                : SuggestType.Interview;

            await DbContext.Suggests
                .Where(m => m.Id == command.SuggestId)
                .ExecuteUpdateAsync(setters =>
                setters.SetProperty(u => u.SuggestType, u => suggestType));

            await DbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpPost("UpdateQuestions")]
    public async Task<IActionResult> AddQuestions(UpdateQuestionsCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var li = await DbContext.Questions
                    .Where(m => m.SuggestId == command.SuggestId)
                    .ToListAsync();

            foreach (var item in command.Questions)
            {
                var q = li.SingleOrDefault(m => m.Id == item.Id);
                q.Answer = item.Answer;
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
    [HttpGet("GetAllSentSuggestByUserId")]
    public async Task<IActionResult> GetAllSentSuggestByUserId()
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);

            var query = await DbContext.Suggests.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .Where(m => m.SuggestType == SuggestType.Sent || m.SuggestType == SuggestType.Passed
                         || m.SuggestType == SuggestType.Rejected || m.SuggestType == SuggestType.Interview)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Company,
                    m.About,
                    m.Location,
                    m.Description,
                    m.JobType,
                    m.SuggestType,

                    Responses = (m.Responses == null || m.Responses.Count() == 0) ? null
                    : m.Responses.Select(r => new
                    {
                        r.ResponseText,
                        r.AiResult,
                        CreateDate = r.CreateDate.Value.ToLongDateString(),
                    }).ToList(),

                    CreateDate = m.CreateDate.Value.ToShortDateString(),
                })
                .ToListAsync();

            return Ok(query);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpPost("SearchSentSuggestByUserId")]
    public async Task<IActionResult> SearchSentSuggestByUserId(SearchSentSuggestByUserIdCommand command)
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);

            var query = await DbContext.Suggests.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .Where(m => m.SuggestType == SuggestType.Sent || m.SuggestType == SuggestType.Passed
                         || m.SuggestType == SuggestType.Rejected || m.SuggestType == SuggestType.Interview)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Company,
                    m.About,
                    m.Location,
                    m.Description,
                    m.JobType,
                    m.SuggestType,

                    Responses = (m.Responses == null || m.Responses.Count() == 0) ? null
                    : m.Responses.Select(r => new
                    {
                        r.ResponseText,
                        r.AiResult,
                        CreateDate = r.CreateDate.Value.ToLongDateString(),
                    }).ToList(),

                    CreateDate = m.CreateDate.Value.ToShortDateString(),
                })
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(command.Title))
            {
                query = query.Where(m => m.Title.Contains(command.Title)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(command.Company))
            {
                query = query.Where(m => m.Company.Contains(command.Company)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(command.Date))
            {
                query = query.ToList()
                    .Where(m => DateTime.Parse(m.CreateDate).Date
                    == DateTime.Parse(DateTime.Parse(command.Date).ToShortDateString())).ToList();
            }

            if (command.SuggestType == "Rejects")
            {
                query = query.Where(m => m.SuggestType == SuggestType.Rejected).ToList();
            }
            else if (command.SuggestType == "Passed")
            {
                query = query.Where(m => m.SuggestType == SuggestType.Passed).ToList();
            }
            else if (command.SuggestType == "Interviews")
            {
                query = query.Where(m => m.SuggestType == SuggestType.Interview).ToList();
            }
            else if (command.SuggestType == "Sent")
            {
                query = query.Where(m => m.SuggestType == SuggestType.Sent).ToList();
            }

            return Ok(query);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpGet("GetAllWaitingJobsByUserId")]
    public async Task<IActionResult> GetAllWaitingJobsByUserId()
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);

            var query = await DbContext.Suggests.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .Where(m => m.SuggestType == Domain.Enums.SuggestType.NewQuestions)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Company,
                    m.About,
                    m.Location,
                    m.Description,
                    m.JobType,

                    Questions = (m.Questions == null || m.Questions.Count() == 0) ? null
                    : m.Questions.Select(r => new
                    {
                        r.Id,
                        r.Title,
                        r.InputType,
                        r.Options,
                        r.Answer,
                    }).ToList(),

                    CreateDate = m.CreateDate.Value.ToShortDateString(),
                })
                .ToListAsync();

            return Ok(query);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpGet("GetAllNewJobsByUserId")]
    public async Task<IActionResult> GetAllNewJobsByUserId()
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);
            var jobs = new List<JobInfoViewModel>();

            var jobTitles = await DbContext.Positions.AsNoTracking()
                .Where(m => m.UserId == userId)
                .ToListAsync();
            if (jobTitles.Count() == 0)
            {
                ModelState.AddModelError("Messages", "You should have one job in your profile");
                return BadRequest(ModelState);
            }
            var jobTitle = jobTitles.Where(m => m.IsSelected.Value == true)
                 .FirstOrDefault().Title;

            var playwrightPath = configuration.GetConnectionString("PlaywrightPath");

            var apply = new Apply(_httpClientFactory, _chatModel, _openAiKey, jobTitle, playwrightPath);

            #region دریافت آگهی های لینکدین
            var linkedins = await apply.GetLinkedInJobs();
            jobs.AddRange(linkedins);
            #endregion

            #region دریافت آگهی های زینگ
            var xings = await apply.GetXingJobs();
            jobs.AddRange(xings);
            #endregion

            #region دریافت آگهی های استپ استون
            var stepStones = await apply.GetStepStoneJobs();
            jobs.AddRange(stepStones);
            #endregion

            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpGet("GetAllReadyToSendsByUserId")]
    public async Task<IActionResult> GetAllReadyToSendsByUserId()
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);

            var query = await DbContext.Suggests.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .Where(m => m.SuggestType == SuggestType.ReadyToSend)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Company,
                    m.About,
                    m.Location,
                    m.Description,
                    m.JobType,
                    m.CoverLetter,
                    CreateDate = m.CreateDate.Value.ToShortDateString(),
                })
                .ToListAsync();

            return Ok(query);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpPost("SendApplication")]
    public async Task<IActionResult> SendApplication(SendApplicationCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);
            var user = await userManager.FindByIdAsync(userId);
            var playwrightPath = configuration.GetConnectionString("PlaywrightPath");

            var entity = new Suggest();

            if (command.JobType == JobType.Link)
            {
                entity = await DbContext.Suggests
                    .Where(m => m.Id == command.Id)
                    .SingleOrDefaultAsync();

                entity.SuggestType = SuggestType.Sent;
            }
            else
            {
                entity.Title = command.Title;
                entity.Company = command.Company;
                entity.UserId = userId;
                entity.CoverLetter = command.CoverLetter;
                entity.Link = command.Link;
                entity.Location = command.Location;
                entity.JobType = command.JobType;
                entity.SuggestType = SuggestType.Sent;
                DbContext.Suggests.Add(entity);
            }

            await DbContext.SaveChangesAsync();

            var sendApplication = new SendApplication
               (
               DbContext,
               user,
               command,
               _openAiKey,
               playwrightPath,
               _hostingEnvironment.WebRootPath,
               entity
               );

            try
            {
                await sendApplication.Send();
            }
            catch { }

            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User")]
    [HttpPost("GenerateCoverLetter")]
    public async Task<IActionResult> GenerateCoverLetter(GenerateCoverLetterCommand command)
    {
        var playwrightPath = configuration.GetConnectionString("PlaywrightPath");
        var jobs = new List<JobInfoViewModel>();
        var userId = CurrentUserService.GetUserId(HttpContext);
        var user = await userManager.FindByIdAsync(userId);

        var jobTitles = await DbContext.Positions.AsNoTracking()
             .Where(m => m.UserId == userId)
             .ToListAsync();
        if (jobTitles.Count() == 0)
        {
            ModelState.AddModelError("Messages", "You should have one job in your profile");
            return BadRequest(ModelState);
        }
        var jobTitle = jobTitles.Where(m => m.IsSelected.Value == true)
                .FirstOrDefault().Title;

        var skills = await DbContext.Skills.AsNoTracking()
            .Where(m => !m.Deleted)
            .Where(m => m.UserId == userId)
            .ToListAsync();

        var Experiences = await DbContext.Experiences.AsNoTracking()
            .Where(m => !m.Deleted)
            .Where(m => m.UserId == userId)
            .ToListAsync();

        var Educations = await DbContext.Educations.AsNoTracking()
            .Where(m => !m.Deleted)
            .Where(m => m.UserId == userId)
            .ToListAsync();

        var Certificates = await DbContext.Certificates.AsNoTracking()
            .Where(m => !m.Deleted)
            .Where(m => m.UserId == userId)
            .ToListAsync();

        var li = new ResumeResult();
        li.Skills = skills;
        li.Experiences = Experiences;
        li.Educations = Educations;
        li.Certificates = Certificates;

        var apply = new Apply(_httpClientFactory, _chatModel, _openAiKey
            , jobTitle, playwrightPath);

        foreach (var item in command.Jobs)
        {
            jobs.Add(new JobInfoViewModel
            {
                Company = item.Company,
                CoverLetter = "",
                Link = item.Link,
                Location = item.Location,
                Title = item.Title,
                JobType = item.Type,
            });
        }

        #region بررسی شرکت ها و تولید کاورلتر
        var res = await apply.GenerateCoverLetter(jobs, li, user);
        #endregion

        foreach (var item in res)
        {
            if (item.JobType == JobType.Link)
            {
                var entity = new Suggest();
                entity.Title = item.Title;
                entity.Company = item.Company;
                entity.UserId = userId;
                entity.CoverLetter = item.CoverLetter;
                entity.Link = item.Link;
                entity.Location = item.Location;
                entity.JobType = JobType.Link;
                entity.SuggestType = SuggestType.ReadyToSend;
                DbContext.Suggests.Add(entity);

                await DbContext.SaveChangesAsync();
            }
        }

        return Ok(res);
    }

    [HttpPost("ResumeAnalyze")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> ResumeAnalyze([FromForm] IFormFile pdf, [FromForm] string TokenId)
    {
        var userId = CurrentUserService.GetUserId(TokenId);

        var ai = new AI(_httpClientFactory, _chatModel, _openAiKey);
        string text;
        using (var ms = new MemoryStream())
        {
            await pdf.CopyToAsync(ms);
            ms.Position = 0;
            text = ai.ExtractTextFromPdf(ms);
        }

        var li = await ai.ExtractAndAnalyzeResume(text);

        #region پر کردن اطلاعات از خروجی رزومه
        foreach (var item in li.Skills)
        {
            var hasExist = await DbContext.Skills.AsNoTracking()
                .Where(m => !m.Deleted)
                .AnyAsync(m => m.Title == item.Title);

            if (!hasExist)
            {
                var entity = new Skill();
                entity.Title = item.Title;
                entity.UserId = userId;

                DbContext.Skills.Add(entity);
            }
        }

        foreach (var item in li.Experiences)
        {
            var hasExist = await DbContext.Experiences.AsNoTracking()
                .Where(m => !m.Deleted)
                .AnyAsync(m => m.Project == item.Project && m.Company == item.Company
                            && m.Role == item.Role && m.Activity == item.Activity && m.Benefit == item.Benefit);

            if (!hasExist)
            {
                var entity = new Experience();
                entity.Project = item.Project;
                entity.Company = item.Company;
                entity.Role = item.Role;
                entity.Activity = item.Activity;
                entity.Benefit = item.Benefit;
                entity.UserId = userId;

                DbContext.Experiences.Add(entity);
            }
        }

        foreach (var item in li.Educations)
        {
            var hasExist = await DbContext.Educations.AsNoTracking()
                .Where(m => !m.Deleted)
                .AnyAsync(m => m.Academy == item.Academy && m.Degree == item.Degree && m.Score == item.Score);

            if (!hasExist)
            {
                var entity = new Education();
                entity.Academy = item.Academy;
                entity.Degree = item.Degree;
                entity.UserId = userId;

                DbContext.Educations.Add(entity);
            }
        }

        foreach (var item in li.Certificates)
        {
            var hasExist = await DbContext.Certificates.AsNoTracking()
                .Where(m => !m.Deleted)
                .AnyAsync(m => m.Academy == item.Academy && m.Academy == item.Academy && m.Skill == item.Skill);

            if (!hasExist)
            {
                var entity = new Certificate();
                entity.Title = item.Title;
                entity.Academy = item.Academy;
                entity.Skill = item.Skill;
                entity.UserId = userId;

                DbContext.Certificates.Add(entity);
            }
        }

        foreach (var item in li.JobTitles)
        {
            var hasExist = await DbContext.Positions.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .AnyAsync(m => m.Title == item);

            if (!hasExist)
            {
                var entity = new Position();
                entity.Title = item;
                entity.UserId = userId;

                DbContext.Positions.Add(entity);
            }
        }

        var user = await userManager.FindByIdAsync(userId);

        await userManager.UpdateAsync(user);
        #endregion

        await DbContext.SaveChangesAsync();

        return Ok(li);
    }

    [CustomAuthorize("Admin,User")]
    [HttpGet("GetNewJobsAndGenerateCoverLetter")]
    public async Task<IActionResult> GetNewJobsAndGenerateCoverLetter()
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);
            var user = await userManager.FindByIdAsync(userId);

            var jobs = new List<JobInfoViewModel>();

            var playwrightPath = configuration.GetConnectionString("PlaywrightPath");

            var jobTitles = await DbContext.Positions.AsNoTracking()
                .Where(m => m.UserId == userId)
                .ToListAsync();
            if (jobTitles.Count() == 0)
            {
                ModelState.AddModelError("Messages", "You should have one job in your profile");
                return BadRequest(ModelState);
            }
            var jobTitle = jobTitles.Where(m => m.IsSelected.Value == true)
                .FirstOrDefault().Title;

            var apply = new Apply(_httpClientFactory, _chatModel, _openAiKey
                , jobTitles.FirstOrDefault().Title, playwrightPath);

            #region دریافت آگهی های لینکدین
            var linkedins = await apply.GetLinkedInJobs();
            jobs.AddRange(linkedins);
            #endregion

            #region دریافت آگهی های زینگ
            var xings = await apply.GetXingJobs();
            jobs.AddRange(xings);
            #endregion

            #region دریافت آگهی های استپ استون
            var stepStones = await apply.GetStepStoneJobs();
            jobs.AddRange(stepStones);
            #endregion

            #region بررسی شرکت ها و تولید کاورلتر
            var skills = await DbContext.Skills.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            var Experiences = await DbContext.Experiences.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            var Educations = await DbContext.Educations.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            var Certificates = await DbContext.Certificates.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            var li = new ResumeResult();
            li.Skills = skills;
            li.Experiences = Experiences;
            li.Educations = Educations;
            li.Certificates = Certificates;

            var res = await apply.GenerateCoverLetter(jobs, li, user);
            #endregion

            return Ok(res);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
