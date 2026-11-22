using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebUI.Services;

namespace WebAPI.CustomFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly HashSet<string> _allowedRoles;

        public CustomAuthorizeAttribute(string RoleName)
        {
            _allowedRoles = new HashSet<string>(
                (RoleName ?? string.Empty).Split(',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.Ordinal);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var validatedRoles = CurrentUserService.ValidateJwtToken(
                    context.HttpContext.Request.Headers["Token"].ToString());
                var userRoles = (validatedRoles ?? string.Empty).Split(',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // Require a complete role name. Empty claims and partial matches
                // must never grant access to a protected endpoint.
                if (!_allowedRoles.Overlaps(userRoles))
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
