using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Extensions
{
    public static class AccountExtensions
    {
        public static async Task<ApplicationUser?> FindByPhoneAsync(this UserManager<ApplicationUser> userManager, string phoneNumber)
        {
            return await userManager.Users.FirstOrDefaultAsync(u => u.UserName == phoneNumber);
        }
    }
}
