using MS.Vehiculos.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MS.Vehiculos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables del archivo .env
            DotNetEnv.Env.Load("../.env");
            
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel for HTTP/2 over HTTP (insecure) for gRPC
            builder.Configuration["Kestrel:Endpoints:gRPC:Url"] = "http://0.0.0.0:5135";
            builder.Configuration["Kestrel:Endpoints:gRPC:Protocols"] = "Http2";

            // Add services to the container.
            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            // Configure AppContext for gRPC insecure connections
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // gRPC reflection (helps Postman / tooling discover services)
            builder.Services.AddGrpcReflection();

            // Agregar controladores para endpoints HTTP
            builder.Services.AddControllers();

            // Registrar el servicio de base de datos
            builder.Services.AddScoped<IDatabaseService, DatabaseService>();

            // Registrar repositorios
            builder.Services.AddScoped<MS.Vehiculos.Domain.Interfaces.ITipoMaquinariaRepository, MS.Vehiculos.Infraestructure.Repositories.TipoMaquinariaRepository>();
            builder.Services.AddScoped<MS.Vehiculos.Domain.Interfaces.IVehiculoRepository, MS.Vehiculos.Infraestructure.Repositories.VehiculoRepository>();

            // Registrar servicios de aplicación
            builder.Services.AddScoped<MS.Vehiculos.Application.Services.TipoMaquinariaService>();
            builder.Services.AddScoped<MS.Vehiculos.Application.Services.VehiculoService>(sp =>
            {
                var repo = sp.GetRequiredService<MS.Vehiculos.Domain.Interfaces.IVehiculoRepository>();
                var tipoRepo = sp.GetRequiredService<MS.Vehiculos.Domain.Interfaces.ITipoMaquinariaRepository>();
                return new MS.Vehiculos.Application.Services.VehiculoService(repo, tipoRepo);
            });

            // Agregar Swagger para documentación de la API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ✅ Agregar Health Checks con verificación de MySQL
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddMySql(
                    connectionString: BuildConnectionString(),
                    name: "mysql-vehicles",
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

            // Configurar gRPC
            app.MapGrpcService<MS.Vehiculos.Services.VehiculosGrpcService>();
            app.MapGrpcService<MS.Vehiculos.Services.TiposGrpcService>();
            app.MapGrpcService<MS.Vehiculos.Services.ReportesVehiculosGrpcService>();
            // Mapear reflection solo en entornos de desarrollo
            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }
            
            // Configurar controladores HTTP
            app.MapControllers();
            
            // ✅ Configurar Health Checks
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/ready");
            
            app.MapGet("/", () => "Microservicio de Vehículos - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }

        // ✅ Método helper para construir connection string (usado en Health Check)
        private static string BuildConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? 
                       Environment.GetEnvironmentVariable("VEHICLES_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? 
                       Environment.GetEnvironmentVariable("VEHICLES_DB_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? 
                          Environment.GetEnvironmentVariable("VEHICLES_DB_NAME") ?? "VehiclesDB";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? 
                      Environment.GetEnvironmentVariable("VEHICLES_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("DB_PASS") ?? 
                          Environment.GetEnvironmentVariable("VEHICLES_DB_PASS") ?? "root";

            return $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
        }
    }
}