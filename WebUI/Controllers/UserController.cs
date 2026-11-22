using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text.Json;
using WebAPI.CustomFilters;
using WebAPI.Extensions;
using WebAPI.Helper;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers
{
    public class UserController : ApiControllerBase
    {
        protected readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAiKey;
        private readonly string _chatModel;

        public UserController(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> UserManager,
            IWebHostEnvironment hostingEnvironment,
            IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _hostingEnvironment = hostingEnvironment;
            userManager = UserManager;

            _httpClientFactory = httpClientFactory;
            _openAiKey = config.GetConnectionString("OpenAiKey");
            _chatModel = config["OPENAI_CHAT_MODEL"] ?? "gpt-4o-mini";
        }

        private async Task<string> CreateUser(AddOrUpdateUserViewModel model, UserType userType, List<string> roles = null)
        {
            var query = await userManager.FindByEmailAsync(model.Email);
            if (query == null)
            {
                var senfId = Guid.Empty;

                var employe = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    PhoneNumber = "",
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true,
                    Deleted = false,
                    UserType = userType,
                    Active = true,
                    CreateDate = DateTime.Now,
                };
                var res1 = await userManager.CreateAsync(employe, model.Password);
                if (res1.Errors.Any(m => m.Code.ToLower().Contains("tooshort")))
                {
                    throw new Exception("تعداد کاراکتر های رمز باید از 8 عدد بیشتر باشد");
                }
                if (res1.Errors.Any(m => m.Code.ToLower().Contains("duplicate")))
                {
                    throw new Exception("شماره تماس تکراری است");
                }

                if (userType == UserType.User)
                {
                    await userManager.AddToRolesAsync(employe, new[] { "User" });
                }
                else if (userType == UserType.Admin)
                {
                    await userManager.AddToRolesAsync(employe, roles);
                }

                return employe.Id;
            }
            else
            {
                query.PhoneNumber = "";
                query.UserName = model.Email;
                query.FirstName = model.FirstName;
                query.LastName = model.LastName;
                query.Active = true;

                var res = await userManager.UpdateAsync(query);

                string resetToken = await userManager.GeneratePasswordResetTokenAsync(query);
                var res1 = await userManager.ResetPasswordAsync(query, resetToken, model.Password);
                if (res1.Errors.Any(m => m.Code.ToLower().Contains("tooshort")))
                {
                    throw new Exception("تعداد کاراکتر های رمز باید از 8 عدد بیشتر باشد");
                }
                if (res1.Errors.Any(m => m.Code.ToLower().Contains("duplicate")))
                {
                    throw new Exception("شماره تماس تکراری است");
                }
                return query.Id;
            }
        }

        [CustomAuthorize("Admin,READ_USERS")]
        [HttpPost("AddOrUpdateUser")]
        public async Task<IActionResult> AddOrUpdateUser(AddOrUpdateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = await CreateUser(model, UserType.User);


                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Messages", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [CustomAuthorize("Admin,READ_USERS")]
        [HttpPost("AddOrUpdateAdmin")]
        public async Task<IActionResult> AddOrUpdateAdmin(AddOrUpdateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = await CreateUser(model, UserType.Admin);

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Messages", ex.Message);
                return BadRequest(ModelState);
            }
        }


        [CustomAuthorize("Admin,User")]
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = CurrentUserService.GetUserId(HttpContext);
                var query = await userManager.FindByIdAsync(userId);
                query.FirstName = model.FirstName;
                query.LastName = model.LastName;
                query.PhoneNumber = model.PhoneNumber;
                query.ImageName = model.ImageName;
                query.AboutMe = model.AboutMe;
                query.Linkdin = model.LinkedInUserName + "," + model.LinkedInPassword;
                query.Stepstone = model.StepStoneUserName + "," + model.StepStonePassword;
                query.Xing = model.XingUserName + "," + model.XingPassword;
                query.ApplyType = model.ApplyType;

                await userManager.UpdateAsync(query);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [CustomAuthorize("Admin,User")]
        [HttpPost("UpdateInformation")]
        public async Task<IActionResult> UpdateInformation(UpdateInformationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = CurrentUserService.GetUserId(HttpContext);
                var query = await userManager.FindByIdAsync(userId);
                query.AboutMe = model.AboutMe;

                await userManager.UpdateAsync(query);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [CustomAuthorize("Admin,User,READ_USERS")]
        [HttpGet("Delete/{Id}")]
        public async Task<ActionResult> Delete(string Id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var entity = await DbContext.Users
                    .FindAsync(new object[] { Id });

                if (entity == null)
                {
                    throw new Exception("موردی یافت نشد");
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


        [CustomAuthorize("Admin,READ_USERS")]
        [HttpPost("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(FilterParamsViewModel model)
        {
            try
            {
                var query = await DbContext.Users.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserType == UserType.User)
                    .Select(m => new
                    {
                        m.Id,
                        m.Email,
                        m.PhoneNumber,
                        FullName = m.FirstName + " " + m.LastName,
                        m.FirstName,
                        m.LastName,
                        m.FatherName,
                        m.Active,
                        CreateDate = m.CreateDate.Value.ToShortDateString(),
                    })
                    .ToListAsync();

                if (model.Filter != null)
                {
                    foreach (var item in model.Filter)
                    {
                        if (item.Key == "FullName")
                        {
                            var val = item.Value[0];
                            query = query.Where(m => m.FullName.Contains(val)).ToList();
                        }
                        if (item.Key == "PhoneNumber")
                        {
                            var val = item.Value[0];
                            query = query.Where(m => m.PhoneNumber.Contains(val)).ToList();
                        }
                        //if (item.Key == "CreateDate")
                        //{
                        //    var val = item.Value[0];
                        //    try
                        //    {
                        //        DateTime date = PersianDateTime.Parse(val).ToDateTime().Date;
                        //        query = query.Where(m => m.CreateDate.Year == date.Year
                        //                              && m.CreateDate.Month == date.Month
                        //                              && m.CreateDate.Day == date.Day).ToList();
                        //    }
                        //    catch { }
                        //}
                    }
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

        [HttpPost("ProcessUserDetailVoice")]
        [RequestSizeLimit(50_000_000)]
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

Input: an English transcript where the user verbally talks about their skills, projects, company, role, activities, benefits, educational background, certifications, or professional training.

Your task is to extract all relevant information and return a structured JSON result with the following sections:

1) Skills:
Extract all mentioned skills.
Return as an array of objects:
- Title: the skill name exactly as it appears in English.

2) Experiences:
Extract project-related and experiences information.
Return as an array of objects:
- Project: project name (or empty string)
- Company: company name (or empty string)
- Role: role or position (or empty string)
- Activity: main activity or task (or empty string)
- Benefit: benefit, goal, or outcome (or empty string)

3) Educations:
Extract educational background.
Return as an array of objects:
- Academy: institution name (or empty string)
- Degree: degree or education level (or empty string)

4) Certificates:
Extract certifications or professional training.
Return as an array of objects:
- Academy: issuing organization (or empty string)
- Title: certification title (or empty string)
- Skill: related skill or expertise area (or empty string)

Strict Output Rules:
- Output must be valid JSON only.
- Output must be entirely in English.
- Do not use Persian (Farsi), Arabic, or any non-English words.
- Do not translate, interpret, or localize names or terms.
- Keep all extracted values exactly as they appear in the input.
- If a value is missing, return an empty string """".
- If no items exist in an array section, return an empty array [].
- Ignore any non-English words in the transcript.

Return the result in this exact structure:

{{
  ""Skills"":  {{
    ""Title"": """"
  }},
  ""Experiences"": {{
    ""Project"": """",
    ""Company"": """",
    ""Role"": """",
    ""Activity"": """",
    ""Benefit"": """"
  }},
  ""Educations"": {{
    ""Academy"": """",
    ""Degree"": """",    
    ""Score"": 0
  }},
  ""Certificates"": {{
    ""Academy"": """",
    ""Title"": """",    
    ""Skill"": """",
  }}
}}

