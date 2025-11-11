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

            // Configure Kestrel for HTTP/2 over HTTP (insecure) for gRPC
            builder.Configuration["Kestrel:Endpoints:gRPC:Url"] = "http://0.0.0.0:5174";
            builder.Configuration["Kestrel:Endpoints:gRPC:Protocols"] = "Http2";



            // Add services to the container.
            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            // Configure AppContext for gRPC insecure connections
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

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

            // ✅ Agregar Health Checks con verificación de MySQL
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddMySql(
                    connectionString: BuildConnectionString(),
                    name: "mysql-routes",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] { "db", "mysql" }
                );

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
            app.MapGrpcService<MS.Rutas.Services.ReportesRutasGrpcService>();
            app.MapGrpcService<MS.Rutas.Services.ReportesAvanzadosRutasGrpcService>();
            
            // Mapear reflection solo en desarrollo
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            // Configurar controladores HTTP
            app.MapControllers();
            
            // ✅ Configurar Health Checks
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/ready");
            
            app.MapGet("/", () => "Microservicio de Rutas - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }

        // ✅ Método helper para construir connection string (usado en Health Check)
        private static string BuildConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? 
                       Environment.GetEnvironmentVariable("ROUTES_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? 
                       Environment.GetEnvironmentVariable("ROUTES_DB_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? 
                          Environment.GetEnvironmentVariable("ROUTES_DB_NAME") ?? "RoutesDB";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? 
                      Environment.GetEnvironmentVariable("ROUTES_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("DB_PASS") ?? 
                          Environment.GetEnvironmentVariable("ROUTES_DB_PASS") ?? "root";

            return $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
        }
    }
}