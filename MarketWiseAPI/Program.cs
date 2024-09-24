using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000")  
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