Now process the following text and return only JSON:

""{transcript}""
";

                var extractedJson = await ai.ExtractSkillsJsonAsync(transcript, prompt);

                UserDetailViewModel li = JsonSerializer.Deserialize<UserDetailViewModel>(extractedJson);

                var userId = CurrentUserService.GetUserId(HttpContext);

                foreach (var item in li.Skills)
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

                await DbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                if (System.IO.File.Exists(tmpFile)) System.IO.File.Delete(tmpFile);
            }
        }

        [HttpPost("ProcessUserDetailChat")]
        public async Task<IActionResult> ProcessUserDetailChat(ProcessUserDetailChatViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ai = new AI(_httpClientFactory, _chatModel, _openAiKey);

                var prompt = $@"
You are an information extractor.

Input: an English transcript where the user verbally talks about their skills, projects, company, role, activities, benefits, educational background, certifications, or professional training.

Your task is to extract all relevant information and return a structured JSON result with the following sections:

1) Skills:
Extract all mentioned skills.
Return as an array of objects:
- Title: the skill name exactly as it appears in English.

2) Experiences:
Extract project-related and experiences information.
Return as an array of objects:
- Project: project name (or empty string)
- Company: company name (or empty string)
- Role: role or position (or empty string)
- Activity: main activity or task (or empty string)
- Benefit: benefit, goal, or outcome (or empty string)

