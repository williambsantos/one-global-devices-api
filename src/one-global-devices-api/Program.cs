using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Domain.Services;
using OneGlobalDevicesApi.Infra.SQLServer.Connections;
using OneGlobalDevicesApi.Infra.SQLServer.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region Service Database Registration

var databaseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(databaseConnectionString))
    throw new InvalidOperationException("Connection string not found.");

builder.Services.AddScoped<IDatabaseConnection, DatabaseConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DatabaseConnection>>();
    return new DatabaseConnection(databaseConnectionString, logger);
});

#endregion

#region Services Registration

builder.Services.AddScoped<IDevicesCrudService, DevicesCrudService>();
builder.Services.AddScoped<IDeviceRepository, DeviceSqlServerRepository>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
