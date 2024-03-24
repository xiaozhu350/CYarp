using CYarp.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;

namespace CYarpServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ע��CYarp��������
            builder.Services.AddCYarp(builder.Configuration.GetSection(nameof(CYarpOptions)));
           
            // asp.net��jwt��֤����������
            builder.Services.AddControllers();
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            builder.Services.Configure<JwtTokenOptions>(builder.Configuration.GetSection(nameof(JwtTokenOptions)));
            builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<IOptions<JwtTokenOptions>>((jwt, jwtTokenOptions) =>
            {
                jwt.TokenValidationParameters = jwtTokenOptions.Value.GetParameters();
            });

            builder.Host.ConfigureHostOptions(host =>
            {
                host.ShutdownTimeout = TimeSpan.FromSeconds(1d);
            });

            // serilog
            builder.Host.UseSerilog((context, logger) =>
            {
                var template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]{NewLine}{SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";
                logger.ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: template)
                    .WriteTo.File(Path.Combine("logs", @"log.txt"), rollingInterval: RollingInterval.Day, outputTemplate: template);
            }, writeToProviders: false);

            var app = builder.Build();

            app.UseAuthentication();
            app.UseCYarp();

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}