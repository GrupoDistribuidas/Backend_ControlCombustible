using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Combustible.Protos;
using MS.Combustible.Domain.Interfaces;

namespace MS.Combustible.Services
{
    public class ReportesAvanzadosCombustibleGrpcService : ReportesAvanzadosCombustibleService.ReportesAvanzadosCombustibleServiceBase
    {
        private readonly IRegistroConsumoRepository _registroConsumoRepository;
        private readonly IAsignacionRutaRepository _asignacionRutaRepository;
        private readonly ILogger<ReportesAvanzadosCombustibleGrpcService> _logger;

        // Clientes gRPC opcionales para enriquecer datos
        private readonly MS.Rutas.Protos.RutasService.RutasServiceClient? _rutasClient;
        private readonly MS.Vehiculos.Protos.VehiculosService.VehiculosServiceClient? _vehiculosClient;

        public ReportesAvanzadosCombustibleGrpcService(
            IRegistroConsumoRepository registroConsumoRepository,
            IAsignacionRutaRepository asignacionRutaRepository,
            ILogger<ReportesAvanzadosCombustibleGrpcService> logger,
            IConfiguration configuration)
        {
            _registroConsumoRepository = registroConsumoRepository;
            _asignacionRutaRepository = asignacionRutaRepository;
            _logger = logger;

            // Configurar clientes gRPC opcionales
            try
            {
                var rutasServiceUrl = configuration.GetValue<string>("Services:RutasService:Url") 
                    ?? Environment.GetEnvironmentVariable("MS_RUTAS_GRPC_URL") 
                    ?? "https://localhost:5143";
                
                var vehiculosServiceUrl = configuration.GetValue<string>("Services:VehiculosService:Url") 
                    ?? Environment.GetEnvironmentVariable("MS_VEHICULOS_GRPC_URL") 
                    ?? "https://localhost:5135";
                
                var rutasChannel = Grpc.Net.Client.GrpcChannel.ForAddress(rutasServiceUrl);
                _rutasClient = new MS.Rutas.Protos.RutasService.RutasServiceClient(rutasChannel);

                var vehiculosChannel = Grpc.Net.Client.GrpcChannel.ForAddress(vehiculosServiceUrl);
                _vehiculosClient = new MS.Vehiculos.Protos.VehiculosService.VehiculosServiceClient(vehiculosChannel);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo conectar con servicios externos para enriquecimiento de datos");
            }
        }

        public override async Task GetConsumoPorTipoVehiculo(Empty request, IServerStreamWriter<ConsumoPorTipoVehiculoDto> responseStream, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Obteniendo consumo por tipo de vehículo");
                var consumos = await _registroConsumoRepository.GetConsumoPorTipoVehiculoAsync();

                // Optimización: Cargar todos los vehículos una sola vez
                Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>? vehiculosDict = null;
                if (_vehiculosClient != null)
                {
                    try
                    {
                        vehiculosDict = new Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>();
                        var vehiculosCall = _vehiculosClient.ListarTodosVehiculos(new Empty());
                        
                        await foreach (var vehiculo in vehiculosCall.ResponseStream.ReadAllAsync())
                        {
                            vehiculosDict[vehiculo.Id] = vehiculo;
                        }
                        
                        _logger.LogInformation("Cargados {Count} vehículos para enriquecimiento", vehiculosDict.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudieron cargar los vehículos para enriquecimiento");
                    }
                }

                foreach (var (vehiculoId, _, consumoPromedioReal, consumoPromedioEstimado, desviacionPromedio, totalViajes, combustibleTotalReal) in consumos)
                {
                    string vehiculoNombre = $"Vehículo #{vehiculoId}";
                    int tipoMaquinariaId = vehiculoId;
                    
                    // Enriquecer desde el diccionario precargado (sin llamadas gRPC adicionales)
                    if (vehiculosDict != null && vehiculosDict.TryGetValue(vehiculoId, out var vehiculo))
                    {
                        vehiculoNombre = vehiculo.Nombre;
                        tipoMaquinariaId = vehiculo.TipoMaquinariaId;
                    }

                    await responseStream.WriteAsync(new ConsumoPorTipoVehiculoDto
                    {
                        TipoVehiculoId = tipoMaquinariaId,
                        TipoVehiculoNombre = vehiculoNombre,
                        ConsumoPromedioReal = consumoPromedioReal,
                        ConsumoPromedioEstimado = consumoPromedioEstimado,
                        DesviacionPromedio = desviacionPromedio,
                        TotalViajes = totalViajes,
                        CombustibleTotalReal = combustibleTotalReal
                    });
                }

                _logger.LogInformation("Consumo por tipo de vehículo obtenido exitosamente: {Count} vehículos", consumos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener consumo por tipo de vehículo");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener consumo por tipo de vehículo"));
            }
        }

