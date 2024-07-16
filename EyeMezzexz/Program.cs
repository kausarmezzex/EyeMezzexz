using E_Commerce_Mezzex.Data;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using EyeMezzexz.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceReference1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<WebServiceClient>();
// Configure DbContext with SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("E-CommDConnectionString")));
builder.Services.AddDbContext<CustomerData>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MezzexEye")));


builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();



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
        builder =>
        {
            builder.AllowAnyOrigin()
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
    var customerDataContext = services.GetRequiredService<CustomerData>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        SeedDatabase(applicationDbContext);
        SeedDatabaseCus(customerDataContext);
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
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

void SeedDatabase(ApplicationDbContext context)
{
    try
    {
        if (!context.Users.Any(u => u.Username == "testuser"))
        {
            context.Users.AddRange(new List<User>
            {
                new User { Username = "testuser", Password = BCrypt.Net.BCrypt.HashPassword("password") },
                new User { Username = "testuser1", Password = BCrypt.Net.BCrypt.HashPassword("password1") },
                new User { Username = "testuser2", Password = BCrypt.Net.BCrypt.HashPassword("password2") },
                new User { Username = "testuser3", Password = BCrypt.Net.BCrypt.HashPassword("password3") },
                new User { Username = "testuser4", Password = BCrypt.Net.BCrypt.HashPassword("password4") }
            });

            context.SaveChanges();
            Console.WriteLine("Users have been seeded to the database.");
        }
        else
        {
            Console.WriteLine("Users already exist in the database.");
        }

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
            if (!context.Tasks.Any(t => t.Name == taskName))
            {
                context.Tasks.Add(new TaskModel { Name = taskName });
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

void SeedDatabaseCus(CustomerData customerData)
{
    try
    {
        if (!customerData.Users.Any(u => u.Username == "testuser"))
        {
            customerData.Users.AddRange(new List<User>
            {
                new User { Username = "testuser", Password = BCrypt.Net.BCrypt.HashPassword("password") },
                new User { Username = "testuser1", Password = BCrypt.Net.BCrypt.HashPassword("password1") },
                new User { Username = "testuser2", Password = BCrypt.Net.BCrypt.HashPassword("password2") },
                new User { Username = "testuser3", Password = BCrypt.Net.BCrypt.HashPassword("password3") },
                new User { Username = "testuser4", Password = BCrypt.Net.BCrypt.HashPassword("password4") }
            });

            customerData.SaveChanges();
            Console.WriteLine("Users have been seeded to the database.");
        }
        else
        {
            Console.WriteLine("Users already exist in the database.");
        }
        customerData.SaveChanges();
        Console.WriteLine("Tasks have been seeded to the database.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}
