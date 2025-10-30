using MS.Combustible.Application.DTOs;
using MS.Combustible.Domain.Entities;
using MS.Combustible.Domain.Interfaces;

namespace MS.Combustible.Application.Services
{
    public class AsignacionRutaService
    {
        private readonly IAsignacionRutaRepository _asignacionRepo;
        private readonly IEstadoAsignacionRepository _estadoRepo;
        private readonly ILogger<AsignacionRutaService> _logger;
        private readonly MS.Choferes.Protos.ChoferesService.ChoferesServiceClient _choferesClient;
        private readonly MS.Vehiculos.Protos.VehiculosService.VehiculosServiceClient _vehiculosClient;
        private readonly MS.Rutas.Protos.RutasService.RutasServiceClient _rutasClient;

        public AsignacionRutaService(
            IAsignacionRutaRepository asignacionRepo,
            IEstadoAsignacionRepository estadoRepo,
            ILogger<AsignacionRutaService> logger,
            MS.Choferes.Protos.ChoferesService.ChoferesServiceClient choferesClient,
            MS.Vehiculos.Protos.VehiculosService.VehiculosServiceClient vehiculosClient,
            MS.Rutas.Protos.RutasService.RutasServiceClient rutasClient)
        {
            _asignacionRepo = asignacionRepo;
            _estadoRepo = estadoRepo;
            _logger = logger;
            _choferesClient = choferesClient;
            _vehiculosClient = vehiculosClient;
            _rutasClient = rutasClient;
        }

