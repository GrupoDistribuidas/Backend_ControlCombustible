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

            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Services:AuthService:Url"] = Environment.GetEnvironmentVariable("MS_AUTENTICACION_GRPC_URL"),
                ["Services:VehiculosService:Url"] = Environment.GetEnvironmentVariable("MS_VEHICULOS_GRPC_URL"),
                ["Services:ChoferesService:Url"] = Environment.GetEnvironmentVariable("MS_CHOFERES_GRPC_URL"),
                ["Services:CombustibleService:Url"] = Environment.GetEnvironmentVariable("MS_COMBUSTIBLE_GRPC_URL"),
                ["Services:RutasService:Url"] = Environment.GetEnvironmentVariable("MS_RUTAS_GRPC_URL")
            }.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
             .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            // Agregar controladores
            builder.Services.AddControllers();

            // ✅ Configurar Swagger/OpenAPI con autenticación JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Gateway - Control de Combustible",
                    Version = "v1.0",
                    Description = @"
## API Gateway para Sistema de Control de Combustible

Este API Gateway centraliza el acceso a todos los microservicios del sistema de control de combustible.

### Autenticación:
1. Usar endpoint `/auth/login` para obtener token JWT
2. Incluir token en header: `Authorization: Bearer {token}`
3. El token expira según configuración del sistema",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipo de Desarrollo",
                        Email = "dev@controlcombustible.com"
                    }
                });

                // Incluir comentarios XML para documentación detallada
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }

                // 🔒 Configurar el esquema de seguridad Bearer
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = @"
Introduce tu token JWT en el formato: **Bearer {token}**

Ejemplo: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

Para obtener un token:
1. Usar el endpoint `/auth/login` 
2. Copiar el valor del campo `token` de la respuesta
3. Agregarlo aquí con el prefijo 'Bearer '"
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

                // Configurar generación de documentación adicional
                options.DescribeAllParametersInCamelCase();
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
