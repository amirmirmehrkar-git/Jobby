using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    public ApiControllerBase()
    {
    }

    private IApplicationDbContext _dbContext = null!;
    protected IApplicationDbContext DbContext =>
        _dbContext ??= HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();

}
