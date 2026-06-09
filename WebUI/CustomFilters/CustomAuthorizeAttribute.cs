using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using WebUI.Services;
using static Infrastructure.Services.JwtService;

namespace WebAPI.CustomFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        string _RoleName;

        public CustomAuthorizeAttribute(string RoleName)
        {
            _RoleName = RoleName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var validatedToken = CurrentUserService.ValidateJwtToken(context.HttpContext.Request.Headers["Token"].ToString());

                if (!validatedToken.Contains(_RoleName) && !_RoleName.Contains(validatedToken))
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                }
            }
            catch (Exception)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }
    }
}
