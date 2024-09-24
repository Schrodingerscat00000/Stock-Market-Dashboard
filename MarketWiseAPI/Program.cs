using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure database connection (e.g., SQL Server)
builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000")  // Allow requests from React app
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");  // Apply the CORS policy globally
app.UseAuthorization();

app.MapControllers();

app.Run();
