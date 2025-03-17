using Auth0.AspNetCore.Authentication;
using Infrastructure.Repositories;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Mapping;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infastructure.Hubs;
using InventoryManagement.Infastructure.Identity;
using InventoryManagement.Infastructure.Persistence;
using InventoryManagement.Infastructure.Repositories;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.WEB.Components;
using InventoryManagement.WEB.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog; 

namespace InventoryManagement.WEB
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services
            .AddAuth0WebAppAuthentication(options =>
            {
                options.Domain = builder.Configuration["Auth0:Domain"];
                options.ClientId = builder.Configuration["Auth0:ClientId"];
                options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
            })
            .WithAccessToken(options =>
            {
                options.Audience = builder.Configuration["Auth0:Audience"];
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
                options.AddPolicy("Employee", policy => policy.RequireRole("Employee"));
            });
            builder.Services.AddControllersWithViews();

            builder.Services.AddSignalR();

            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IClaimsTransformation, ClaimsTransformation>();

            builder.Services.AddHttpClient<Auth0Service>();

            builder.Services.AddScoped<IItemRepository, ItemRepository>();
            builder.Services.AddScoped<IItemService, ItemService>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<ILocationRepository, LocationRepository>();
            builder.Services.AddScoped<ILocationService, LocationService>();

            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();

            builder.Services.AddScoped<IAuth0Repository, Auth0Repository>();
            builder.Services.AddScoped<IAuth0Service, Auth0Service>();

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IAccountService, AccountService>();

            builder.Services.AddScoped<IReportService, ReportService>();

            builder.Services.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7025/");
            });

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "InventoryManagement API",
                    Version = "v1",
                    Description = "API for managing inventory, users, and transactions.",
                    Contact = new OpenApiContact
                    {
                        Name = "Ilya Maksimovich",
                        Url = new Uri("https://github.com/Raiver103/InventoryManagement")
                    }
                });

                // ¬ключаем XML-документацию (см. шаг 3)
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });


            var app = builder.Build();
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);

                app.UseHsts();
            }

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseSerilogRequestLogging();

            app.MapHub<InventoryHub>("/inventoryHub");

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode();

            app.Run();
        }
    } 
}
