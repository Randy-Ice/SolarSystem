
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using SolarSystem.Data;
using SolarSystem.Middleware;

namespace SolarSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            //Odata
            builder.Services.AddControllers()
                .AddOData(options => options.Select().Filter().OrderBy().Count().SetMaxTop(100));
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //Connection string
            Env.Load();
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            //Database
            builder.Services.AddDbContext<ProjectDB>(
                options => options.UseSqlServer(connectionString)
                );
            //Auth
            builder.Services.AddDbContext<AuthDB>(
                options => options.UseSqlServer(connectionString));
            builder.Services.AddAuthentication();
            builder.Services.AddIdentityApiEndpoints<IdentityUser>()
                .AddEntityFrameworkStores<AuthDB>();
            //Logging
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Information() //.Debug, .Information, .Warning, .Error, .Fatal
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);


            //Health  + rate limiting
            builder.Services.AddHealthChecks();
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 100;
                    opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 5;
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            app.UseMiddleware<HandleExceptionMidleware>();
            app.UseRateLimiter();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHealthChecks("/health");
            app.MapIdentityApi<IdentityUser>();


            app.MapControllers();

            app.Run();
        }
    }
}
