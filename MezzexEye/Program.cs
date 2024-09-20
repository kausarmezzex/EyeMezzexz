using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EyeMezzexz.Controllers;
using MezzexEye.Services;
using MezzexEye.Controllers;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
// Add logging
builder.Logging.AddConsole();
builder.Services.AddTransient<WebServiceClient>(); // Register WebServiceClient
builder.Services.AddTransient<UserService>(); // Register UserService
builder.Services.AddTransient<DataController>();
builder.Services.AddTransient<AccountApiController>();
builder.Services.AddTransient<ApiService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddTransient<TeamAssignmentApiController>();
builder.Services.AddScoped<DataForViewController>();
builder.Services.AddScoped<AccountController>();
// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext with SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("E-CommDConnectionString")));


// Register Identity services
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure session services (if needed)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure cookie settings for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Login/Index";
    options.LogoutPath = "/Login/Logout";
    options.AccessDeniedPath = "/Login/AccessDenied";
});

// Configure authorization policies (if any)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CreateCategoryPolicy", policy =>
        policy.RequireClaim("Permission", "CreateCategory"));
    options.AddPolicy("CreateBrandPolicy", policy =>
        policy.RequireClaim("Permission", "CreateBrand"));
    options.AddPolicy("CreateProductPolicy", policy =>
        policy.RequireClaim("Permission", "CreateProduct"));
    options.AddPolicy("ManageSettingsPolicy", policy =>
        policy.RequireClaim("Permission", "ManageSettings"));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
