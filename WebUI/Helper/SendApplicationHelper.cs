using Application.Common.Interfaces;
using Domain.Entities;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Helper
{
    public static class SendApplicationHelper
    {
        public static string ExtractJsonFromTextFormFields(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            int start = text.IndexOf('[');
            int end = text.LastIndexOf(']');
            if (start == -1 || end == -1 || end < start) return null;
            string candidate = text.Substring(start, end - start + 1).Trim();

            if (candidate.StartsWith("```") && candidate.EndsWith("```"))
            {
                int firstFenceEnd = candidate.IndexOf('\n');
                if (firstFenceEnd > 0)
                {
                    candidate = candidate.Substring(firstFenceEnd + 1);
                    if (candidate.EndsWith("```"))
                        candidate = candidate.Substring(0, candidate.Length - 3).Trim();
                }
            }

            return candidate;
        }

        public static string ExtractJsonFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            // ساده: اولین '{' تا آخرین '}' را برمی‌دارد
            int start = text.IndexOf('{');
            int end = text.LastIndexOf('}');
            if (start == -1 || end == -1 || end < start) return null;
            string candidate = text.Substring(start, end - start + 1).Trim();

            // حذف ```json ... ``` در صورت وجود
            if (candidate.StartsWith("```") && candidate.EndsWith("```"))
            {
                candidate = candidate.Substring(3, candidate.Length - 6).Trim();
            }
            return candidate;
        }

        public static async Task<string> GenerateResume(IApplicationDbContext DbContext, ApplicationUser user, string WebRootPath)
        {
            var userImageName = user.ImageName != null
                ? (WebRootPath + "\\Content\\Images\\ProfileImages\\" + user.ImageName)
                : "";
            userImageName = userImageName.Replace("\\", "/");

            var skills = await DbContext.Skills.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == user.Id)
                .Select(m => new
                {
                    m.Title,
                })
                .ToListAsync();

            var experiences = await DbContext.Experiences.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == user.Id)
                .Select(m => new
                {
                    m.Id,
                    m.Project,
                    m.Company,
                    m.Role,
                })
                .ToListAsync();

            var educations = await DbContext.Educations.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == user.Id)
                .Select(m => new
                {
                    m.Academy,
                    m.Degree,
                    //m.Score,
                })
                .ToListAsync();

            var certificates = await DbContext.Certificates.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.UserId == user.Id)
                .Select(m => new
                {
                    m.Title,
                    m.Academy,
                    m.Skill,
                })
                .ToListAsync();

            var jobTitles = await DbContext.Positions.AsNoTracking()
                .Where(m => m.UserId == user.Id)
                .ToListAsync();
            var jobTitle = "";
            if (jobTitles.Count() != 0)
            {
                jobTitle = jobTitles.Where(m => m.IsSelected.Value == true)
                 .FirstOrDefault().Title;
            }

            Report rpt = new Report();
            rpt.RegisterData(skills, "skills");
            rpt.RegisterData(experiences, "experiences");
            rpt.RegisterData(educations, "educations");
            rpt.RegisterData(certificates, "certificates");
            //rpt.Design();
            rpt.Load(WebRootPath + "\\Content\\Resume\\Resume.frx");

            rpt.SetParameterValue("UserFullName", user.FirstName + " " + user.LastName);
            rpt.SetParameterValue("UserImageName", userImageName);
            rpt.SetParameterValue("Email", user.Email);
            rpt.SetParameterValue("Phone", user.PhoneNumber);
            rpt.SetParameterValue("Job", jobTitle);
            rpt.SetParameterValue("ProfileText", user.AboutMe);

            rpt.Prepare();

            #region PDF
            var fileName = Guid.NewGuid().ToString() + ".pdf";
            var filePath = Path.Combine(WebRootPath + "\\Content\\\\Resumes\\", fileName);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.Export(rpt, fs);
                fs.Flush();
            }
            #endregion

            return filePath;
        }
    }
}
