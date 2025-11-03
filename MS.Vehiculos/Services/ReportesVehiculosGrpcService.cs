using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Vehiculos.Protos;
using MS.Vehiculos.Domain.Interfaces;

namespace MS.Vehiculos.Services
{
    public class ReportesVehiculosGrpcService : ReportesVehiculosService.ReportesVehiculosServiceBase
    {
        private readonly IVehiculoRepository _vehiculoRepository;
        private readonly ILogger<ReportesVehiculosGrpcService> _logger;

        public ReportesVehiculosGrpcService(
            IVehiculoRepository vehiculoRepository,
            ILogger<ReportesVehiculosGrpcService> logger)
        {
            _vehiculoRepository = vehiculoRepository;
            _logger = logger;
        }

        public override async Task<TotalVehiculosActivosResponse> GetTotalVehiculosActivos(Empty request, ServerCallContext context)
        {
            try
            {
                var (totalActivos, totalInactivos, totalGeneral) = await _vehiculoRepository.GetTotalVehiculosActivosAsync();

                return new TotalVehiculosActivosResponse
                {
                    TotalActivos = totalActivos,
                    TotalInactivos = totalInactivos,
                    TotalGeneral = totalGeneral
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de vehículos activos");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener total de vehículos activos"));
            }
        }

        public override async Task GetVehiculosPorTipo(Empty request, IServerStreamWriter<VehiculosPorTipoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var vehiculosPorTipo = await _vehiculoRepository.GetVehiculosPorTipoAsync();

                foreach (var (tipoId, tipoNombre, cantidad) in vehiculosPorTipo)
                {
                    await responseStream.WriteAsync(new VehiculosPorTipoDto
                    {
                        TipoMaquinariaId = tipoId,
                        TipoNombre = tipoNombre,
                        Cantidad = cantidad
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos por tipo");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener vehículos por tipo"));
            }
        }

        public override async Task GetVehiculosPorEstado(Empty request, IServerStreamWriter<VehiculosPorEstadoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var vehiculosPorEstado = await _vehiculoRepository.GetVehiculosPorEstadoAsync();

                foreach (var (disponibilidad, cantidad) in vehiculosPorEstado)
                {
                    await responseStream.WriteAsync(new VehiculosPorEstadoDto
                    {
                        Disponibilidad = disponibilidad,
                        Cantidad = cantidad
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos por estado");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener vehículos por estado"));
            }
        }
    }
}
