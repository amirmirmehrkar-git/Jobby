using Application.CQRS.Document_CQRS;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WebAPI.CustomFilters;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers;

public class DocumentController : ApiControllerBase
{
    [CustomAuthorize("Admin,User")]
    [HttpPost("AddOrUpdate")]
    public async Task<IActionResult> AddOrUpdate(CreateCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);

            var entity = await DbContext.Documents
                .FindAsync(new object[] { command.Id });

            if (entity == null)
            {
                entity = new Document();
                entity.Title = command.Title;
                entity.FileName = command.FileName;
                entity.FileType = GetDefaultExtension(command.FileName);
                entity.UserId = userId;

                DbContext.Documents.Add(entity);
            }
            else
            {
                entity.Title = command.Title;
                entity.FileName = command.FileName;
                entity.FileType = GetDefaultExtension(command.FileName);
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
    [HttpGet("AddOrUpdateResume/{FileName}")]
    public async Task<IActionResult> AddOrUpdateResume(string FileName)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);

            var entity = new Document();
            entity.FileName = FileName;
            entity.FileType = GetDefaultExtension(FileName);
            entity.IsResume = true;
            entity.UserId = userId;

            DbContext.Documents.Add(entity);

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

            var query = await DbContext.Documents.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => !m.IsResume.Value)
                    .Where(m => m.UserId == userId)
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.FileName,
                        m.FileType,
                        m.CreateDate,
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
    [HttpPost("GetAllResumeByUserId")]
    public async Task<IActionResult> GetAllResumeByUserId(FilterParamsViewModel model)
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);

            var query = await DbContext.Documents.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.IsResume.Value)
                    .Where(m => m.UserId == userId)
                    .Select(m => new
                    {
                        m.Id,
                        m.FileName,
                        CreateDate = m.CreateDate.Value.ToShortDateString(),
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

            var entity = await DbContext.Documents.FindAsync(new object[] { Id });

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

    public static FileType GetDefaultExtension(string mimeType)
    {
        FileType res = FileType.Pdf;
        if (mimeType.Contains("jpeg") || mimeType.Contains("png") || mimeType.Contains("jpg"))
        {
            res = FileType.Image;
        }
        else if (mimeType.Contains("mp4"))
        {
            res = FileType.Video;
        }
        //else if (mimeType.Contains("mp3") || mimeType.Contains("mpeg"))
        //{
        //    result = ".mp3";
        //}
        //else if (mimeType.Contains("sheet"))
        //{
        //    result = ".xlsx";
        //}
        return res;
    }
}