        public override async Task GetDesviacionesCombustible(Empty request, IServerStreamWriter<DesviacionCombustibleDto> responseStream, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Obteniendo desviaciones de combustible");
                var desviaciones = await _registroConsumoRepository.GetDesviacionesCombustibleAsync(50);

                // Optimización: Cargar todos los vehículos y rutas una sola vez
                Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>? vehiculosDict = null;
                Dictionary<int, MS.Rutas.Protos.RutaDto>? rutasDict = null;

                if (_vehiculosClient != null)
                {
                    try
                    {
                        vehiculosDict = new Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>();
                        var vehiculosCall = _vehiculosClient.ListarTodosVehiculos(new Empty());
                        await foreach (var vehiculo in vehiculosCall.ResponseStream.ReadAllAsync())
                        {
                            vehiculosDict[vehiculo.Id] = vehiculo;
                        }
                        _logger.LogInformation("Cargados {Count} vehículos para enriquecimiento", vehiculosDict.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudieron cargar los vehículos");
                    }
                }

                if (_rutasClient != null)
                {
                    try
                    {
                        rutasDict = new Dictionary<int, MS.Rutas.Protos.RutaDto>();
                        var rutasCall = _rutasClient.ListarTodasRutas(new Empty());
                        await foreach (var ruta in rutasCall.ResponseStream.ReadAllAsync())
                        {
                            rutasDict[ruta.Id] = ruta;
                        }
                        _logger.LogInformation("Cargadas {Count} rutas para enriquecimiento", rutasDict.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudieron cargar las rutas");
                    }
                }

                foreach (var (registroId, asignacionId, rutaId, _, vehiculoId, _, combustibleEstimado, combustibleReal, desviacion, porcentajeDesviacion, motivo) in desviaciones)
                {
                    string rutaNombre = $"Ruta #{rutaId}";
                    string vehiculoNombre = $"Vehículo #{vehiculoId}";

                    // Enriquecer desde diccionarios precargados (sin llamadas gRPC adicionales)
                    if (rutasDict != null && rutasDict.TryGetValue(rutaId, out var ruta))
                    {
                        rutaNombre = ruta.Nombre;
                    }

                    if (vehiculosDict != null && vehiculosDict.TryGetValue(vehiculoId, out var vehiculo))
                    {
                        vehiculoNombre = vehiculo.Nombre;
                    }

                    await responseStream.WriteAsync(new DesviacionCombustibleDto
                    {
                        RegistroId = registroId,
                        AsignacionId = asignacionId,
                        RutaId = rutaId,
                        RutaNombre = rutaNombre,
                        VehiculoId = vehiculoId,
                        VehiculoNombre = vehiculoNombre,
                        CombustibleEstimado = combustibleEstimado,
                        CombustibleReal = combustibleReal,
                        Desviacion = desviacion,
                        PorcentajeDesviacion = porcentajeDesviacion,
                        Motivo = motivo
                    });
                }

                _logger.LogInformation("Desviaciones de combustible obtenidas exitosamente: {Count} registros", desviaciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener desviaciones de combustible");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener desviaciones de combustible"));
            }
        }

