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
    SeedDatabase(context);
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
    // Check if the user already exists
    if (!context.Users.Any(u => u.Username == "testuser"))
    {
        context.Users.Add(new User { Username = "testuser", Password = "password" });
    }

    // List of task names
    var tasks = new List<string>
    {
        "Listing",
        "Costing",
        "New Product Work",
        "Shipment",
        "Live Stock",
        "Clearance",
        "Promotion",
        "Amazon Emails",
        "Ebay Email",
        "DP Ordering",
        "AST Ordering",
        "Supplier Invoice",
        "VAT Invoice Entry",
        "Break",
        "Other",
        "Credit Note",
        "Inventory Check",
        "Lunch",
        "Washroom",
        "Out Of Stock",
        "Meeting"
    };

    // Seed tasks if not already present
    foreach (var taskName in tasks)
    {
        if (!context.Tasks.Any(t => t.Name == taskName))
        {
            context.Tasks.Add(new TaskModel { Name = taskName });
        }
    }

    context.SaveChanges();
}
