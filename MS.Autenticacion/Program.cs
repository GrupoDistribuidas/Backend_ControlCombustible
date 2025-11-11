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

            // Configure Kestrel for HTTP/2 over HTTP (insecure) for gRPC
            // Usar 0.0.0.0 para aceptar conexiones desde otros contenedores
            builder.Configuration["Kestrel:Endpoints:gRPC:Url"] = "http://0.0.0.0:5001";
            builder.Configuration["Kestrel:Endpoints:gRPC:Protocols"] = "Http2";

            // Logging: Agrega para ver logs en consola (útil para debugging)
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddGrpc();

            // Configure AppContext for gRPC insecure connections
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // Agregar controladores para endpoints HTTP
            builder.Services.AddControllers();

            // Registrar el servicio de base de datos
            builder.Services.AddScoped<IDatabaseConnection, MS.Autenticacion.Persistence.DatabaseService>();

            // Registros para auth y repos
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRolRepository, RolRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            // ✅ Agregar Health Checks con verificación de MySQL
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddMySql(
                    connectionString: BuildConnectionString(),
                    name: "mysql-auth",
                    timeout: TimeSpan.FromSeconds(3),
                    tags: new[] { "db", "mysql" }
                );

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

            // Configurar servicios gRPC
            app.MapGrpcService<GrpcAuthService>();
            app.MapGrpcService<GrpcUserService>();

            // Configurar controladores HTTP
            app.MapControllers();

            // ✅ Configurar Health Checks
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/ready");

            app.MapGet("/", () => "Microservicio de Autenticación - gRPC y HTTP endpoints disponibles. Swagger: /swagger");

            app.Run();
        }

        // ✅ Método helper para construir connection string (usado en Health Check)
        private static string BuildConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? 
                       Environment.GetEnvironmentVariable("AUTH_DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? 
                       Environment.GetEnvironmentVariable("AUTH_DB_PORT") ?? "3306";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? 
                          Environment.GetEnvironmentVariable("AUTH_DB_NAME") ?? "AuthDB";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? 
                      Environment.GetEnvironmentVariable("AUTH_DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("DB_PASS") ?? 
                          Environment.GetEnvironmentVariable("AUTH_DB_PASS") ?? "root";

            return $"Server={host};Port={port};Database={database};Uid={user};Pwd={password};";
        }
    }
}