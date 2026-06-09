using Application.CQRS.Email_CQRS;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebAPI.CustomFilters;
using WebAPI.ViewModels;
using WebUI.Services;

namespace WebAPI.Controllers;

public class EmailController : ApiControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IConfiguration configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _openAiKey;
    private readonly string _chatModel;
    protected readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager;

    public EmailController(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> UserManager,
        IHttpClientFactory httpClientFactory, IConfiguration config, IWebHostEnvironment hostingEnvironment)
    {
        _httpClientFactory = httpClientFactory;
        _openAiKey = config.GetConnectionString("OpenAiKey");
        _chatModel = config["OPENAI_CHAT_MODEL"] ?? "gpt-4o-mini";
        configuration = config;
        userManager = UserManager;
        _hostingEnvironment = hostingEnvironment;
    }


    [CustomAuthorize("Admin,User")]
    [HttpPost("GenerateAnswerForEmail")]
    public async Task<IActionResult> GenerateAnswerForEmail(GenerateAnswerForEmailCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ai = new AI(_httpClientFactory, _chatModel, _openAiKey);
            var ret = await ai.GenerateResponse(command.Message);
            var res = JsonSerializer.Deserialize<GenerateResponseViewModel>(ret);

            return Ok(res.text);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
