using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Combustible.Protos;
using MS.Combustible.Domain.Interfaces;

namespace MS.Combustible.Services
{
    public class ReportesGrpcService : ReportesService.ReportesServiceBase
    {
        private readonly IRegistroConsumoRepository _registroConsumoRepository;
        private readonly IAsignacionRutaRepository _asignacionRutaRepository;
        private readonly ILogger<ReportesGrpcService> _logger;

        // Cliente gRPC para obtener información de rutas
        private readonly MS.Rutas.Protos.RutasService.RutasServiceClient? _rutasClient;

        public ReportesGrpcService(
            IRegistroConsumoRepository registroConsumoRepository,
            IAsignacionRutaRepository asignacionRutaRepository,
            ILogger<ReportesGrpcService> logger,
            IConfiguration configuration)
        {
            _registroConsumoRepository = registroConsumoRepository;
            _asignacionRutaRepository = asignacionRutaRepository;
            _logger = logger;

            // Configurar cliente gRPC de rutas
            try
            {
                var rutasServiceUrl = configuration.GetValue<string>("Services:RutasService:Url") 
                    ?? Environment.GetEnvironmentVariable("MS_RUTAS_GRPC_URL") 
                    ?? "https://localhost:5143";
                
                var channel = Grpc.Net.Client.GrpcChannel.ForAddress(rutasServiceUrl);
                _rutasClient = new MS.Rutas.Protos.RutasService.RutasServiceClient(channel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo conectar con el servicio de rutas");
            }
        }

        public override async Task<ConsumoPromedioResponse> GetConsumoPromedioCombustible(Empty request, ServerCallContext context)
        {
            try
            {
                var consumoPromedio = await _registroConsumoRepository.GetConsumoPromedioAsync();
                var totalRegistros = await _registroConsumoRepository.GetTotalRegistrosAsync();

                return new ConsumoPromedioResponse
                {
                    ConsumoPromedio = consumoPromedio,
                    TotalRegistros = totalRegistros
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consumo promedio de combustible");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener consumo promedio"));
            }
        }

        public override async Task GetConsumoPorRuta(Empty request, IServerStreamWriter<ConsumoPorRutaDto> responseStream, ServerCallContext context)
        {
            try
            {
                var consumoPorRuta = await _registroConsumoRepository.GetConsumoPorRutaAsync();

                foreach (var (rutaId, consumoPromedio, totalViajes, totalEstimado, totalReal) in consumoPorRuta)
                {
                    // Intentar obtener el nombre de la ruta del servicio de rutas
                    string rutaNombre = $"Ruta {rutaId}";
                    
                    if (_rutasClient != null)
                    {
                        try
                        {
                            var rutaInfo = await _rutasClient.GetByIdAsync(new MS.Rutas.Protos.GetByIdRequest { Id = rutaId });
                            if (rutaInfo != null && !string.IsNullOrEmpty(rutaInfo.Nombre))
                            {
                                rutaNombre = rutaInfo.Nombre;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "No se pudo obtener información de la ruta {RutaId}", rutaId);
                        }
                    }

                    await responseStream.WriteAsync(new ConsumoPorRutaDto
                    {
                        RutaId = rutaId,
                        RutaNombre = rutaNombre,
                        ConsumoPromedio = consumoPromedio,
                        TotalViajes = totalViajes,
                        CombustibleTotalEstimado = totalEstimado,
                        CombustibleTotalReal = totalReal
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consumo por ruta");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener consumo por ruta"));
            }
        }
    }
}
