using Application.Common.Interfaces;
using FluentValidation.AspNetCore;
using Infrastructure.Services;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace WebUI
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddWebUIServices(this IServiceCollection services)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IJwtService, JwtService>();

            services.AddHttpContextAccessor();

            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

            services.AddControllersWithViews().AddFluentValidation(x => x.AutomaticValidationEnabled = false);

            services.AddRazorPages();

            //services.AddMvc(options =>
            //{
            //    options.Filters.Add<FeatureFlagAttribute>();
            //});

            // Customise default API behaviour
            services.Configure<ApiBehaviorOptions>(options =>
                options.SuppressModelStateInvalidFilter = true);

            //services.AddOpenApiDocument(configure =>
            //{
            //    configure.Title = "AspNetCoreTest API";
            //    configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            //    {
            //        Type = OpenApiSecuritySchemeType.ApiKey,
            //        Name = "Authorization",
            //        In = OpenApiSecurityApiKeyLocation.Header,
            //        Description = "Type into the textbox: Bearer {your JWT token}."
            //    });

            //    configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            //});

            return services;
        }
    }
}
