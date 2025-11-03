using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MS.Combustible.Protos;
using MS.Vehiculos.Protos;
using MS.Choferes.Protos;
using MS.Rutas.Protos;
using Google.Protobuf.WellKnownTypes;

namespace ApiGateway.Controllers
{
    /// <summary>
    /// Controlador de endpoints de reportes y dashboards
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly ReportesService.ReportesServiceClient _reportesCombustibleClient;
        private readonly ReportesVehiculosService.ReportesVehiculosServiceClient _reportesVehiculosClient;
        private readonly ReportesChoferesService.ReportesChoferesServiceClient _reportesChoferesClient;
        private readonly ReportesRutasService.ReportesRutasServiceClient _reportesRutasClient;
        private readonly ReportesAvanzadosCombustibleService.ReportesAvanzadosCombustibleServiceClient _reportesAvanzadosCombustibleClient;
        private readonly ReportesAvanzadosRutasService.ReportesAvanzadosRutasServiceClient _reportesAvanzadosRutasClient;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            ReportesService.ReportesServiceClient reportesCombustibleClient,
            ReportesVehiculosService.ReportesVehiculosServiceClient reportesVehiculosClient,
            ReportesChoferesService.ReportesChoferesServiceClient reportesChoferesClient,
            ReportesRutasService.ReportesRutasServiceClient reportesRutasClient,
            ReportesAvanzadosCombustibleService.ReportesAvanzadosCombustibleServiceClient reportesAvanzadosCombustibleClient,
            ReportesAvanzadosRutasService.ReportesAvanzadosRutasServiceClient reportesAvanzadosRutasClient,
            ILogger<ReportesController> logger)
        {
            _reportesCombustibleClient = reportesCombustibleClient;
            _reportesVehiculosClient = reportesVehiculosClient;
            _reportesChoferesClient = reportesChoferesClient;
            _reportesRutasClient = reportesRutasClient;
            _reportesAvanzadosCombustibleClient = reportesAvanzadosCombustibleClient;
            _reportesAvanzadosRutasClient = reportesAvanzadosRutasClient;
            _logger = logger;
        }

        // ========== KPIs Dashboard ==========

        /// <summary>
        /// Obtiene todos los KPIs principales del dashboard
        /// </summary>
        /// <remarks>
        /// Este endpoint agrega información de múltiples microservicios para proporcionar:
        /// - Total de vehículos activos/inactivos
        /// - Total de choferes activos/inactivos/disponibles
        /// - Total de rutas activas/inactivas y distancia total
        /// - Consumo promedio de combustible
        /// 
        /// Ideal para paneles de resumen ejecutivo.
        /// </remarks>
        /// <response code="200">KPIs obtenidos exitosamente</response>
        /// <response code="401">No autorizado - Token JWT inválido o ausente</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("kpis")]
        [ProducesResponseType(typeof(KpisResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<KpisResponse>> GetKpis()
        {
            try
            {
                _logger.LogInformation("Obteniendo KPIs del dashboard desde todos los microservicios");

                // Llamadas paralelas a todos los microservicios
                var vehiculosTask = _reportesVehiculosClient.GetTotalVehiculosActivosAsync(new Empty());
                var choferesTask = _reportesChoferesClient.GetTotalChoferesActivosAsync(new Empty());
                var rutasTask = _reportesRutasClient.GetTotalRutasActivasAsync(new Empty());
                var combustibleTask = _reportesCombustibleClient.GetConsumoPromedioCombustibleAsync(new Empty());

                await Task.WhenAll(vehiculosTask.ResponseAsync, choferesTask.ResponseAsync, 
                                   rutasTask.ResponseAsync, combustibleTask.ResponseAsync);

                var vehiculos = await vehiculosTask.ResponseAsync;
                var choferes = await choferesTask.ResponseAsync;
                var rutas = await rutasTask.ResponseAsync;
                var combustible = await combustibleTask.ResponseAsync;

                var response = new KpisResponse
                {
                    Vehiculos = new VehiculosKpi
                    {
                        TotalActivos = vehiculos.TotalActivos,
                        TotalInactivos = vehiculos.TotalInactivos,
                        TotalGeneral = vehiculos.TotalGeneral
                    },
                    Choferes = new ChoferesKpi
                    {
                        TotalActivos = choferes.TotalActivos,
                        TotalInactivos = choferes.TotalInactivos,
                        TotalDisponibles = choferes.TotalDisponibles,
                        TotalGeneral = choferes.TotalGeneral
                    },
                    Rutas = new RutasKpi
                    {
                        TotalActivas = rutas.TotalActivas,
                        TotalInactivas = rutas.TotalInactivas,
                        TotalGeneral = rutas.TotalGeneral,
                        DistanciaTotal = rutas.DistanciaTotal
                    },
                    Combustible = new CombustibleKpi
                    {
                        ConsumoPromedio = combustible.ConsumoPromedio,
                        TotalRegistros = combustible.TotalRegistros
                    }
                };

                _logger.LogInformation("KPIs obtenidos exitosamente");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener KPIs del dashboard");
                return StatusCode(500, new { message = "Error al obtener KPIs", details = ex.Message });
            }
        }

        // ========== Reportes de Combustible ==========

        /// <summary>
        /// Obtiene el consumo promedio general de combustible
        /// </summary>
        /// <remarks>
        /// Calcula el promedio de consumo real de combustible de todos los registros 
        /// en el sistema, junto con el total de registros analizados.
        /// </remarks>
        /// <response code="200">Consumo promedio obtenido exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("combustible/promedio")]
        [ProducesResponseType(typeof(ConsumoPromedioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConsumoPromedioResponse>> GetConsumoPromedio()
        {
            try
            {
                _logger.LogInformation("Obteniendo consumo promedio de combustible");
                var response = await _reportesCombustibleClient.GetConsumoPromedioCombustibleAsync(new Empty());
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consumo promedio");
                return StatusCode(500, new { message = "Error al obtener consumo promedio", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el consumo promedio de combustible por ruta
        /// </summary>
        /// <remarks>
        /// Retorna un listado detallado del consumo por cada ruta, incluyendo:
        /// - ID y nombre de la ruta
        /// - Consumo promedio por viaje
        /// - Total de viajes realizados
        /// - Combustible total estimado vs real
        /// 
        /// Útil para análisis de eficiencia por ruta.
        /// </remarks>
        /// <response code="200">Consumo por ruta obtenido exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("combustible/por-ruta")]
        [ProducesResponseType(typeof(List<ConsumoPorRutaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ConsumoPorRutaDto>>> GetConsumoPorRuta(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo consumo de combustible por ruta");
                
                var consumos = new List<ConsumoPorRutaDto>();
                var call = _reportesCombustibleClient.GetConsumoPorRuta(new Empty());
                
                // Leer el stream de respuestas
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    consumos.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} registros de consumo por ruta", consumos.Count);
                return Ok(consumos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consumo por ruta");
                return StatusCode(500, new { message = "Error al obtener consumo por ruta", details = ex.Message });
            }
        }

        // ========== Reportes de Vehículos ==========

        /// <summary>
        /// Obtiene el total de vehículos activos e inactivos
        /// </summary>
        /// <remarks>
        /// Proporciona estadísticas generales sobre el estado de los vehículos:
        /// - Total activos (en operación)
        /// - Total inactivos (fuera de servicio)
        /// - Total general
        /// </remarks>
        /// <response code="200">Totales obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("vehiculos/totales")]
        [ProducesResponseType(typeof(TotalVehiculosActivosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TotalVehiculosActivosResponse>> GetTotalVehiculos()
        {
            try
            {
                _logger.LogInformation("Obteniendo totales de vehículos");
                var response = await _reportesVehiculosClient.GetTotalVehiculosActivosAsync(new Empty());
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener totales de vehículos");
                return StatusCode(500, new { message = "Error al obtener totales de vehículos", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la distribución de vehículos por tipo de maquinaria
        /// </summary>
        /// <remarks>
        /// Retorna un desglose de cuántos vehículos existen de cada tipo 
        /// (camiones, excavadoras, grúas, etc.).
        /// 
        /// Ideal para gráficos de barras o pie charts.
        /// </remarks>
        /// <response code="200">Distribución por tipo obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("vehiculos/por-tipo")]
        [ProducesResponseType(typeof(List<VehiculosPorTipoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<VehiculosPorTipoDto>>> GetVehiculosPorTipo(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo vehículos por tipo");
                
                var vehiculosPorTipo = new List<VehiculosPorTipoDto>();
                var call = _reportesVehiculosClient.GetVehiculosPorTipo(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    vehiculosPorTipo.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} tipos de vehículos", vehiculosPorTipo.Count);
                return Ok(vehiculosPorTipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos por tipo");
                return StatusCode(500, new { message = "Error al obtener vehículos por tipo", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la distribución de vehículos por estado de disponibilidad
        /// </summary>
        /// <remarks>
        /// Retorna cuántos vehículos están en cada estado:
        /// - Disponible
        /// - En mantenimiento
        /// - No disponible
        /// 
        /// Útil para monitorear la capacidad operativa de la flota.
        /// </remarks>
        /// <response code="200">Distribución por estado obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("vehiculos/por-estado")]
        [ProducesResponseType(typeof(List<VehiculosPorEstadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<VehiculosPorEstadoDto>>> GetVehiculosPorEstado(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo vehículos por estado");
                
                var vehiculosPorEstado = new List<VehiculosPorEstadoDto>();
                var call = _reportesVehiculosClient.GetVehiculosPorEstado(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    vehiculosPorEstado.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} estados de vehículos", vehiculosPorEstado.Count);
                return Ok(vehiculosPorEstado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos por estado");
                return StatusCode(500, new { message = "Error al obtener vehículos por estado", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el total de choferes activos, inactivos y disponibles
        /// </summary>
        /// <remarks>
        /// Proporciona estadísticas sobre el personal de conducción:
        /// - Total activos (en nómina activa)
        /// - Total inactivos (suspendidos/retirados)
        /// - Total disponibles (activos y sin asignación actual)
        /// - Total general
        /// </remarks>
        /// <response code="200">Totales obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("choferes/totales")]
        [ProducesResponseType(typeof(TotalChoferesActivosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TotalChoferesActivosResponse>> GetTotalChoferes()
        {
            try
            {
                _logger.LogInformation("Obteniendo totales de choferes");
                var response = await _reportesChoferesClient.GetTotalChoferesActivosAsync(new Empty());
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener totales de choferes");
                return StatusCode(500, new { message = "Error al obtener totales de choferes", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el total de rutas activas e inactivas
        /// </summary>
        /// <remarks>
        /// Proporciona estadísticas sobre las rutas disponibles:
        /// - Total activas (en operación)
        /// - Total inactivas (fuera de servicio)
        /// - Total general
        /// - Distancia total de todas las rutas activas
        /// </remarks>
        /// <response code="200">Totales obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("rutas/totales")]
        [ProducesResponseType(typeof(TotalRutasActivasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TotalRutasActivasResponse>> GetTotalRutas()
        {
            try
            {
                _logger.LogInformation("Obteniendo totales de rutas");
                var response = await _reportesRutasClient.GetTotalRutasActivasAsync(new Empty());
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener totales de rutas");
                return StatusCode(500, new { message = "Error al obtener totales de rutas", details = ex.Message });
            }
        }

        // ========== REPORTES AVANZADOS - ANÁLISIS GEOGRÁFICO ==========

        /// <summary>
        /// Obtiene la distribución de rutas por provincia con estadísticas detalladas
        /// </summary>
        /// <remarks>
        /// Retorna análisis geográfico de rutas incluyendo:
        /// - Total de rutas por provincia
        /// - Distancia total acumulada
        /// - Rutas activas vs inactivas por región
        /// 
        /// Útil para planificación territorial y optimización de cobertura.
        /// </remarks>
        /// <response code="200">Distribución por provincia obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("rutas-por-provincia")]
        [ProducesResponseType(typeof(List<RutasPorProvinciaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RutasPorProvinciaDto>>> GetRutasPorProvincia(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo distribución de rutas por provincia");
                
                var rutas = new List<RutasPorProvinciaDto>();
                var call = _reportesAvanzadosRutasClient.GetRutasPorProvincia(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    rutas.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidas {Count} provincias con rutas", rutas.Count);
                return Ok(rutas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rutas por provincia");
                return StatusCode(500, new { message = "Error al obtener rutas por provincia", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la distribución de puntos logísticos por tipo
        /// </summary>
        /// <remarks>
        /// Analiza los puntos de ruta clasificados por tipo:
        /// - Terminales
        /// - Depósitos
        /// - Estaciones de servicio
        /// - Puertos
        /// - Almacenes
        /// - Distribuidores
        /// 
        /// Incluye la provincia principal de cada tipo de punto.
        /// </remarks>
        /// <response code="200">Distribución por tipo obtenida exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("puntos-por-tipo")]
        [ProducesResponseType(typeof(List<PuntosPorTipoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PuntosPorTipoDto>>> GetPuntosPorTipo(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo distribución de puntos por tipo");
                
                var puntos = new List<PuntosPorTipoDto>();
                var call = _reportesAvanzadosRutasClient.GetPuntosPorTipo(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    puntos.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} tipos de puntos", puntos.Count);
                return Ok(puntos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener puntos por tipo");
                return StatusCode(500, new { message = "Error al obtener puntos por tipo", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los 20 puntos logísticos más utilizados en el sistema
        /// </summary>
        /// <remarks>
        /// Ranking de puntos más frecuentes en rutas, mostrando:
        /// - Veces usado como punto de inicio
        /// - Veces usado como punto final
        /// - Total de usos acumulados
        /// - Provincia y tipo de punto
        /// 
        /// Ideal para identificar cuellos de botella y optimizar infraestructura.
        /// </remarks>
        /// <response code="200">Top 20 puntos obtenidos exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("puntos-mas-utilizados")]
        [ProducesResponseType(typeof(List<PuntoMasUtilizadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PuntoMasUtilizadoDto>>> GetPuntosMasUtilizados(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo puntos más utilizados");
                
                var puntos = new List<PuntoMasUtilizadoDto>();
                var call = _reportesAvanzadosRutasClient.GetPuntosMasUtilizados(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    puntos.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} puntos más utilizados", puntos.Count);
                return Ok(puntos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener puntos más utilizados");
                return StatusCode(500, new { message = "Error al obtener puntos más utilizados", details = ex.Message });
            }
        }

        // ========== REPORTES AVANZADOS - EFICIENCIA DE COMBUSTIBLE ==========

        /// <summary>
        /// Obtiene el análisis de consumo de combustible por tipo de vehículo
        /// </summary>
        /// <remarks>
        /// Análisis detallado por categoría de vehículo incluyendo:
        /// - Consumo promedio real vs estimado
        /// - Desviación promedio del combustible
        /// - Total de viajes realizados
        /// - Combustible total consumido
        /// 
        /// Permite identificar qué tipos de vehículos son más eficientes.
        /// </remarks>
        /// <response code="200">Consumo por tipo obtenido exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("consumo-por-tipo-vehiculo")]
        [ProducesResponseType(typeof(List<ConsumoPorTipoVehiculoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ConsumoPorTipoVehiculoDto>>> GetConsumoPorTipoVehiculo(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo consumo de combustible por tipo de vehículo");
                
                var consumos = new List<ConsumoPorTipoVehiculoDto>();
                var call = _reportesAvanzadosCombustibleClient.GetConsumoPorTipoVehiculo(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    consumos.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} tipos de vehículo con consumo", consumos.Count);
                return Ok(consumos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consumo por tipo de vehículo");
                return StatusCode(500, new { message = "Error al obtener consumo por tipo de vehículo", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las 50 mayores desviaciones de combustible (estimado vs real)
        /// </summary>
        /// <remarks>
        /// Detecta anomalías en el consumo mostrando:
        /// - Diferencia entre combustible estimado y real
        /// - Porcentaje de desviación
        /// - Detalles de ruta y vehículo involucrados
        /// - Motivo de la desviación (si está documentado)
        /// 
        /// Crítico para auditorías y detección de fraudes.
        /// </remarks>
        /// <response code="200">Desviaciones obtenidas exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("desviaciones-combustible")]
        [ProducesResponseType(typeof(List<DesviacionCombustibleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<DesviacionCombustibleDto>>> GetDesviacionesCombustible(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo desviaciones de combustible");
                
                var desviaciones = new List<DesviacionCombustibleDto>();
                var call = _reportesAvanzadosCombustibleClient.GetDesviacionesCombustible(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    desviaciones.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidas {Count} desviaciones de combustible", desviaciones.Count);
                return Ok(desviaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener desviaciones de combustible");
                return StatusCode(500, new { message = "Error al obtener desviaciones de combustible", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el ranking de vehículos más eficientes en consumo de combustible
        /// </summary>
        /// <remarks>
        /// Top N vehículos con mejor ratio de eficiencia (menor consumo real vs estimado):
        /// - Ratio de eficiencia (Real/Estimado - valores menores son mejores)
        /// - Total de viajes realizados
        /// - Consumo promedio real
        /// - Ahorro de combustible acumulado
        /// 
        /// Parámetros:
        /// - limite: Cantidad de vehículos a retornar (default: 10)
        /// </remarks>
        /// <param name="limite">Cantidad de vehículos en el top (default: 10)</param>
        /// <response code="200">Ranking obtenido exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("vehiculos-mas-eficientes")]
        [ProducesResponseType(typeof(List<VehiculoEficienciaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<VehiculoEficienciaDto>>> GetVehiculosMasEficientes(
            [FromQuery] int limite = 10, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo top {Limite} vehículos más eficientes", limite);
                
                var vehiculos = new List<VehiculoEficienciaDto>();
                var request = new TopVehiculosRequest { Limite = limite };
                var call = _reportesAvanzadosCombustibleClient.GetVehiculosMasEficientes(request);
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    vehiculos.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} vehículos más eficientes", vehiculos.Count);
                return Ok(vehiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos más eficientes");
                return StatusCode(500, new { message = "Error al obtener vehículos más eficientes", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el ranking de vehículos menos eficientes en consumo de combustible
        /// </summary>
        /// <remarks>
        /// Top N vehículos con peor ratio de eficiencia (mayor consumo real vs estimado):
        /// - Ratio de eficiencia (Real/Estimado - valores mayores indican ineficiencia)
        /// - Total de viajes realizados
        /// - Consumo promedio real
        /// - Exceso de combustible consumido
        /// 
        /// Útil para identificar vehículos que requieren mantenimiento o reemplazo.
        /// 
        /// Parámetros:
        /// - limite: Cantidad de vehículos a retornar (default: 10)
        /// </remarks>
        /// <param name="limite">Cantidad de vehículos en el top (default: 10)</param>
        /// <response code="200">Ranking obtenido exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("vehiculos-menos-eficientes")]
        [ProducesResponseType(typeof(List<VehiculoEficienciaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<VehiculoEficienciaDto>>> GetVehiculosMenosEficientes(
            [FromQuery] int limite = 10, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo top {Limite} vehículos menos eficientes", limite);
                
                var vehiculos = new List<VehiculoEficienciaDto>();
                var request = new TopVehiculosRequest { Limite = limite };
                var call = _reportesAvanzadosCombustibleClient.GetVehiculosMenosEficientes(request);
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    vehiculos.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Obtenidos {Count} vehículos menos eficientes", vehiculos.Count);
                return Ok(vehiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos menos eficientes");
                return StatusCode(500, new { message = "Error al obtener vehículos menos eficientes", details = ex.Message });
            }
        }

        // ========== REPORTES AVANZADOS - DASHBOARD OPERATIVO ==========

        /// <summary>
        /// Obtiene asignaciones agrupadas dinámicamente por estado
        /// </summary>
        /// <remarks>
        /// Proporciona una vista dinámica de asignaciones agrupadas por estado.
        /// No depende de estados hardcodeados, retorna todos los estados de la BD.
        /// 
        /// Para cada estado retorna:
        /// - Nombre del estado
        /// - Total de asignaciones en ese estado
        /// - Choferes asignados en ese estado
        /// - Vehículos asignados en ese estado
        /// 
        /// **NOTA:** Este es el nuevo endpoint dinámico. El endpoint /asignaciones-activas 
        /// está deprecated y se mantiene solo para compatibilidad hacia atrás.
        /// </remarks>
        /// <response code="200">Asignaciones por estado obtenidas exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("asignaciones-por-estado")]
        [ProducesResponseType(typeof(List<AsignacionPorEstadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AsignacionPorEstadoDto>>> GetAsignacionesPorEstado(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo asignaciones por estado (dinámico)");
                
                var asignaciones = new List<AsignacionPorEstadoDto>();
                var call = _reportesAvanzadosCombustibleClient.GetAsignacionesPorEstado(new Empty());
                
                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    asignaciones.Add(call.ResponseStream.Current);
                }

                _logger.LogInformation("Asignaciones por estado obtenidas: {Count} estados diferentes", asignaciones.Count);
                return Ok(asignaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones por estado");
                return StatusCode(500, new { message = "Error al obtener asignaciones por estado", details = ex.Message });
            }
        }

        /// <summary>
        /// [DEPRECATED] Obtiene el estado operativo actual del sistema
        /// </summary>
        /// <remarks>
        /// **ESTE ENDPOINT ESTÁ DEPRECATED.**  
        /// Use `/api/Reportes/asignaciones-por-estado` para obtener datos dinámicos.
        /// 
        /// Proporciona una vista consolidada del estado operativo:
        /// - Total de asignaciones activas (en curso)
        /// - Asignaciones pendientes de iniciar
        /// - Asignaciones completadas
        /// - Asignaciones canceladas
        /// - Choferes actualmente en ruta
        /// - Vehículos actualmente asignados
        /// 
        /// Ideal para dashboard de control operativo en tiempo real.
        /// </remarks>
        /// <response code="200">Estado operativo obtenido exitosamente</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno</response>
        [HttpGet("asignaciones-activas")]
        [Obsolete("Este endpoint está deprecated. Use /api/Reportes/asignaciones-por-estado para resultados dinámicos.")]
        [ProducesResponseType(typeof(AsignacionesActivasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AsignacionesActivasResponse>> GetAsignacionesActivas()
        {
            try
            {
                _logger.LogWarning("Endpoint asignaciones-activas está deprecated. Use asignaciones-por-estado.");
                var response = await _reportesAvanzadosCombustibleClient.GetAsignacionesActivasAsync(new Empty());
                _logger.LogInformation("Estado operativo obtenido: {Activas} asignaciones activas, {Choferes} choferes en ruta", 
                    response.TotalAsignacionesActivas, response.ChoferesEnRuta);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones activas");
                return StatusCode(500, new { message = "Error al obtener asignaciones activas", details = ex.Message });
            }
        }
    }

    // ========== DTOs para respuesta agregada de KPIs ==========

    public class KpisResponse
    {
        public VehiculosKpi Vehiculos { get; set; } = null!;
        public ChoferesKpi Choferes { get; set; } = null!;
        public RutasKpi Rutas { get; set; } = null!;
        public CombustibleKpi Combustible { get; set; } = null!;
    }

    public class VehiculosKpi
    {
        public int TotalActivos { get; set; }
        public int TotalInactivos { get; set; }
        public int TotalGeneral { get; set; }
    }

    public class ChoferesKpi
    {
        public int TotalActivos { get; set; }
        public int TotalInactivos { get; set; }
        public int TotalDisponibles { get; set; }
        public int TotalGeneral { get; set; }
    }

    public class RutasKpi
    {
        public int TotalActivas { get; set; }
        public int TotalInactivas { get; set; }
        public int TotalGeneral { get; set; }
        public double DistanciaTotal { get; set; }
    }

    public class CombustibleKpi
    {
        public double ConsumoPromedio { get; set; }
        public int TotalRegistros { get; set; }
    }
}
