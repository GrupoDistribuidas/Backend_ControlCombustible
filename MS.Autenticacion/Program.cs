using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MS.Autenticacion.Application.Services;
using MS.Autenticacion.Domain.Interfaces;
using MS.Autenticacion.Persistence;
using MS.Autenticacion.Infrastructure.Repositories;

namespace MS.Autenticacion
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables del archivo .env
            DotNetEnv.Env.Load("../.env");

            var builder = WebApplication.CreateBuilder(args);

            // Logging: Agrega para ver logs en consola (útil para debugging)
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddGrpc();

            // Agregar controladores para endpoints HTTP
            builder.Services.AddControllers();

            // Registrar el servicio de base de datos
            builder.Services.AddScoped<IDatabaseConnection, MS.Autenticacion.Persistence.DatabaseService>();

            // Registros para auth y repos
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRolRepository, RolRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Configuración JWT
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET no configurado en .env");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MS.Autenticacion",
                        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MS.Autenticacion",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                    };
                });

            // Agregar Swagger para documentación de la API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                // Opcional: Configura Swagger para JWT (agrega botón "Authorize" en UI)
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Por favor ingresa JWT con Bearer al inicio (e.g., Bearer tu_token)",
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Middleware para autenticación y autorización
            app.UseAuthentication();
            app.UseAuthorization();

            // Configurar gRPC - Reemplazar GreeterService con GrpcAuthService
            app.MapGrpcService<GrpcAuthService>();

            // Configurar controladores HTTP
            app.MapControllers();

            app.MapGet("/", () => "Microservicio de Autenticación - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }
    }
}