using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFarmManager.API.Extensions;
using SmartFarmManager.DataAccessObject.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

DotNetEnv.Env.Load();
builder.Host.AddAppConfigurations();
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
var timeZoneId = builder.Configuration["TimeZone"] ?? "UTC";
TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
TimeZoneInfo.ClearCachedData();
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseInfrastructure();
if (environment != null && !environment.Equals("Development"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartFarmContext>();

        try
        {
            
            if (dbContext.Database.EnsureCreated())
            {
                dbContext.Database.Migrate();
                logger.LogInformation("Database created successfully.");
            }
            else
            {
                logger.LogInformation("Database already exists.");
            }



        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring database creation or applying migrations.");
        }
    }
}

app.Run();
