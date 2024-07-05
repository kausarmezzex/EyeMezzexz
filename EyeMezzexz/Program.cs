using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("E-CommDConnectionString")));

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
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        SeedDatabase(context);
    }
    catch (Exception ex)
    {
        // Log the error
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
        // Log the error
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}
