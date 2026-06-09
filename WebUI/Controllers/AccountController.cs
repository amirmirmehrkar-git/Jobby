using Application.Common.Interfaces;
using Application.CQRS.User_CQRS;
using DevExpress.Internal;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Playwright;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebAPI.Controllers;
using WebAPI.Extensions;
using WebAPI.ViewModels;
using WebUI.Services;
using WebUI.ViewModels.Linkedin;

namespace WebUI.Controllers
{
    public class AccountController : ApiControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        protected readonly IJwtService jwtService;
        protected readonly SignInManager<ApplicationUser> signInManager;
        protected readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;

        public AccountController(
               IJwtService JwtService
             , SignInManager<ApplicationUser> SignInManager
            , UserManager<ApplicationUser> UserManager
            , IConfiguration configuration
            , IWebHostEnvironment hostingEnvironment)
        {
            signInManager = SignInManager;
            userManager = UserManager;
            jwtService = JwtService;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        [Route("Login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound();
            }

            var status = await signInManager.PasswordSignInAsync(user.UserName, model.Password, true, false);
            if (status.Succeeded)
            {
                await signInManager.SignOutAsync();
                await signInManager.SignInAsync(user, true);


                var userId = jwtService.GenerateJwtToken(string.Join(",", user.Id));
                Response.Cookies.Append("TokenId", userId);

                var roles = userManager.GetRolesAsync(user).Result;
                var token = jwtService.GenerateJwtToken(string.Join(",", roles));
                Response.Cookies.Append("Token", token);

                return Ok(new
                {
                    success = true,
                    role = roles.Any(m => m.Contains("READ")) ? "Admin" : roles.FirstOrDefault(),
                    TokenId = userId,
                    Token = token
                });
            }
            else
            {
                ModelState.AddModelError("Messages", "The username or password is incorrect.");
                return BadRequest(ModelState);
            }
        }

        [AllowAnonymous]
        [Route("SignUp")]
        [HttpPost]
        public async Task<IActionResult> SignUp(AddOrUpdateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var query = await userManager.FindByPhoneAsync(model.Email);
                if (query == null)
                {
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        PhoneNumber = "",
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        EmailConfirmed = true,
                        Deleted = false,
                        UserType = Domain.Enums.UserType.User,
                        CreateDate = DateTime.Now
                    };
                    await userManager.CreateAsync(user, model.Password);

                    await userManager.AddToRolesAsync(user, new[] { "User" });

                    DbContext.Wallets.Add(new Wallet
                    {
                        Amount = 0,
                        UserId = user.Id,
                        Deleted = false,
                    });
                    await DbContext.SaveChangesAsync();
                }
                else
                {
                    query.FirstName = model.FirstName;
                    query.LastName = model.LastName;
                    await userManager.UpdateAsync(query);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("loginWithToken/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> loginWithToken(string Id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            await signInManager.SignOutAsync();
            await signInManager.SignInAsync(user, true);

            var userId = jwtService.GenerateJwtToken(string.Join(",", user.Id));
            Response.Cookies.Append("TokenId", userId);

            var roles = userManager.GetRolesAsync(user).Result;
            var token = jwtService.GenerateJwtToken(string.Join(",", roles));
            Response.Cookies.Append("Token", token);

            return Ok(new
            {
                success = true,
                role = roles.Any(m => m.Contains("READ")) ? "Admin" : roles.FirstOrDefault(),
                TokenId = userId,
                Token = token
            });
        }

        //[CustomAuthorize("Admin,User")]
        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = CurrentUserService.GetUserId(HttpContext);

                var query = DbContext.Users.AsNoTracking()
                  .Where(m => !m.Deleted)
                  .Where(m => m.Id == userId)
                  .ToList()
                  .Select(m => new GetProfileDto
                  {
                      Id = m.Id,
                      PhoneNumber = m.PhoneNumber,
                      Email = m.Email,
                      FirstName = m.FirstName,
                      LastName = m.LastName,
                      FullName = m.FirstName + " " + m.LastName,
                      Address = m.Address,
                      ImageName = m.ImageName,
                      AboutMe = m.AboutMe,
                      ApplyType = m.ApplyType,
                      LinkedInUserName = string.IsNullOrWhiteSpace(m.Linkdin) ? "" : m.Linkdin.Split(',')[0],
                      LinkedInPassword = string.IsNullOrWhiteSpace(m.Linkdin) ? "" : m.Linkdin.Split(',')[1],
                      StepStoneUserName = string.IsNullOrWhiteSpace(m.Stepstone) ? "" : m.Stepstone.Split(',')[0],
                      StepStonePassword = string.IsNullOrWhiteSpace(m.Stepstone) ? "" : m.Stepstone.Split(',')[1],
                      XingUserName = string.IsNullOrWhiteSpace(m.Xing) ? "" : m.Xing.Split(',')[0],
                      XingPassword = string.IsNullOrWhiteSpace(m.Xing) ? "" : m.Xing.Split(',')[1],

                      WalletAmount = 0,
                      NeededJob = "",
                  })
                  .SingleOrDefault();

                var wallet = await DbContext.Wallets.AsNoTracking()
                    .Where(m => !m.Deleted)
                    .Where(m => m.UserId == userId)
                    .SingleOrDefaultAsync();
                query.WalletAmount = wallet == null ? 0 : wallet.Amount;

                var jobTitles = await DbContext.Positions.AsNoTracking()
                    .Where(m => m.UserId == userId)
                    .ToListAsync();
                query.NeededJob = jobTitles.Count() == 0
                    ? ""
                    : jobTitles.Where(m => m.IsSelected.Value == true).FirstOrDefault() == null
                    ? jobTitles.FirstOrDefault().Title
                    : jobTitles.Where(m => m.IsSelected.Value == true).FirstOrDefault().Title;

                return Ok(query);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult LogOff()
        {
            return RedirectToAction("Index", "Home");
        }

        /////////////////////////////////////////////////////////////////

        [HttpGet("linkedin-login")]
        public IActionResult LinkedInLogin()
        {
            string clientId =
                _configuration["LinkedIn:ClientId"];

            string redirectUri =
                "https://localhost:44306/api/Account/linkedin-callback";

            string state = Guid.NewGuid().ToString();

            string url =
                $"https://www.linkedin.com/oauth/v2/authorization" +
                $"?response_type=code" +
                $"&client_id={clientId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&scope=openid profile email" +
                $"&state={state}";

            return Redirect(url);
        }


        [HttpGet("linkedin-callback")]
        public async Task<IActionResult> LinkedInCallback(
        string code)
        {
            string clientId =
                _configuration["LinkedIn:ClientId"];

            string clientSecret =
                _configuration["LinkedIn:ClientSecret"];

            string redirectUri =
                "https://localhost:44306/api/Account/linkedin-callback";

            using var httpClient = new HttpClient();

            var values =
                new Dictionary<string, string>
                {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "client_secret", clientSecret }
                };

            var tokenResponse =
                await httpClient.PostAsync(
                    "https://www.linkedin.com/oauth/v2/accessToken",
                    new FormUrlEncodedContent(values));

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

            var token = JsonSerializer.Deserialize<LinkedInTokenResponse>(tokenJson);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token.access_token);


            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.id_token);

