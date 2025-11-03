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

            // Configure Kestrel for HTTP/2 over HTTP (insecure) for gRPC
            builder.Configuration["Kestrel:Endpoints:gRPC:Url"] = "http://localhost:5136";
            builder.Configuration["Kestrel:Endpoints:gRPC:Protocols"] = "Http2";

            // Add services to the container.
            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            // Configure AppContext for gRPC insecure connections
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // Agregar controladores para endpoints HTTP
            builder.Services.AddControllers();
            
            // Registrar el servicio de base de datos
            builder.Services.AddScoped<IDatabaseService, DatabaseService>();
            
            // Registrar repositorios
            builder.Services.AddScoped<IAsignacionRutaRepository, AsignacionRutaRepository>();
            builder.Services.AddScoped<IEstadoAsignacionRepository, EstadoAsignacionRepository>();
            builder.Services.AddScoped<IRegistroConsumoRepository, RegistroConsumoRepository>();
            builder.Services.AddScoped<IEstadoRegistroConsumoRepository, EstadoRegistroConsumoRepository>();
            
            // Configurar clientes gRPC para otros microservicios
            var choferesUrl = Environment.GetEnvironmentVariable("MS_CHOFERES_GRPC_URL") ?? "http://localhost:5133";
            builder.Services.AddGrpcClient<MS.Choferes.Protos.ChoferesService.ChoferesServiceClient>(options =>
            {
                options.Address = new Uri(choferesUrl);
            });

            var vehiculosUrl = Environment.GetEnvironmentVariable("MS_VEHICULOS_GRPC_URL") ?? "http://localhost:5135";
            builder.Services.AddGrpcClient<MS.Vehiculos.Protos.VehiculosService.VehiculosServiceClient>(options =>
            {
                options.Address = new Uri(vehiculosUrl);
            });

            var rutasUrl = Environment.GetEnvironmentVariable("MS_RUTAS_GRPC_URL") ?? "http://localhost:5174";
            builder.Services.AddGrpcClient<MS.Rutas.Protos.RutasService.RutasServiceClient>(options =>
            {
                options.Address = new Uri(rutasUrl);
            });
            
            // Registrar servicios de aplicación
            builder.Services.AddScoped<AsignacionRutaService>();
            builder.Services.AddScoped<Application.Services.RegistroConsumoService>();
            
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
            app.MapGrpcService<RegistroConsumoGrpcService>();
            app.MapGrpcService<MS.Combustible.Services.ReportesGrpcService>();
            app.MapGrpcService<MS.Combustible.Services.ReportesAvanzadosCombustibleGrpcService>();

            // Configurar controladores HTTP
            app.MapControllers();
            
            app.MapGet("/", () => "Microservicio de Combustible - gRPC endpoint disponible en http://localhost:5136");

            app.Run();
        }
    }
}