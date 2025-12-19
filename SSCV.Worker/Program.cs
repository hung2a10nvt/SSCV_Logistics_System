using Microsoft.EntityFrameworkCore;
using SSCV.Infrastructure.Data;
using SSCV.Worker;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHostedService<TelemetryWorker>();
builder.Services.AddHostedService<AlertSystemWorker>();
var host = builder.Build();
host.Run();