            var email = jwt.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

            var name = jwt.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
            var firstName = name.Split(' ')[0];
            var lastName = name.Replace(firstName + " ", "");

            var givenName = jwt.Claims.FirstOrDefault(x => x.Type == "given_name")?.Value;
            var picture = jwt.Claims.FirstOrDefault(x => x.Type == "picture")?.Value;

            string fileName = null;
            if (!string.IsNullOrWhiteSpace(picture))
            {
                var folderPath = _hostingEnvironment.WebRootPath + "\\Content\\Images\\ProfileImages\\";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                fileName = Guid.NewGuid() + ".jpg";
                string savePath = Path.Combine(folderPath, fileName);
                using var httpClient2 = new HttpClient();
                var imageBytes = await httpClient2.GetByteArrayAsync(picture);
                await System.IO.File.WriteAllBytesAsync(
                    savePath,
                    imageBytes);
            }

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    PhoneNumber = "",
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true,
                    Deleted = false,
                    UserType = Domain.Enums.UserType.User,
                    CreateDate = DateTime.Now,
                    ImageName = fileName
                };
                await userManager.CreateAsync(user, email);

                await userManager.AddToRolesAsync(user, new[] { "User" });

                DbContext.Wallets.Add(new Wallet
                {
                    Amount = 0,
                    UserId = user.Id,
                    Deleted = false,
                });
                await DbContext.SaveChangesAsync();
            }
            else
            {
                existingUser.FirstName = firstName;
                existingUser.LastName = lastName;
                existingUser.ImageName = fileName;
                await userManager.UpdateAsync(existingUser);
            }

            var userId = jwtService.GenerateJwtToken(string.Join(",", existingUser.Id));
            Response.Cookies.Append("TokenId", userId);

            var roles = userManager.GetRolesAsync(existingUser).Result;
            var resToken = jwtService.GenerateJwtToken(string.Join(",", roles));
            Response.Cookies.Append("Token", resToken);

            //return Ok(new
            //{
            //    success = true,
            //    role = roles.Any(m => m.Contains("READ")) ? "Admin" : roles.FirstOrDefault(),
            //    TokenId = userId,
            //    Token = resToken
            //});

            return Redirect("https://localhost:44306/#!/login/" + userId);
        }

        [HttpGet("ChangePassword/{OldPassword}/{NewPassword}/{NewPasswordConfirm}")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(string OldPassword, string NewPassword, string NewPasswordConfirm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (NewPassword != NewPasswordConfirm)
            {
                ModelState.AddModelError("Messages", "New password and confirm must be exactly the same.");
                return BadRequest(ModelState);
            }

            var userId = CurrentUserService.GetUserId(HttpContext);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var s = await userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
            if (s.Succeeded)
            {
                return Ok();
            }
            else
            {
                //ModelState.AddModelError("Messages", "Current password is not correct.");
                ModelState.AddModelError("Messages", s.Errors.FirstOrDefault().Description);
                return BadRequest(ModelState);
            }
        }

        [HttpGet("DeleteAccount")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAccount()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = CurrentUserService.GetUserId(HttpContext);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            await DbContext.Wallets
                            .Where(m => m.UserId == userId)
                            .ExecuteDeleteAsync();

            var s = await userManager.DeleteAsync(user);
            if (s.Succeeded)
            {
                return Ok();
            }
            else
            {
                ModelState.AddModelError("Messages", s.Errors.FirstOrDefault().Description);
                return BadRequest(ModelState);
            }
        }
    }
}