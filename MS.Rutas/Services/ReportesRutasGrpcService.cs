using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Rutas.Protos;
using MS.Rutas.Domain.Interfaces;

namespace MS.Rutas.Services
{
    public class ReportesRutasGrpcService : ReportesRutasService.ReportesRutasServiceBase
    {
        private readonly IRutaRepository _rutaRepository;
        private readonly ILogger<ReportesRutasGrpcService> _logger;

        public ReportesRutasGrpcService(
            IRutaRepository rutaRepository,
            ILogger<ReportesRutasGrpcService> logger)
        {
            _rutaRepository = rutaRepository;
            _logger = logger;
        }

        public override async Task<TotalRutasActivasResponse> GetTotalRutasActivas(Empty request, ServerCallContext context)
        {
            try
            {
                var (totalActivas, totalInactivas, totalGeneral, distanciaTotal) = await _rutaRepository.GetTotalRutasActivasAsync();

                return new TotalRutasActivasResponse
                {
                    TotalActivas = totalActivas,
                    TotalInactivas = totalInactivas,
                    TotalGeneral = totalGeneral,
                    DistanciaTotal = distanciaTotal
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de rutas activas");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener total de rutas activas"));
            }
        }
    }
}
