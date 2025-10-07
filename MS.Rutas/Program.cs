using MS.Rutas.Services;
using DotNetEnv;

namespace MS.Rutas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables del archivo .env
            DotNetEnv.Env.Load("../.env");
            
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();
            
            // Agregar controladores para endpoints HTTP
            builder.Services.AddControllers();
            
            // Registrar el servicio de base de datos
            builder.Services.AddScoped<IDatabaseService, DatabaseService>();
            
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
            app.MapGrpcService<GreeterService>();
            
            // Configurar controladores HTTP
            app.MapControllers();
            
            app.MapGet("/", () => "Microservicio de Rutas - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }
    }
}