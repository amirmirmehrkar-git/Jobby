using Domain.Entities;
using System.Diagnostics.SymbolStore;

namespace WebAPI.ViewModels
{
    public class ResumeResult
    {
        public List<Skill> Skills { get; set; } = new List<Skill>();
        public List<Experience> Experiences { get; set; } = new List<Experience>();
        public List<Education> Educations { get; set; } = new List<Education>();
        public List<Certificate> Certificates { get; set; } = new List<Certificate>();
        public List<string> JobTitles { get; set; } = new List<string>();
        //public string JobTitle { get; set; } = string.Empty;
        public List<ResumeAnalysisItem> ResumeAnalysis { get; set; }
        public string Summary { get; set; }
    }
    public class ResumeAnalysisItem
    {
        public string Dimension { get; set; }
        public int Score { get; set; }
        public string Comments { get; set; }
    }
}