        public override async Task GetVehiculosMasEficientes(TopVehiculosRequest request, IServerStreamWriter<VehiculoEficienciaDto> responseStream, ServerCallContext context)
        {
            try
            {
                int limite = request.Limite > 0 ? request.Limite : 10;
                _logger.LogInformation("Obteniendo top {Limite} vehículos más eficientes", limite);
                
                var vehiculos = await _registroConsumoRepository.GetVehiculosMasEficientesAsync(limite);

                // Optimización: Cargar todos los vehículos una sola vez
                Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>? vehiculosDict = null;

                if (_vehiculosClient != null)
                {
                    try
                    {
                        vehiculosDict = new Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>();
                        var vehiculosCall = _vehiculosClient.ListarTodosVehiculos(new Empty());
                        await foreach (var vehiculo in vehiculosCall.ResponseStream.ReadAllAsync())
                        {
                            vehiculosDict[vehiculo.Id] = vehiculo;
                        }
                        _logger.LogInformation("Cargados {Count} vehículos para enriquecimiento", vehiculosDict.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudieron cargar los vehículos");
                    }
                }

                foreach (var (vehiculoId, _, _, _, ratioEficiencia, totalViajes, consumoPromedioReal, ahorroCombustible) in vehiculos)
                {
                    string vehiculoNombre = $"Vehículo #{vehiculoId}";
                    string vehiculoPlaca = "SIN-PLACA";
                    string tipoVehiculo = "Desconocido";

                    // Enriquecer desde diccionario precargado (sin llamadas gRPC adicionales)
                    if (vehiculosDict != null && vehiculosDict.TryGetValue(vehiculoId, out var vehiculo))
                    {
                        vehiculoNombre = vehiculo.Nombre;
                        vehiculoPlaca = vehiculo.Placa;
                        tipoVehiculo = $"Tipo {vehiculo.TipoMaquinariaId}";
                    }

                    await responseStream.WriteAsync(new VehiculoEficienciaDto
                    {
                        VehiculoId = vehiculoId,
                        VehiculoNombre = vehiculoNombre,
                        VehiculoPlaca = vehiculoPlaca,
                        TipoVehiculo = tipoVehiculo,
                        RatioEficiencia = ratioEficiencia,
                        TotalViajes = totalViajes,
                        ConsumoPromedioReal = consumoPromedioReal,
                        AhorroCombustible = ahorroCombustible
                    });
                }

                _logger.LogInformation("Vehículos más eficientes obtenidos exitosamente: {Count} vehículos", vehiculos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos más eficientes");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener vehículos más eficientes"));
            }
        }

