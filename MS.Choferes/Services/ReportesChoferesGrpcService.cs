using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Choferes.Protos;
using MS.Choferes.Domain.Interfaces;

namespace MS.Choferes.Services
{
    public class ReportesChoferesGrpcService : ReportesChoferesService.ReportesChoferesServiceBase
    {
        private readonly IChoferRepository _choferRepository;
        private readonly ILogger<ReportesChoferesGrpcService> _logger;

        public ReportesChoferesGrpcService(
            IChoferRepository choferRepository,
            ILogger<ReportesChoferesGrpcService> logger)
        {
            _choferRepository = choferRepository;
            _logger = logger;
        }

        public override async Task<TotalChoferesActivosResponse> GetTotalChoferesActivos(Empty request, ServerCallContext context)
        {
            try
            {
                var (totalActivos, totalInactivos, totalDisponibles, totalGeneral) = await _choferRepository.GetTotalChoferesActivosAsync();

                return new TotalChoferesActivosResponse
                {
                    TotalActivos = totalActivos,
                    TotalInactivos = totalInactivos,
                    TotalDisponibles = totalDisponibles,
                    TotalGeneral = totalGeneral
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de choferes activos");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener total de choferes activos"));
            }
        }
    }
}
