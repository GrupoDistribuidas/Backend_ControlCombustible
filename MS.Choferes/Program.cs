using MS.Choferes.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MS.Choferes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables del archivo .env
            DotNetEnv.Env.Load("../.env");
            
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to listen HTTPS/HTTP2 on localhost:5133 for local gRPC testing
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5133, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                    listenOptions.UseHttps();
                });
            });

            // Add services to the container.
            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            // gRPC reflection
            builder.Services.AddGrpcReflection();

            // Agregar controladores para endpoints HTTP
            builder.Services.AddControllers();
            
            // Registrar el servicio de base de datos
            builder.Services.AddScoped<IDatabaseService, DatabaseService>();
            // Registrar repositorios y servicios de aplicación
            builder.Services.AddScoped<MS.Choferes.Domain.Interfaces.ITipoMaquinariaRepository, MS.Choferes.Infraestructure.Repositories.TipoMaquinariaRepository>();
            builder.Services.AddScoped<MS.Choferes.Domain.Interfaces.IChoferRepository, MS.Choferes.Infraestructure.Repositories.ChoferRepository>();
            builder.Services.AddScoped<MS.Choferes.Application.Services.TipoMaquinariaService>();
            builder.Services.AddScoped<MS.Choferes.Application.Services.ChoferService>(sp =>
            {
                var repo = sp.GetRequiredService<MS.Choferes.Domain.Interfaces.IChoferRepository>();
                var tipoRepo = sp.GetRequiredService<MS.Choferes.Domain.Interfaces.ITipoMaquinariaRepository>();
                return new MS.Choferes.Application.Services.ChoferService(repo, tipoRepo);
            });
            
            // Agregar Swagger para documentación de la API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configurar gRPC
            app.MapGrpcService<MS.Choferes.Services.TiposGrpcService>();
            app.MapGrpcService<MS.Choferes.Services.ChoferesGrpcService>();
            // Mapear reflection solo en desarrollo
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            // Configurar controladores HTTP
            app.MapControllers();
            
            app.MapGet("/", () => "Microservicio de Choferes - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }
    }
}