3) Educations:
Extract educational background.
Return as an array of objects:
- Academy: institution name (or empty string)
- Degree: degree or education level (or empty string)

4) Certificates:
Extract certifications or professional training.
Return as an array of objects:
- Academy: issuing organization (or empty string)
- Title: certification title (or empty string)
- Skill: related skill or expertise area (or empty string)

Strict Output Rules:
- Output must be valid JSON only.
- Output must be entirely in English.
- Do not use Persian (Farsi), Arabic, or any non-English words.
- Do not translate, interpret, or localize names or terms.
- Keep all extracted values exactly as they appear in the input.
- If a value is missing, return an empty string """".
- If no items exist in an array section, return an empty array [].
- Ignore any non-English words in the transcript.

Return the result in this exact structure:

{{
  ""Skills"":  {{
    ""Title"": """"
  }},
  ""Experiences"": {{
    ""Project"": """",
    ""Company"": """",
    ""Role"": """",
    ""Activity"": """",
    ""Benefit"": """"
  }},
  ""Educations"": {{
    ""Academy"": """",
    ""Degree"": """",    
    ""Score"": 0
  }},
  ""Certificates"": {{
    ""Academy"": """",
    ""Title"": """",    
    ""Skill"": """",
  }}
}}

Now process the following text and return only JSON:

""{model.Chat}""
";

                var extractedJson = await ai.ExtractSkillsJsonAsync(model.Chat, prompt);

                UserDetailViewModel li = JsonSerializer.Deserialize<UserDetailViewModel>(extractedJson);

                var userId = CurrentUserService.GetUserId(HttpContext);

                foreach (var item in li.Skills)
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

                await DbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Messages", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpGet("GetUserPositions")]
        public async Task<IActionResult> GetUserPositions()
        {
            try
            {
                var userId = CurrentUserService.GetUserId(HttpContext);

                var query = DbContext.Positions.AsNoTracking()
                  .Where(m => !m.Deleted)
                  .Where(m => m.UserId == userId)
                  .ToList()
                  .Select(m => new
                  {
                      m.Id,
                      m.IsSelected,
                      m.Index,
                      m.Title
                  })
                  .ToList();

                return Ok(query);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("SelectPosition/{Id}")]
        public async Task<IActionResult> SelectPosition(Guid Id)
        {
            try
            {
                var userId = CurrentUserService.GetUserId(HttpContext);

                var query = await DbContext.Positions
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == userId)
                    .ToListAsync();
                foreach (var item in query)
                {
                    item.IsSelected = false;
                }
                await DbContext.SaveChangesAsync();

                var obj = await DbContext.Positions
                  .Where(m => !m.Deleted)
                  .Where(m => m.Id == Id)
                  .SingleOrDefaultAsync();
                obj.IsSelected = true;
                await DbContext.SaveChangesAsync();

                return Ok(query);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
