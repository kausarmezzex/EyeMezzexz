using E_Commerce_Mezzex.Data;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using MezzexEye.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddTransient<WebServiceClient>();

// Configure DbContext with SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("E-CommDConnectionString")));

builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<UserService>();

// Add session services
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

// Configure CORS
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

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        SeedDatabase(applicationDbContext);
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Initialize the seed data
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

void SeedDatabase(ApplicationDbContext context)
{
    try
    {
        // List of task names
        var tasks = new List<string>
        {
            "Listing", "Costing", "New Product Work", "Shipment", "Live Stock",
            "Clearance", "Promotion", "Amazon Emails", "Ebay Email", "DP Ordering",
            "AST Ordering", "Supplier Invoice", "VAT Invoice Entry", "Break",
            "Other", "Credit Note", "Inventory Check", "Lunch", "Washroom",
            "Out Of Stock", "Meeting"
        };

        foreach (var taskName in tasks)
        {
            if (!context.TaskNames.Any(t => t.Name == taskName))
            {
                context.TaskNames.Add(new TaskNames { Name = taskName });
            }
        }

        context.SaveChanges();
        Console.WriteLine("Tasks have been seeded to the database.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}
