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

            // Configure Kestrel for HTTP/2 over HTTP (insecure) for gRPC
            builder.Configuration["Kestrel:Endpoints:gRPC:Url"] = "http://0.0.0.0:5133";
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
            
            // Configurar cliente gRPC para MS.Autenticacion (validar usuarios)
            var authServiceUrl = Environment.GetEnvironmentVariable("MS_AUTENTICACION_GRPC_URL") ?? "http://localhost:5001";
            builder.Services.AddGrpcClient<MS.Autenticacion.Grpc.UserService.UserServiceClient>(options =>
            {
                options.Address = new Uri(authServiceUrl);
            });
            
            // Registrar repositorios y servicios de aplicación
            builder.Services.AddScoped<MS.Choferes.Domain.Interfaces.ITipoMaquinariaRepository, MS.Choferes.Infraestructure.Repositories.TipoMaquinariaRepository>();
            builder.Services.AddScoped<MS.Choferes.Domain.Interfaces.IChoferRepository, MS.Choferes.Infraestructure.Repositories.ChoferRepository>();
            builder.Services.AddScoped<MS.Choferes.Application.Services.TipoMaquinariaService>();
            builder.Services.AddScoped<MS.Choferes.Application.Services.ChoferService>(sp =>
            {
                var repo = sp.GetRequiredService<MS.Choferes.Domain.Interfaces.IChoferRepository>();
                var tipoRepo = sp.GetRequiredService<MS.Choferes.Domain.Interfaces.ITipoMaquinariaRepository>();
                var userClient = sp.GetRequiredService<MS.Autenticacion.Grpc.UserService.UserServiceClient>();
                return new MS.Choferes.Application.Services.ChoferService(repo, tipoRepo, userClient);
            });
            
            // Agregar Swagger para documentación de la API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ✅ Agregar Health Checks con verificación de MySQL
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddMySql(
                    connectionString: BuildConnectionString(),
                    name: "mysql-drivers",
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
            app.MapGrpcService<MS.Choferes.Services.TiposGrpcService>();
            app.MapGrpcService<MS.Choferes.Services.ChoferesGrpcService>();
            app.MapGrpcService<MS.Choferes.Services.ReportesChoferesGrpcService>();
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
            
            app.MapGet("/", () => "Microservicio de Choferes - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }

        // ✅ Método helper para construir connection string (usado en Health Check)
        private static string BuildConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? 
                       Environment.GetEnvironmentVariable("DRIVERS_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? 
                       Environment.GetEnvironmentVariable("DRIVERS_DB_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? 
                          Environment.GetEnvironmentVariable("DRIVERS_DB_NAME") ?? "DriversDB";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? 
                      Environment.GetEnvironmentVariable("DRIVERS_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("DB_PASS") ?? 
                          Environment.GetEnvironmentVariable("DRIVERS_DB_PASS") ?? "root";

            return $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
        }
    }
}