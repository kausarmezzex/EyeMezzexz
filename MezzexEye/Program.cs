using EyeMezzexz.Data;
using EyeMezzexz.Services;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using EyeMezzexz.Controllers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<ApiService>();

// Add the DataForViewController with dependency injection
builder.Services.AddTransient<DataForViewController>();

// Add logging
builder.Services.AddLogging();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext with SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("E-CommDConnectionString")));

// Register ApiService with HttpClient
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri("https://smapi.mezzex.com/"); // Ensure this is your correct API base URL
});

// Configure Identity services
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

// Configure session services
builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Login/Index"; // Update with your login path
    options.LogoutPath = "/Login/Logout";
    options.AccessDeniedPath = "/Login/AccessDenied";
});

// Configure authorization policies
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authentication and authorization
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
