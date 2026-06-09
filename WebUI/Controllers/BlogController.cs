using Application.CQRS.Blog_CQRS;
using Domain.Entities.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WebAPI.ViewModels;
using WebUI.Services;
namespace WebAPI.Controllers
{
    public class BlogController : ApiControllerBase
    {
        [HttpPost("AddOrUpdate")]
        public async Task<IActionResult> AddOrUpdate(CreateCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = CurrentUserService.GetUserId(HttpContext);

                var entity = await DbContext.Blogs.FindAsync(new object[] { command.Id });

                if (entity == null)
                {
                    entity = new Blog();
                    entity.Title = command.Title;
                    entity.Description = command.Description;
                    entity.FileName = command.FileName;
                    entity.CreateDate = DateTime.Now;
                    DbContext.Blogs.Add(entity);

                    if (command.Details != null)
                    {
                        var details = command.Details
                            .Select(m => new BlogDetail
                            {
                                BlogId = entity.Id,
                                Key = m.Key,
                                Value = m.Value,
                                CreateDate = DateTime.Now
                            });
                        DbContext.BlogDetails.AddRange(details);
                    }
                }
                else
                {
                    entity.Title = command.Title;
                    entity.Description = command.Description;
                    entity.FileName = command.FileName;

                    DbContext.BlogDetails
                        .RemoveRange(DbContext.BlogDetails.Where(m => m.BlogId == entity.Id));

                    if (command.Details != null)
                    {
                        var details = command.Details
                            .Select(m => new BlogDetail
                            {
                                BlogId = entity.Id,
                                Key = m.Key,
                                Value = m.Value,
                                CreateDate = DateTime.Now
                            });
                        DbContext.BlogDetails.AddRange(details);
                    }
                }

                await DbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("Delete/{Id}")]
        [HttpGet]
        public async Task<ActionResult> Delete(Guid Id)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var entity = await DbContext.Blogs.FindAsync(new object[] { Id });

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

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll(FilterParamsViewModel model)
        {
            try
            {
                var userId = CurrentUserService.GetUserId(HttpContext);

                var query = await DbContext.Blogs.AsNoTracking()
                        .Where(m => !m.Deleted)
                        .Select(m => new
                        {
                            m.Id,
                            m.Title,
                            m.Description,
                            m.FileName,
                            m.FileType,

                            Details = m.Details.Where(f => !f.Deleted).Select(f => new
                            {
                                f.Id,
                                f.Key,
                                f.Value,
                                CreateDate = f.CreateDate.Value
                            }).ToList(),

                            CreateDate = m.CreateDate.Value,
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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var query = await DbContext.Blogs.AsNoTracking()
                        .Where(m => !m.Deleted)
                        .Select(m => new
                        {
                            m.Id,
                            m.Title,
                            m.Description,
                            m.FileName,
                            m.FileType,

                            Details = m.Details.Where(f => !f.Deleted).Select(f => new
                            {
                                f.Id,
                                f.Key,
                                f.Value,
                                CreateDate = f.CreateDate.Value
                            }).ToList(),

                            CreateDate = m.CreateDate.Value,
                        })
                        .ToListAsync();

                return Ok(query);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("GetById/{Id}")]
        public async Task<IActionResult> GetById(Guid Id)
        {
            try
            {
                var query = await DbContext.Blogs.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.Id == Id)
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        m.FileName,
                        m.FileType,

                        Details = m.Details.Where(f => !f.Deleted).Select(f => new
                        {
                            f.Id,
                            f.Key,
                            f.Value,
                            CreateDate = f.CreateDate.Value
                        }).ToList(),

                        CreateDate = m.CreateDate.Value,
                    })
                    .SingleOrDefaultAsync();

                return Ok(query);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
