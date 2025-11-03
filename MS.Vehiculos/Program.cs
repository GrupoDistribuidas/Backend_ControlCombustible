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
            builder.Configuration["Kestrel:Endpoints:gRPC:Url"] = "http://localhost:5135";
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
            
            app.MapGet("/", () => "Microservicio de Vehículos - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }
    }
}