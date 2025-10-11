using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Vehiculos.Protos;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere JWT para todos los endpoints
    public class TiposController : ControllerBase
    {
        private readonly ILogger<TiposController> _logger;
        private readonly string _vehiculosServiceUrl;

        public TiposController(ILogger<TiposController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _vehiculosServiceUrl = configuration.GetValue<string>("Services:VehiculosService:Url") ?? "https://localhost:7056";
        }

        /// <summary>
        /// Lista todos los tipos de maquinaria
        /// </summary>
        /// <returns>Lista de tipos de maquinaria</returns>
        [HttpGet]
        public async Task<IActionResult> ListarTipos()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(_vehiculosServiceUrl);
                var client = new TiposService.TiposServiceClient(channel);

                var tipos = new List<object>();
                
                using var call = client.ListarTipos(new Empty());
                
                await foreach (var tipo in call.ResponseStream.ReadAllAsync())
                {
                    tipos.Add(new
                    {
                        Id = tipo.Id,
                        Nombre = tipo.Nombre,
                        Descripcion = tipo.Descripcion
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Tipos de maquinaria obtenidos exitosamente",
                    Data = tipos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tipos de maquinaria");
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }
}