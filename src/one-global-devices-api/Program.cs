using OneGlobalDevicesApi.Domain.Repositories;
using OneGlobalDevicesApi.Domain.Services;
using OneGlobalDevicesApi.Infra.SQLServer.Connections;
using OneGlobalDevicesApi.Infra.SQLServer.Repositories;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using Scalar.AspNetCore;
using OneGlobalDevicesApi.Domain.Entities;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // For Minimal APIs and controller-based APIs in .NET 7+
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // For controller-based APIs
        builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddEndpointsApiExplorer();

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
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}