using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Vehiculos.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.ComponentModel.DataAnnotations;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador para la gestión de tipos de maquinaria
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere JWT para todos los endpoints
    [Produces("application/json")]
    public class TiposController : ControllerBase
    {
        private readonly ILogger<TiposController> _logger;
        private readonly TiposService.TiposServiceClient _tiposClient;

        public TiposController(ILogger<TiposController> logger, TiposService.TiposServiceClient tiposClient)
        {
            _logger = logger;
            _tiposClient = tiposClient;
        }

        /// <summary>
        /// Obtiene todos los tipos de maquinaria disponibles en el sistema
        /// </summary>
        /// <returns>Lista completa de tipos de maquinaria con sus detalles</returns>
        /// <response code="200">Lista de tipos obtenida exitosamente</response>
        /// <response code="401">Token JWT no válido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListarTipos()
        {
            try
            {
                var tipos = new List<object>();
                
                using var call = _tiposClient.ListarTipos(new Empty());
                
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
                return StatusCode(500, new { Success = false, Message = "Error interno del servidor" });
            }
        }
    }
}