using Application.CQRS.Ticket_CQRS;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WebAPI.CustomFilters;
using WebAPI.Helper;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers;

public class TicketController : ApiControllerBase
{
    [CustomAuthorize("Admin,User,READ_TICKETS")]
    [HttpPost("AddOrUpdate")]
    public async Task<IActionResult> AddOrUpdate(CreateCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.UserId = CurrentUserService.GetUserId(HttpContext);

            var entity = await DbContext.Tickets
                .FindAsync(new object[] { command.Id });

            if (entity == null)
            {
                entity = new Ticket();
                entity.Title = command.Title;
                entity.Number = (DbContext.Tickets.AsNoTracking().Count() + 357159).ToString();
                entity.Priority = command.Priority;
                entity.Status = TicketStatus.Open;
                entity.UserId = command.UserId;
                DbContext.Tickets.Add(entity);

                var entity2 = new TicketMessage();
                entity2.TextMessage = command.TextMessage;
                entity2.FileName = command.FileName;
                entity2.FromId = command.UserId;
                entity2.TicketId = entity.Id;
                DbContext.TicketMessages.Add(entity2);
            }

            await DbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,User,READ_TICKETS")]
    [HttpPost("AddOrUpdateTicketMessage")]
    public async Task<IActionResult> AddOrUpdateTicketMessage(CreateTicketMessageCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = CurrentUserService.GetUserId(HttpContext);
            command.UserId = userId;

            var entity = await DbContext.TicketMessages
                .FindAsync(new object[] { command.Id });

            if (entity == null)
            {
                entity = new TicketMessage();
                entity.TextMessage = command.TextMessage;
                entity.FileName = command.FileName;
                entity.FromId = command.UserId;
                entity.TicketId = command.TicketId;
                DbContext.TicketMessages.Add(entity);

                var ticket = await DbContext.Tickets
                    .FindAsync(new object[] { command.TicketId });

                ticket.Status = Domain.Enums.TicketStatus.Open;
            }

            await DbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [CustomAuthorize("Admin,READ_TICKETS")]
    [HttpPost("GetAll")]
    public async Task<IActionResult> GetAll(FilterParamsViewModel model)
    {
        try
        {
            var query = await DbContext.Tickets.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Join(DbContext.Users.AsNoTracking(), t => t.UserId, u => u.Id, (t, u) => new
                    {
                        CreateDate = t.CreateDate.Value.ToShortDateString(),
                        LastModifiedDate = t.ModifiedDate.Value.ToShortDateString(),
                        t.Id,
                        t.Number,
                        t.Priority,
                        t.Status,
                        t.Title,
                        UserFullName = u.FirstName + " " + u.LastName,
                    })
                    .ToListAsync();

            if (model.Filter != null)
            {
                foreach (var item in model.Filter)
                {
                    if (item.Key == "Title")
                    {
                        var val = item.Value[0];
                        query = query.Where(m => m.Title.Contains(val)).ToList();
                    }
                    else if (item.Key == "Number")
                    {
                        var val = item.Value[0];
                        query = query.Where(m => m.Number == val).ToList();
                    }
                    else if (item.Key == "UserFullName")
                    {
                        var val = item.Value[0];
                        query = query.Where(m => m.UserFullName == val).ToList();
                    }
                    //else if (item.Key == "LastModifiedDate")
                    //{

                    //    var val = item.Value[0];
                    //    try
                    //    {
                    //        DateTime date = PersianDateTime.Parse(val).ToDateTime().Date;
                    //        query = query.Where(m => m.LastModifiedDate.Date == date).ToList();
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

    [CustomAuthorize("Admin,User,READ_TICKETS")]
    [HttpPost("GetAllByUserId")]
    public async Task<IActionResult> GetAllByUserId(FilterParamsViewModel model)
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);
            var query = await DbContext.Tickets.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == userId)
                    .Join(DbContext.Users.AsNoTracking(), t => t.UserId, u => u.Id, (t, u) => new
                    {
                        CreateDate = t.CreateDate.Value.ToShortDateString(),
                        LastModifiedDate = t.ModifiedDate.Value.ToShortDateString(),
                        t.Id,
                        t.Number,
                        t.Priority,
                        t.Status,
                        t.Title,
                        UserFullName = u.FirstName + " " + u.LastName,
                    })
                    .ToListAsync();

            if (model.Filter != null)
            {
                foreach (var item in model.Filter)
                {
                    if (item.Key == "Title")
                    {
                        var val = item.Value[0];
                        query = query.Where(m => m.Title.Contains(val)).ToList();
                    }
                    else if (item.Key == "Number")
                    {
                        var val = item.Value[0];
                        query = query.Where(m => m.Number == val).ToList();
                    }
                    else if (item.Key == "UserFullName")
                    {
                        var val = item.Value[0];
                        query = query.Where(m => m.UserFullName == val).ToList();
                    }
                    //else if (item.Key == "LastModifiedDate")
                    //{

                    //    var val = item.Value[0];
                    //    try
                    //    {
                    //        DateTime date = PersianDateTime.Parse(val).ToDateTime().Date;
                    //        query = query.Where(m => m.LastModifiedDate.Date == date).ToList();
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

    [CustomAuthorize("Admin,User,READ_TICKETS")]
    [Route("GetById/{Id}")]
    [HttpGet]
    public async Task<IActionResult> GetById(Guid Id)
    {
        try
        {
            var userId = CurrentUserService.GetUserId(HttpContext);
            var query = await DbContext.Tickets
                    .Include(x => x.Messages)
                    .Where(m => m.Id == Id)
                    .SingleOrDefaultAsync();

            if (query.Status == TicketStatus.Open)
            {
                if (query.Messages.ToList().LastOrDefault().FromId != userId)
                {
                    query.Status = TicketStatus.Seen;
                    await DbContext.SaveChangesAsync();
                }
            }

            var res = await DbContext.Tickets.AsNoTracking()
                .Where(m => !m.Deleted)
                .Where(m => m.Id == Id)
                .Join(DbContext.Users.AsNoTracking(), t => t.UserId, u => u.Id, (t, u) => new
                {
                    CreateDate = t.CreateDate.Value,
                    LastModifiedDate = t.ModifiedDate.Value,
                    t.Number,
                    t.Priority,
                    t.Status,
                    t.Title,
                    UserFullName = u.FirstName + " " + u.LastName,
                    Messages = t.Messages.Select(tm => new
                    {
                        CreateDate = tm.CreateDate.Value,
                        tm.TextMessage,
                        tm.FileName,
                        tm.Id,
                        Right = tm.FromId == userId ? true : false
                    })
                    .OrderByDescending(tm => tm.CreateDate).ToList()
                })
                .SingleOrDefaultAsync();

            return Ok(res);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
