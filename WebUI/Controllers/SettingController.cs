using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.CustomFilters;
using WebUI.Services;
namespace WebAPI.Controllers;
public class SettingController : ApiControllerBase
{
    [CustomAuthorize("Admin,User")]
    [HttpGet("AddOrUpdate/{Key}")]
    public async Task<IActionResult> AddOrUpdate(string? Key)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);

            var entity = await DbContext.UserSettings
                .Where(m => m.UserId == userId && m.Key == Key)
                .SingleOrDefaultAsync();

            if (entity == null)
            {
                entity = new UserSetting();
                entity.Key = Key;
                entity.Value = "true";
                entity.UserId = userId;

                DbContext.UserSettings.Add(entity);
            }
            else
            {
                entity.Value = (!Convert.ToBoolean(entity.Value)).ToString();
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
    [HttpGet("GetAllSettingsByUserId")]
    public async Task<IActionResult> GetAllSettingsByUserId()
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);

            var entity = DbContext.UserSettings.AsNoTracking()
                .Where(m => m.UserId == userId)
                .ToList()
                .Select(m => new
                {
                    m.Id,
                    m.Key,
                    Value = Convert.ToBoolean(m.Value)
                })
                .ToList();

            return Ok(entity);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