        public override async Task GetVehiculosMenosEficientes(TopVehiculosRequest request, IServerStreamWriter<VehiculoEficienciaDto> responseStream, ServerCallContext context)
        {
            try
            {
                int limite = request.Limite > 0 ? request.Limite : 10;
                _logger.LogInformation("Obteniendo top {Limite} vehículos menos eficientes", limite);
                
                var vehiculos = await _registroConsumoRepository.GetVehiculosMenosEficientesAsync(limite);

                // Optimización: Cargar todos los vehículos una sola vez
                Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>? vehiculosDict = null;

                if (_vehiculosClient != null)
                {
                    try
                    {
                        vehiculosDict = new Dictionary<int, MS.Vehiculos.Protos.VehiculoDto>();
                        var vehiculosCall = _vehiculosClient.ListarTodosVehiculos(new Empty());
                        await foreach (var vehiculo in vehiculosCall.ResponseStream.ReadAllAsync())
                        {
                            vehiculosDict[vehiculo.Id] = vehiculo;
                        }
                        _logger.LogInformation("Cargados {Count} vehículos para enriquecimiento", vehiculosDict.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudieron cargar los vehículos");
                    }
                }

                foreach (var (vehiculoId, _, _, _, ratioEficiencia, totalViajes, consumoPromedioReal, ahorroCombustible) in vehiculos)
                {
                    string vehiculoNombre = $"Vehículo #{vehiculoId}";
                    string vehiculoPlaca = "SIN-PLACA";
                    string tipoVehiculo = "Desconocido";

                    // Enriquecer desde diccionario precargado (sin llamadas gRPC adicionales)
                    if (vehiculosDict != null && vehiculosDict.TryGetValue(vehiculoId, out var vehiculo))
                    {
                        vehiculoNombre = vehiculo.Nombre;
                        vehiculoPlaca = vehiculo.Placa;
                        tipoVehiculo = $"Tipo {vehiculo.TipoMaquinariaId}";
                    }

                    await responseStream.WriteAsync(new VehiculoEficienciaDto
                    {
                        VehiculoId = vehiculoId,
                        VehiculoNombre = vehiculoNombre,
                        VehiculoPlaca = vehiculoPlaca,
                        TipoVehiculo = tipoVehiculo,
                        RatioEficiencia = ratioEficiencia,
                        TotalViajes = totalViajes,
                        ConsumoPromedioReal = consumoPromedioReal,
                        AhorroCombustible = ahorroCombustible
                    });
                }

                _logger.LogInformation("Vehículos menos eficientes obtenidos exitosamente: {Count} vehículos", vehiculos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos menos eficientes");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener vehículos menos eficientes"));
            }
        }

        public override async Task GetAsignacionesPorEstado(Empty request, IServerStreamWriter<AsignacionPorEstadoDto> responseStream, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Obteniendo asignaciones por estado (dinámico)");
                var asignaciones = await _asignacionRutaRepository.GetAsignacionesPorEstadoAsync();

                foreach (var (estadoNombre, totalAsignaciones, choferesAsignados, vehiculosAsignados) in asignaciones)
                {
                    await responseStream.WriteAsync(new AsignacionPorEstadoDto
                    {
                        EstadoNombre = estadoNombre,
                        TotalAsignaciones = totalAsignaciones,
                        ChoferesAsignados = choferesAsignados,
                        VehiculosAsignados = vehiculosAsignados
                    });
                }

                _logger.LogInformation("Asignaciones por estado obtenidas exitosamente: {Count} estados", asignaciones.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones por estado");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener asignaciones por estado"));
            }
        }

        [Obsolete("Este método está deprecated. Usar GetAsignacionesPorEstado en su lugar.")]
        public override async Task<AsignacionesActivasResponse> GetAsignacionesActivas(Empty request, ServerCallContext context)
        {
            try
            {
                _logger.LogWarning("GetAsignacionesActivas está deprecated. Use GetAsignacionesPorEstado para resultados dinámicos.");
                
                // Mantener compatibilidad hacia atrás usando el nuevo método dinámico
                var asignaciones = await _asignacionRutaRepository.GetAsignacionesPorEstadoAsync();
                
                var response = new AsignacionesActivasResponse();
                
                foreach (var (estadoNombre, totalAsignaciones, choferesAsignados, vehiculosAsignados) in asignaciones)
                {
                    // Mapear dinámicamente a la estructura antigua (best effort)
                    switch (estadoNombre.ToLower())
                    {
                        case "asignada":
                            response.TotalAsignacionesActivas = totalAsignaciones;
                            break;
                        case "en proceso":
                            response.TotalAsignacionesPendientes = totalAsignaciones;
                            response.ChoferesEnRuta = choferesAsignados;
                            response.VehiculosAsignados = vehiculosAsignados;
                            break;
                        case "completada":
                            response.TotalAsignacionesCompletadas = totalAsignaciones;
                            break;
                        case "cancelada":
                            response.TotalAsignacionesCanceladas = totalAsignaciones;
                            break;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones activas");
                throw new RpcException(new Status(StatusCode.Internal, "Error al obtener asignaciones activas"));
            }
        }
    }
}
