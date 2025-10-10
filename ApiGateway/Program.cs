using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Cargar variables de .env
            DotNetEnv.Env.Load("../.env");

            var builder = WebApplication.CreateBuilder(args);

            // Agregar controladores
            builder.Services.AddControllers();

            // ✅ Configurar Swagger/OpenAPI con autenticación JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Gateway",
                    Version = "v1",
                    Description = "Gateway de microservicios con autenticación JWT y control de roles"
                });

                // 🔒 Configurar el esquema de seguridad Bearer
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Introduce tu token JWT en el formato: Bearer {token}"
                });

                // 🔐 Requerir el token para endpoints protegidos
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Configurar gRPC client factory (para llamar a MS.Autenticacion)
            var msAuthGrpcUrl = Environment.GetEnvironmentVariable("MS_AUTENTICACION_GRPC_URL") ?? "http://localhost:5001";
            builder.Services.AddGrpcClient<MS.Autenticacion.Grpc.AuthService.AuthServiceClient>((provider, options) =>
            {
                options.Address = new Uri(msAuthGrpcUrl);
            });

            // 🔑 Configuración JWT
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET no configurado en .env");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MS.Autenticacion";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MS.Autenticacion";

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ClockSkew = TimeSpan.Zero,
                    };
                });

            var app = builder.Build();

            // Pipeline de la app
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
                    options.DocumentTitle = "ChallengeHub Gateway";
                });
            }

            app.UseHttpsRedirection();

            // 🔐 Middleware de autenticación y autorización
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
