using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    //await _context.Database.MigrateAsync();
                    await _context.Database.EnsureCreatedAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "در ایجاد پایگاه داده خطایی رخ داد.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "در بارگزاری اطلاعات اولیه خطایی رخ داد.");
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            // نقش های اولیه
            var administratorRole = new IdentityRole("Admin");
            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }
            var userRole = new IdentityRole("User");
            if (_roleManager.Roles.All(r => r.Name != userRole.Name))
            {
                await _roleManager.CreateAsync(userRole);
            }

            // کاربران اولیه
            var administrator = new ApplicationUser
            {
                UserName = "adminA@yahoo.com",
                Email = "adminA@yahoo.com",
                FirstName = "Main",
                LastName = "Admin",
                FatherName = "AADDMMIINN",
                Deleted = false,
                UserType = UserType.Admin
            };
            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                var a = await _userManager.CreateAsync(administrator, "admin");
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }

            await _context.SaveChangesAsync();
        }
    }
}
