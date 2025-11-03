using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Rutas.Protos;
using MS.Rutas.Domain.Interfaces;

namespace MS.Rutas.Services
{
    public class ReportesAvanzadosRutasGrpcService : ReportesAvanzadosRutasService.ReportesAvanzadosRutasServiceBase
    {
        private readonly IRutaRepository _rutaRepository;
        private readonly IPuntoRepository _puntoRepository;
        private readonly ILogger<ReportesAvanzadosRutasGrpcService> _logger;

        public ReportesAvanzadosRutasGrpcService(
            IRutaRepository rutaRepository,
            IPuntoRepository puntoRepository,
            ILogger<ReportesAvanzadosRutasGrpcService> logger)
        {
            _rutaRepository = rutaRepository;
            _puntoRepository = puntoRepository;
            _logger = logger;
        }

        public override async Task GetRutasPorProvincia(Empty request, IServerStreamWriter<RutasPorProvinciaDto> responseStream, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Obteniendo rutas por provincia");
                var rutas = await _rutaRepository.GetRutasPorProvinciaAsync();

                foreach (var (provincia, totalRutas, distanciaTotal, rutasActivas, rutasInactivas) in rutas)
                {
                    await responseStream.WriteAsync(new RutasPorProvinciaDto
                    {
                        Provincia = provincia,
                        TotalRutas = totalRutas,
                        DistanciaTotal = distanciaTotal,
                        RutasActivas = rutasActivas,
                        RutasInactivas = rutasInactivas
                    });
                }

                _logger.LogInformation("Rutas por provincia obtenidas exitosamente: {Count} provincias", rutas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rutas por provincia");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener rutas por provincia"));
            }
        }

        public override async Task GetPuntosPorTipo(Empty request, IServerStreamWriter<PuntosPorTipoDto> responseStream, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Obteniendo puntos por tipo");
                var puntos = await _puntoRepository.GetPuntosPorTipoAsync();

                foreach (var (tipoPunto, cantidad, provinciaPrincipal) in puntos)
                {
                    await responseStream.WriteAsync(new PuntosPorTipoDto
                    {
                        TipoPunto = tipoPunto,
                        Cantidad = cantidad,
                        ProvinciaPrincipal = provinciaPrincipal
                    });
                }

                _logger.LogInformation("Puntos por tipo obtenidos exitosamente: {Count} tipos", puntos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener puntos por tipo");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener puntos por tipo"));
            }
        }

        public override async Task GetPuntosMasUtilizados(Empty request, IServerStreamWriter<PuntoMasUtilizadoDto> responseStream, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Obteniendo puntos más utilizados");
                var puntos = await _puntoRepository.GetPuntosMasUtilizadosAsync();

                foreach (var (puntoId, nombrePunto, provincia, tipoPunto, vecesComoInicio, vecesComoFin, totalUsos) in puntos)
                {
                    await responseStream.WriteAsync(new PuntoMasUtilizadoDto
                    {
                        PuntoId = puntoId,
                        NombrePunto = nombrePunto,
                        Provincia = provincia,
                        TipoPunto = tipoPunto,
                        VecesComoInicio = vecesComoInicio,
                        VecesComoFin = vecesComoFin,
                        TotalUsos = totalUsos
                    });
                }

                _logger.LogInformation("Puntos más utilizados obtenidos exitosamente: {Count} puntos", puntos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener puntos más utilizados");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener puntos más utilizados"));
            }
        }
    }
}
