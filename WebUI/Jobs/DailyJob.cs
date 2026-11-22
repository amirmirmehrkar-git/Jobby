using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Jobs
{
    public class DailyJob : IJob
    {
        protected IApplicationDbContext DbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAiKey;
        private readonly string _chatModel;
        protected readonly UserManager<ApplicationUser> userManager;

        public DailyJob(IApplicationDbContext dbContext, UserManager<ApplicationUser> UserManager,
        IHttpClientFactory httpClientFactory, IConfiguration config, IWebHostEnvironment hostingEnvironment)
        {
            DbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _openAiKey = config.GetConnectionString("OpenAiKey");
            _chatModel = config["OPENAI_CHAT_MODEL"] ?? "gpt-4o-mini";
            configuration = config;
            userManager = UserManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var users = DbContext.Users.AsNoTracking()
                .Where(m => m.UserType == UserType.User);

           

            foreach (var u in users)
            {
                var user = userManager.FindByIdAsync(u.Id).Result;

                var jobs = new List<JobInfoViewModel>();

                var playwrightPath = configuration.GetConnectionString("PlaywrightPath");

                var jobTitles = DbContext.Positions.AsNoTracking()
                    .Where(m => m.UserId == user.Id)
                    .ToList();
                var jobTitle = "";
                if (jobTitles.Count() != 0)
                {
                    jobTitle = jobTitles.Where(m => m.IsSelected.Value == true)
                        .FirstOrDefault().Title;
                }

                var apply = new Apply(_httpClientFactory, _chatModel, _openAiKey, jobTitle, playwrightPath);

                #region دریافت آگهی های لینکدین
                var linkedins = apply.GetLinkedInJobs().Result;
                jobs.AddRange(linkedins);
                #endregion

                #region دریافت آگهی های زینگ
                var xings = apply.GetXingJobs().Result;
                jobs.AddRange(xings);
                #endregion

                #region دریافت آگهی های استپ استون
                var stepStones = apply.GetStepStoneJobs().Result;
                jobs.AddRange(stepStones);
                #endregion

                #region بررسی شرکت ها و تولید کاورلتر
                var skills = DbContext.Skills.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == u.Id)
                    .ToListAsync().Result;

                var Experiences = DbContext.Experiences.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == u.Id)
                    .ToListAsync().Result;

                var Educations = DbContext.Educations.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == u.Id)
                    .ToListAsync().Result;

                var Certificates = DbContext.Certificates.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == u.Id)
                    .ToListAsync().Result;

                var li = new ResumeResult();
                li.Skills = skills;
                li.Experiences = Experiences;
                li.Educations = Educations;
                li.Certificates = Certificates;

                var res = apply.GenerateCoverLetter(jobs, li, user).Result;
                #endregion

                foreach (var command in res)
                {
                    var entity = new Suggest();
                    entity.Title = command.Title;
                    entity.Company = command.Company;
                    entity.UserId = u.Id;
                    entity.CoverLetter = command.CoverLetter;
                    entity.Link = command.Link;
                    entity.Location = command.Location;
                    entity.JobType = command.JobType;
                    entity.SuggestType = SuggestType.Sent;
                    DbContext.Suggests.Add(entity);

                    DbContext.SaveChangesAsync();

                    var s = new Application.CQRS.Suggest_CQRS.SendApplicationCommand()
                    {
                        Company = command.Company,
                        CoverLetter = command.CoverLetter,
                        JobType = command.JobType,
                        Link = command.Link,
                        Location = command.Location,
                        Title = command.Title,
                    };

                    var sendApplication = new SendApplication
                       (
                       DbContext,
                       user,
                       s,
                       _openAiKey,
                       playwrightPath,
                       _hostingEnvironment.WebRootPath,
                       entity
                       );

                    try
                    {
                        sendApplication.Send();
                    }
                    catch { }
                }
            }

            return Task.CompletedTask;
        }
    }
}
