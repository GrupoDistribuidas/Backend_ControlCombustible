using MS.Combustible.Services;
using MS.Combustible.Domain.Interfaces;
using MS.Combustible.Infrastructure.Repositories;
using MS.Combustible.Application.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace MS.Combustible
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables del archivo .env
            DotNetEnv.Env.Load("../.env");
            
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to listen HTTPS/HTTP2 on localhost:5136 for local gRPC testing
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5136, listenOptions =>
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

            // Agregar controladores para endpoints HTTP
            builder.Services.AddControllers();
            
            // Registrar el servicio de base de datos
            builder.Services.AddScoped<IDatabaseService, DatabaseService>();
            
            // Registrar repositorios
            builder.Services.AddScoped<IAsignacionRutaRepository, AsignacionRutaRepository>();
            builder.Services.AddScoped<IEstadoAsignacionRepository, EstadoAsignacionRepository>();
            
            // Configurar clientes gRPC para otros microservicios
            var choferesUrl = Environment.GetEnvironmentVariable("MS_CHOFERES_GRPC_URL") ?? "https://localhost:5133";
            builder.Services.AddGrpcClient<MS.Choferes.Protos.ChoferesService.ChoferesServiceClient>(options =>
            {
                options.Address = new Uri(choferesUrl);
            });

            var vehiculosUrl = Environment.GetEnvironmentVariable("MS_VEHICULOS_GRPC_URL") ?? "https://localhost:5135";
            builder.Services.AddGrpcClient<MS.Vehiculos.Protos.VehiculosService.VehiculosServiceClient>(options =>
            {
                options.Address = new Uri(vehiculosUrl);
            });

            var rutasUrl = Environment.GetEnvironmentVariable("MS_RUTAS_GRPC_URL") ?? "https://localhost:5134";
            builder.Services.AddGrpcClient<MS.Rutas.Protos.RutasService.RutasServiceClient>(options =>
            {
                options.Address = new Uri(rutasUrl);
            });
            
            // Registrar servicio de aplicación
            builder.Services.AddScoped<AsignacionRutaService>();
            
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
            app.MapGrpcService<AsignacionesGrpcService>();

            // Configurar controladores HTTP
            app.MapControllers();
            
            app.MapGet("/", () => "Microservicio de Combustible - gRPC endpoint disponible en https://localhost:5136");

            app.Run();
        }
    }
}