using Auth0.AspNetCore.Authentication;
using Infrastructure.Repositories;
using InventoryManagement.Application.Mapping;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infastructure.Identity;
using InventoryManagement.Infastructure.Persistence;
using InventoryManagement.Infastructure.Repositories;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.WEB.Components;
using InventoryManagement.WEB.Controollers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
.AddAuth0WebAppAuthentication(options => {
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

//builder.Services.AddRazorPages();
//builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IClaimsTransformation, ClaimsTransformation>();

builder.Services.AddHttpClient<Auth0Service>();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ItemService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<LocationService>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<TransactionService>();

builder.Services.AddScoped<IAuth0Repository, Auth0Repository>();
builder.Services.AddScoped<Auth0Service>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<AccountService>();

builder.Services.AddScoped<ReportService>();

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7025/"); // Замени на свой API URL
});


//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapHub<InventoryHub>("/inventoryHub");

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication(); // new line
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

//app.MapBlazorHub();

app.Run();
