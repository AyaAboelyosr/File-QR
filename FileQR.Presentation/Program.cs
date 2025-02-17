using FileQR.Domain.Entities;
using FileQR.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using FileQR.Infrastructure.Data;
using FileQR.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Register FileManager as a singleton
        builder.Services.AddSingleton<IFileManager, FileManager>(provider =>
            new FileManager(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")));

        // Register DbContext
        builder.Services.AddDbContext<ApplicationDBContext>(options =>
         options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


        // Register QRSettingsService
        builder.Services.AddScoped<IQRSettingsService, QRSettingsService>();

      builder.Services.AddScoped<IQRCodeGeneration, QRCodeGenerationService>();
       builder.Services.AddScoped<IImageConversion, ImageConversionService>();
        builder.Services.AddScoped<IMeasurementConverter, MeasurementConverterService>();

        // Add Swagger for API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            var url = "https://localhost:56603/swagger/index.html"; 
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open browser: {ex.Message}");
            }
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
