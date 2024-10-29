using SmartFarmManager.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

DotNetEnv.Env.Load();
builder.Host.AddAppConfigurations();
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseInfrastructure();

app.Run();
