using Domain.Entities;
using Domain.Entities.Blog;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<UserSetting> UserSettings { get; set; }
        DbSet<Position> Positions { get; set; }
        DbSet<Blog> Blogs { get; set; }
        DbSet<BlogDetail> BlogDetails { get; set; }
        DbSet<Wallet> Wallets { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<Question> Questions { get; set; }
        DbSet<Certificate> Certificates { get; set; }
        DbSet<Document> Documents { get; set; }
        DbSet<Education> Educations { get; set; }
        DbSet<Experience> Experiences { get; set; }
        DbSet<Skill> Skills { get; set; }
        DbSet<ResponseSuggest> ResponseSuggests { get; set; }
        DbSet<Suggest> Suggests { get; set; }
        DbSet<IdentityUserRole<string>> AspNetUserRoles { get; set; }
        DbSet<ApplicationRole> AspNetRoles { get; set; }
        DbSet<ApplicationUser> Users { get; set; }
        DbSet<Ticket> Tickets { get; set; }
        DbSet<TicketMessage> TicketMessages { get; set; }

        Task<int> SaveChangesAsync();
    }
}