        public async Task<int> CrearAsignacionAsync(CrearAsignacionRutaDto dto)
        {
            var errors = new List<string>();
            if (dto.ChoferId <= 0) errors.Add("ChoferId inválido");
            if (dto.VehiculoId <= 0) errors.Add("VehiculoId inválido");
            if (dto.RutaId <= 0) errors.Add("RutaId inválido");
            if (dto.FechaAsignacion == default) errors.Add("FechaAsignacion es obligatoria");
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            if (dto.FechaAsignacion.Date < DateTime.Now.Date)
                throw new ArgumentException("No se pueden crear asignaciones con fechas pasadas");

            try
            {
                var choferRequest = new MS.Choferes.Protos.GetByIdRequest { Id = dto.ChoferId };
                var choferResponse = await _choferesClient.GetByIdAsync(choferRequest);
                
                _logger.LogInformation($"Chofer Response - Id: {choferResponse?.Id}, NombreCompleto: {choferResponse?.NombreCompleto}, Estado: {choferResponse?.Estado}, Disponible: {choferResponse?.Disponible}");
                
                if (choferResponse == null) throw new KeyNotFoundException($"Chofer {dto.ChoferId} no existe");
                if (!choferResponse.Estado) throw new InvalidOperationException("Chofer inactivo");
                if (!choferResponse.Disponible) throw new InvalidOperationException("Chofer no disponible");

                var vehiculoRequest = new MS.Vehiculos.Protos.GetByIdRequest { Id = dto.VehiculoId };
                var vehiculoResponse = await _vehiculosClient.GetByIdAsync(vehiculoRequest);
                if (vehiculoResponse == null) throw new KeyNotFoundException($"Vehículo {dto.VehiculoId} no existe");
                if (!vehiculoResponse.Disponible.Equals("Disponible", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Vehículo no disponible");

                var rutaRequest = new MS.Rutas.Protos.GetByIdRequest { Id = dto.RutaId };
                var rutaResponse = await _rutasClient.GetByIdAsync(rutaRequest);
                if (rutaResponse == null) throw new KeyNotFoundException($"Ruta {dto.RutaId} no existe");
                if (!rutaResponse.Estado) throw new InvalidOperationException("Ruta no habilitada");

                var asignaciones = await _asignacionRepo.GetByVehiculoIdAsync(dto.VehiculoId);
                var conflicto = asignaciones.FirstOrDefault(a => 
                    a.FechaAsignacion.Date == dto.FechaAsignacion.Date && (a.EstadoId == 1 || a.EstadoId == 2));
                if (conflicto != null) throw new InvalidOperationException("Vehículo ya asignado en esa fecha");

                double combustibleEstimado = (rutaResponse.Distancia * vehiculoResponse.ConsumoCombustibleKm) / 100;

                var asignacion = new AsignacionRuta
                {
                    ChoferId = dto.ChoferId,
                    VehiculoId = dto.VehiculoId,
                    RutaId = dto.RutaId,
                    FechaAsignacion = dto.FechaAsignacion,
                    CombustibleEstimado = combustibleEstimado,
                    EstadoId = 1
                };

                var asignacionId = await _asignacionRepo.CreateAsync(asignacion);

                // Actualizar disponibilidad del chofer y vehículo
                try
                {
                    var updateChoferRequest = new MS.Choferes.Protos.ActualizarDisponibilidadRequest
                    {
                        Id = dto.ChoferId,
                        Disponible = false
                    };
                    await _choferesClient.ActualizarDisponibilidadChoferAsync(updateChoferRequest);
                    _logger.LogInformation($"Chofer ID {dto.ChoferId} marcado como NO disponible");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"No se pudo actualizar disponibilidad del chofer: {ex.Message}");
                }

                try
                {
                    var updateVehiculoRequest = new MS.Vehiculos.Protos.ActualizarDisponibilidadRequest
                    {
                        Id = dto.VehiculoId,
                        Disponible = "No Disponible"
                    };
                    await _vehiculosClient.ActualizarDisponibilidadAsync(updateVehiculoRequest);
                    _logger.LogInformation($"Vehículo ID {dto.VehiculoId} marcado como 'No Disponible'");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"No se pudo actualizar disponibilidad del vehículo: {ex.Message}");
                }

                return asignacionId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<int> ActualizarAsignacionAsync(ActualizarAsignacionRutaDto dto)
        {
            var errors = new List<string>();
            if (dto.Id <= 0) errors.Add("Id inválido");
            if (dto.ChoferId <= 0) errors.Add("ChoferId inválido");
            if (dto.VehiculoId <= 0) errors.Add("VehiculoId inválido");
            if (dto.RutaId <= 0) errors.Add("RutaId inválido");
            if (dto.FechaAsignacion == default) errors.Add("FechaAsignacion es obligatoria");
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            if (dto.FechaAsignacion.Date < DateTime.Now.Date)
                throw new ArgumentException("No se pueden actualizar asignaciones con fechas pasadas");

            try
            {
                var existente = await _asignacionRepo.GetByIdAsync(dto.Id);
                if (existente == null)
                    throw new KeyNotFoundException($"Asignación con ID {dto.Id} no existe");

                var choferRequest = new MS.Choferes.Protos.GetByIdRequest { Id = dto.ChoferId };
                var choferResponse = await _choferesClient.GetByIdAsync(choferRequest);
                if (choferResponse == null) throw new KeyNotFoundException($"Chofer {dto.ChoferId} no existe");
                if (!choferResponse.Estado) throw new InvalidOperationException("Chofer inactivo");
                if (!choferResponse.Disponible && dto.ChoferId != existente.ChoferId)
                    throw new InvalidOperationException("Chofer no disponible");

                var vehiculoRequest = new MS.Vehiculos.Protos.GetByIdRequest { Id = dto.VehiculoId };
                var vehiculoResponse = await _vehiculosClient.GetByIdAsync(vehiculoRequest);
                if (vehiculoResponse == null) throw new KeyNotFoundException($"Vehículo {dto.VehiculoId} no existe");
                if (!vehiculoResponse.Disponible.Equals("Disponible", StringComparison.OrdinalIgnoreCase) && 
                    dto.VehiculoId != existente.VehiculoId)
                    throw new InvalidOperationException("Vehículo no disponible");

                var rutaRequest = new MS.Rutas.Protos.GetByIdRequest { Id = dto.RutaId };
                var rutaResponse = await _rutasClient.GetByIdAsync(rutaRequest);
                if (rutaResponse == null) throw new KeyNotFoundException($"Ruta {dto.RutaId} no existe");
                if (!rutaResponse.Estado) throw new InvalidOperationException("Ruta no habilitada");

                if (dto.VehiculoId != existente.VehiculoId)
                {
                    var asignaciones = await _asignacionRepo.GetByVehiculoIdAsync(dto.VehiculoId);
                    var conflicto = asignaciones.FirstOrDefault(a => 
                        a.Id != dto.Id &&
                        a.FechaAsignacion.Date == dto.FechaAsignacion.Date && 
                        (a.EstadoId == 1 || a.EstadoId == 2));
                    if (conflicto != null)
                        throw new InvalidOperationException("Vehículo ya asignado en esa fecha");
                }

                double combustibleEstimado = (rutaResponse.Distancia * vehiculoResponse.ConsumoCombustibleKm) / 100;

                // Guardar valores originales antes de actualizar
                var choferOriginalId = existente.ChoferId;
                var vehiculoOriginalId = existente.VehiculoId;

                existente.ChoferId = dto.ChoferId;
                existente.VehiculoId = dto.VehiculoId;
                existente.RutaId = dto.RutaId;
                existente.FechaAsignacion = dto.FechaAsignacion;
                existente.CombustibleEstimado = combustibleEstimado;
                if (dto.EstadoId.HasValue)
                    existente.EstadoId = dto.EstadoId.Value;

                var resultado = await _asignacionRepo.UpdateAsync(existente);

                // Gestionar cambios de disponibilidad
                // Si cambió el chofer: liberar el anterior y ocupar el nuevo
                if (dto.ChoferId != choferOriginalId)
                {
                    try
                    {
                        var liberarChoferRequest = new MS.Choferes.Protos.ActualizarDisponibilidadRequest
                        {
                            Id = choferOriginalId,
                            Disponible = true
                        };
                        await _choferesClient.ActualizarDisponibilidadChoferAsync(liberarChoferRequest);
                        _logger.LogInformation($"Chofer anterior ID {choferOriginalId} liberado");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"No se pudo liberar chofer anterior: {ex.Message}");
                    }

                    try
                    {
                        var ocuparChoferRequest = new MS.Choferes.Protos.ActualizarDisponibilidadRequest
                        {
                            Id = dto.ChoferId,
                            Disponible = false
                        };
                        await _choferesClient.ActualizarDisponibilidadChoferAsync(ocuparChoferRequest);
                        _logger.LogInformation($"Nuevo chofer ID {dto.ChoferId} ocupado");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"No se pudo ocupar nuevo chofer: {ex.Message}");
                    }
                }

                // Si cambió el vehículo: liberar el anterior y ocupar el nuevo
                if (dto.VehiculoId != vehiculoOriginalId)
                {
                    try
                    {
                        var liberarVehiculoRequest = new MS.Vehiculos.Protos.ActualizarDisponibilidadRequest
                        {
                            Id = vehiculoOriginalId,
                            Disponible = "Disponible"
                        };
                        await _vehiculosClient.ActualizarDisponibilidadAsync(liberarVehiculoRequest);
                        _logger.LogInformation($"Vehículo anterior ID {vehiculoOriginalId} liberado");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"No se pudo liberar vehículo anterior: {ex.Message}");
                    }

                    try
                    {
                        var ocuparVehiculoRequest = new MS.Vehiculos.Protos.ActualizarDisponibilidadRequest
                        {
                            Id = dto.VehiculoId,
                            Disponible = "No Disponible"
                        };
                        await _vehiculosClient.ActualizarDisponibilidadAsync(ocuparVehiculoRequest);
                        _logger.LogInformation($"Nuevo vehículo ID {dto.VehiculoId} ocupado");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"No se pudo ocupar nuevo vehículo: {ex.Message}");
                    }
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CambiarEstadoAsync(int id, string nuevoEstado)
        {
            try
            {
                var asignacion = await _asignacionRepo.GetByIdAsync(id);
                if (asignacion == null)
                    throw new KeyNotFoundException($"Asignación con ID {id} no existe");

                var estado = await _estadoRepo.GetByNombreAsync(nuevoEstado);
                if (estado == null)
                    throw new KeyNotFoundException($"Estado '{nuevoEstado}' no existe");

                var estadoActual = await _estadoRepo.GetByIdAsync(asignacion.EstadoId);
                if (estadoActual != null)
                {
                    ValidarTransicionEstado(estadoActual.Nombre, nuevoEstado);
                }

                var resultado = await _asignacionRepo.UpdateEstadoAsync(id, estado.Id);

                // Liberar recursos si la asignación se completa o cancela
                if (nuevoEstado == "Completada" || nuevoEstado == "Cancelada")
                {
                    try
                    {
                        var liberarChoferRequest = new MS.Choferes.Protos.ActualizarDisponibilidadRequest
                        {
                            Id = asignacion.ChoferId,
                            Disponible = true
                        };
                        await _choferesClient.ActualizarDisponibilidadChoferAsync(liberarChoferRequest);
                        _logger.LogInformation($"Chofer ID {asignacion.ChoferId} liberado (asignación {nuevoEstado})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"No se pudo liberar chofer: {ex.Message}");
                    }

                    try
                    {
                        var liberarVehiculoRequest = new MS.Vehiculos.Protos.ActualizarDisponibilidadRequest
                        {
                            Id = asignacion.VehiculoId,
                            Disponible = "Disponible"
                        };
                        await _vehiculosClient.ActualizarDisponibilidadAsync(liberarVehiculoRequest);
                        _logger.LogInformation($"Vehículo ID {asignacion.VehiculoId} liberado (asignación {nuevoEstado})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"No se pudo liberar vehículo: {ex.Message}");
                    }
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }
        }

        private void ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            var transicionesValidas = new Dictionary<string, List<string>>
            {
                { "Asignada", new List<string> { "En Proceso", "Cancelada" } },
                { "En Proceso", new List<string> { "Completada", "Pausada", "Cancelada" } },
                { "Pausada", new List<string> { "En Proceso", "Cancelada" } },
                { "Completada", new List<string>() },
                { "Cancelada", new List<string>() }
            };

            if (transicionesValidas.ContainsKey(estadoActual))
            {
                if (!transicionesValidas[estadoActual].Contains(nuevoEstado))
                {
                    throw new InvalidOperationException(
                        $"No se puede cambiar de estado '{estadoActual}' a '{nuevoEstado}'");
                }
            }
        }

        public async Task<IEnumerable<AsignacionRutaDto>> GetAllAsync()
        {
            var asignaciones = await _asignacionRepo.GetAllAsync();
            return asignaciones.Select(a => new AsignacionRutaDto
            {
                Id = a.Id,
                ChoferId = a.ChoferId,
                VehiculoId = a.VehiculoId,
                RutaId = a.RutaId,
                FechaAsignacion = a.FechaAsignacion,
                CombustibleEstimado = a.CombustibleEstimado,
                EstadoId = a.EstadoId
            });
        }

        public async Task<AsignacionRutaDto?> GetByIdAsync(int id)
        {
            var asignacion = await _asignacionRepo.GetByIdAsync(id);
            if (asignacion == null) return null;
            return new AsignacionRutaDto
            {
                Id = asignacion.Id,
                ChoferId = asignacion.ChoferId,
                VehiculoId = asignacion.VehiculoId,
                RutaId = asignacion.RutaId,
                FechaAsignacion = asignacion.FechaAsignacion,
                CombustibleEstimado = asignacion.CombustibleEstimado,
                EstadoId = asignacion.EstadoId
            };
        }

        public async Task<IEnumerable<AsignacionRutaDto>> GetByChoferIdAsync(int choferId)
        {
            var asignaciones = await _asignacionRepo.GetByChoferIdAsync(choferId);
            return asignaciones.Select(a => new AsignacionRutaDto
            {
                Id = a.Id,
                ChoferId = a.ChoferId,
                VehiculoId = a.VehiculoId,
                RutaId = a.RutaId,
                FechaAsignacion = a.FechaAsignacion,
                CombustibleEstimado = a.CombustibleEstimado,
                EstadoId = a.EstadoId
            });
        }

        public async Task<IEnumerable<AsignacionRutaDto>> GetByVehiculoIdAsync(int vehiculoId)
        {
            var asignaciones = await _asignacionRepo.GetByVehiculoIdAsync(vehiculoId);
            return asignaciones.Select(a => new AsignacionRutaDto
            {
                Id = a.Id,
                ChoferId = a.ChoferId,
                VehiculoId = a.VehiculoId,
                RutaId = a.RutaId,
                FechaAsignacion = a.FechaAsignacion,
                CombustibleEstimado = a.CombustibleEstimado,
                EstadoId = a.EstadoId
            });
        }

        public async Task<IEnumerable<AsignacionRutaDto>> GetByEstadoAsync(int estadoId)
        {
            var asignaciones = await _asignacionRepo.GetByEstadoIdAsync(estadoId);
            return asignaciones.Select(a => new AsignacionRutaDto
            {
                Id = a.Id,
                ChoferId = a.ChoferId,
                VehiculoId = a.VehiculoId,
                RutaId = a.RutaId,
                FechaAsignacion = a.FechaAsignacion,
                CombustibleEstimado = a.CombustibleEstimado,
                EstadoId = a.EstadoId
            });
        }

        public async Task<IEnumerable<AsignacionRutaDto>> GetByEstadoAsync(string estadoNombre)
        {
            var estado = await _estadoRepo.GetByNombreAsync(estadoNombre);
            if (estado == null)
                throw new KeyNotFoundException($"Estado '{estadoNombre}' no existe");

            return await GetByEstadoAsync(estado.Id);
        }
    }
}