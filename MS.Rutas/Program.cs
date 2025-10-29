using MS.Rutas.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MS.Rutas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables del archivo .env
            DotNetEnv.Env.Load("../.env");
            
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to listen HTTPS/HTTP2 on a specific port for local gRPC testing
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5134, listenOptions =>
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
            
            // Registrar repositorios
            builder.Services.AddScoped<MS.Rutas.Domain.Interfaces.IRutaRepository, MS.Rutas.Infraestructure.Repositories.RutaRepository>();
            builder.Services.AddScoped<MS.Rutas.Domain.Interfaces.IPuntoRepository, MS.Rutas.Infraestructure.Repositories.PuntoRepository>();
            
            // Registrar servicios de aplicación
            builder.Services.AddScoped<MS.Rutas.Application.Services.RutaService>();
            builder.Services.AddScoped<MS.Rutas.Application.Services.PuntoService>();
            
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

            // Configurar gRPC Services
            app.MapGrpcService<MS.Rutas.Services.RutasGrpcService>();
            app.MapGrpcService<MS.Rutas.Services.PuntosGrpcService>();
            
            // Mapear reflection solo en desarrollo
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            // Configurar controladores HTTP
            app.MapControllers();
            
            app.MapGet("/", () => "Microservicio de Rutas - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }
    }
}