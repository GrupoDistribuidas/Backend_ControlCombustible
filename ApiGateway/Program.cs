
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

            // Add services to the container.

            builder.Services.AddControllers();
            // Configurar Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configurar gRPC client factory (se usará para llamar a MS.Autenticacion)
            var msAuthGrpcUrl = Environment.GetEnvironmentVariable("MS_AUTENTICACION_GRPC_URL") ?? "http://localhost:5001";
            builder.Services.AddGrpcClient<MS.Autenticacion.Grpc.AuthService.AuthServiceClient>((provider, options) =>
            {
                options.Address = new Uri(msAuthGrpcUrl);
            });

            // Configuración JWT
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
                        ClockSkew = TimeSpan.Zero
                    };
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
