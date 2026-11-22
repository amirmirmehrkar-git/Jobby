using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Services;
using Application.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using WebAPI.CustomFilters;
using WebUI.Services;

// Exercise the application's real token decoder without starting the web host
// or constructing its database-backed services. Every key is ephemeral test data.
var previousKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");
var signingKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
var checks = 0;

void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    checks++;
}

void Reject(Action action, string message)
{
    try
    {
        action();
    }
    catch (Exception)
    {
        checks++;
        return;
    }
    throw new InvalidOperationException(message);
}

bool IsAuthorized(string allowedRoles, string token)
{
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["Token"] = token;
    var context = new AuthorizationFilterContext(
        new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
        new List<IFilterMetadata>());
    new CustomAuthorizeAttribute(allowedRoles).OnAuthorization(context);
    if (context.Result is null) return true;
    if (context.Result is StatusCodeResult { StatusCode: StatusCodes.Status403Forbidden }) return false;
    throw new InvalidOperationException("Unexpected authorization response.");
}

try
{
    Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", signingKey);
    var token = new JwtService(null!).GenerateJwtToken("User");
    Check(CurrentUserService.ValidateJwtToken(token) == "User",
        "The application's valid token must round-trip through its decoder.");

    var parts = token.Split('.');
    parts[2] = (parts[2][0] == 'A' ? "B" : "A") + parts[2][1..];
    Reject(() => CurrentUserService.ValidateJwtToken(string.Join('.', parts)),
        "A modified signature must be rejected.");

    Environment.SetEnvironmentVariable("JWT_SIGNING_KEY",
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
    Reject(() => CurrentUserService.ValidateJwtToken(token),
        "A token signed by a different key must be rejected.");
    Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", signingKey);

    var expired = new JwtSecurityToken(
        claims: new[] { new Claim("Roles", "User") },
        notBefore: DateTime.UtcNow.AddHours(-2),
        expires: DateTime.UtcNow.AddHours(-1),
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256));
    Reject(() => CurrentUserService.ValidateJwtToken(
        new JwtSecurityTokenHandler().WriteToken(expired)),
        "An expired token must be rejected.");

    Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", null);
    Reject(() => CurrentUserService.ValidateJwtToken(token),
        "Missing signing configuration must fail closed.");
    Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", signingKey);

    var roleCases = new (string Allowed, string Granted, bool Expected)[]
    {
        ("Admin", "Admin", true),
        ("Admin", "User", false),
        ("Admin", "SuperAdmin", false),
        ("Admin", "Adm", false),
        ("READ_USERS", "READ_USER", false),
        ("Admin", "", false),
        ("Admin", " , , ", false),
        ("Admin,READ_USERS", "User,READ_USERS", true),
        (" Admin ,READ_USERS", " User , Admin ", true),
        ("Admin", "admin", false),
        ("", "Admin", false),
        (" , ", "User", false),
        ("Admin,User", "Admin,User", true),
    };
    foreach (var roleCase in roleCases)
    {
        var signedRoleToken = new JwtService(null!).GenerateJwtToken(roleCase.Granted);
        Check(IsAuthorized(roleCase.Allowed, signedRoleToken) == roleCase.Expected,
            $"Role matching failed for allowed '{roleCase.Allowed}' and granted '{roleCase.Granted}'.");
    }
    Check(!IsAuthorized("Admin", ""), "A missing token must be forbidden.");
    Check(!IsAuthorized("Admin", "invalid.token.value"), "A malformed token must be forbidden.");
    var missingRoles = new JwtSecurityToken(
        expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256));
    Check(!IsAuthorized("Admin", new JwtSecurityTokenHandler().WriteToken(missingRoles)),
        "A signed token with no role claim must be forbidden.");

    // Controllers use client-supplied property names for dynamic table sorting.
    // Preserve property sorting while enforcing the patched parser's restrictions.
    var rows = new[] { new { Name = "Beta", Id = 2 }, new { Name = "Alpha", Id = 1 } };
    var sorted = rows.AsQueryable().OrderBy("Name").ToArray();
    Check(sorted[0].Id == 1, "Property-name sorting must remain supported.");
    sorted = rows.AsQueryable().OrderBy("Name descending").ToArray();
    Check(sorted[0].Id == 2, "Descending property sorting must remain supported.");
    try
    {
        rows.AsQueryable().OrderBy("GetType().Assembly.FullName").ToArray();
        throw new InvalidOperationException("Method expressions must not be accepted as sort fields.");
    }
    catch (ParseException)
    {
        checks++;
    }

    var payment = new PaymentService(null!);
    Check(!await payment.VerifyBuy("unverified-reference"),
        "The unimplemented payment service must never confirm an unverified payment.");
    Check(!await payment.VerifyBuy(""), "A missing payment reference must be rejected.");
    Reject(() => payment.GenerateBuyUrl(new BuyPlanViewMode()).GetAwaiter().GetResult(),
        "An unconfigured payment service must not generate a checkout URL.");

    Console.WriteLine($"Dependency smoke checks passed: {checks}.");
}
finally
{
    Environment.SetEnvironmentVariable("JWT_SIGNING_KEY", previousKey);
}
