using Microsoft.EntityFrameworkCore;
using SSCV.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        int retryCount = 10;
        while (retryCount > 0)
        {
            try
            {
                logger.LogInformation($"-->Connecting to DB)");

                context.Database.Migrate();

                logger.LogInformation("--> Migration succeeded.");

                break; 
            }
            catch (Exception ex)
            {
                retryCount--;
                if (retryCount == 0) throw; 

                logger.LogWarning($"--> Lỗi kết nối: {ex.Message}");
                logger.LogWarning("--> Đợi 3 giây rồi thử lại...");
                System.Threading.Thread.Sleep(3000); 
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "--> Coudlnt ini database.");
        throw; 
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
