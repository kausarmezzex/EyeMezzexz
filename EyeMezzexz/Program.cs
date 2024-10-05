using E_Commerce_Mezzex.Data;
using EyeMezzexz.Controllers;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using MezzexEye.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ServiceReference1;

var builder = WebApplication.CreateBuilder(args);

// Configure app to load appropriate appsettings file based on the environment
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("E-CommDConnectionString")));

// Configure Identity with default identity and roles
builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register custom services
builder.Services.AddTransient<WebServiceClient>(); // Register WebServiceClient
builder.Services.AddTransient<UserService>(); // Register UserService
builder.Services.AddScoped<DataController>(); // Register DataController as Scoped
builder.Services.AddScoped<AccountApiController>(); // Register AccountApiController as Scoped
builder.Services.AddScoped<TeamAssignmentApiController>(); // Register TeamAssignmentApiController as Scoped
builder.Services.AddScoped<TaskAssignmentController>();

// Add session services
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

// Configure CORS to allow any origin, method, and header
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Register IConfiguration as a singleton service
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
var configuration = builder.Configuration;

var baseUrl = configuration["EnvironmentSettings:BaseUrl"];
var uploadFolder = configuration["EnvironmentSettings:UploadFolder"];
var uploadPhysicalFolder = configuration["EnvironmentSettings:UploadPhysicalFolder"];

var app = builder.Build();

// Serve static files
app.UseStaticFiles();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        logger.LogInformation("Seeding data...");
        SeedData.Initialize(services, userManager).Wait();
        logger.LogInformation("Seeding data completed.");
    }
    catch (Exception ex)
    {
        logger.LogError($"An error occurred while seeding the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger for production, but restrict it if needed for security reasons
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger at the root URL
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.UseSession();
app.MapControllers();
app.Run();
