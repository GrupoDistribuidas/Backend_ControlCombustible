using MS.Combustible.Application.DTOs;
using MS.Combustible.Domain.Entities;
using MS.Combustible.Domain.Interfaces;

namespace MS.Combustible.Application.Services
{
    public class RegistroConsumoService
    {
        private readonly IRegistroConsumoRepository _registroRepo;
        private readonly IEstadoRegistroConsumoRepository _estadoRepo;
        private readonly IAsignacionRutaRepository _asignacionRepo;
        private readonly AsignacionRutaService _asignacionService;
        private readonly ILogger<RegistroConsumoService> _logger;

        public RegistroConsumoService(
            IRegistroConsumoRepository registroRepo,
            IEstadoRegistroConsumoRepository estadoRepo,
            IAsignacionRutaRepository asignacionRepo,
            AsignacionRutaService asignacionService,
            ILogger<RegistroConsumoService> logger)
        {
            _registroRepo = registroRepo;
            _estadoRepo = estadoRepo;
            _asignacionRepo = asignacionRepo;
            _asignacionService = asignacionService;
            _logger = logger;
        }

        /// <summary>
        /// Crea un registro de consumo y automáticamente marca la asignación como "Completada",
        /// liberando los recursos (chofer y vehículo) asociados.
        /// Solo permite crear registros para asignaciones en estado "En Proceso" (EstadoId = 2).
        /// </summary>
        public async Task<int> CrearRegistroConsumoAsync(CrearRegistroConsumoDto dto)
        {
            try
            {
                // Validaciones básicas
                var errors = new List<string>();
                if (dto.AsignacionRutaId <= 0) errors.Add("AsignacionRutaId inválido");
                if (dto.FechaRegistro == default) errors.Add("FechaRegistro es obligatoria");
                if (dto.CombustibleEstimado < 0) errors.Add("CombustibleEstimado debe ser mayor o igual a 0");
                if (dto.CombustibleReal < 0) errors.Add("CombustibleReal debe ser mayor o igual a 0");
                if (dto.EstadoId <= 0) errors.Add("EstadoId inválido");
                if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

                // Verificar que la asignación de ruta existe
                var asignacion = await _asignacionRepo.GetByIdAsync(dto.AsignacionRutaId);
                if (asignacion == null)
                    throw new KeyNotFoundException($"Asignación de ruta {dto.AsignacionRutaId} no existe");

                // Verificar que la asignación esté en estado válido para crear registro (debe estar "En Proceso")
                // Estados válidos: 1=Asignada, 2=En Proceso - solo permitir crear registro si está "En Proceso"
                if (asignacion.EstadoId != 2)
                    throw new InvalidOperationException($"Solo se pueden crear registros de consumo para asignaciones 'En Proceso'. Estado actual: {asignacion.EstadoId}");

                // Verificar que no existe un registro previo para esta asignación
                var registroExistente = await _registroRepo.ExistsByAsignacionRutaIdAsync(dto.AsignacionRutaId);
                if (registroExistente)
                    throw new InvalidOperationException($"Ya existe un registro de consumo para la asignación {dto.AsignacionRutaId}");

                // Verificar que el estado existe
                var estado = await _estadoRepo.GetByIdAsync(dto.EstadoId);
                if (estado == null)
                    throw new KeyNotFoundException($"Estado {dto.EstadoId} no existe");

                // Validación especial: Si consumo real > estimado, motivo es obligatorio
                if (dto.CombustibleReal > dto.CombustibleEstimado && string.IsNullOrWhiteSpace(dto.Motivo))
                    throw new ArgumentException("El motivo es obligatorio cuando el consumo real supera el estimado");

                // Validar fecha del registro (no puede ser futura)
                if (dto.FechaRegistro.Date > DateTime.Now.Date)
                    throw new ArgumentException("La fecha de registro no puede ser futura");

                // Validar que la fecha de registro no sea anterior a la fecha de asignación
                if (dto.FechaRegistro.Date < asignacion.FechaAsignacion.Date)
                    throw new ArgumentException("La fecha de registro no puede ser anterior a la fecha de asignación");

                var registro = new RegistroConsumo
                {
                    AsignacionRutaId = dto.AsignacionRutaId,
                    FechaRegistro = dto.FechaRegistro,
                    CombustibleEstimado = dto.CombustibleEstimado,
                    CombustibleReal = dto.CombustibleReal,
                    Motivo = dto.Motivo?.Trim() ?? string.Empty,
                    EstadoId = dto.EstadoId,
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now
                };

                var registroId = await _registroRepo.CreateAsync(registro);
                _logger.LogInformation($"Registro de consumo creado exitosamente con ID {registroId}");

                // Cambiar automáticamente el estado de la asignación a "Completada"
                try
                {
                    await _asignacionService.CambiarEstadoAsync(dto.AsignacionRutaId, "Completada");
                    _logger.LogInformation($"Asignación {dto.AsignacionRutaId} marcada como 'Completada' automáticamente al crear registro de consumo");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"No se pudo cambiar automáticamente el estado de la asignación {dto.AsignacionRutaId} a 'Completada': {ex.Message}");
                    // No lanzamos excepción porque el registro ya se creó exitosamente
                }

                return registroId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear registro de consumo: {ex.Message}");
                throw;
            }
        }

        public async Task<int> ActualizarRegistroConsumoAsync(ActualizarRegistroConsumoDto dto)
        {
            try
            {
                // Validaciones básicas
                var errors = new List<string>();
                if (dto.Id <= 0) errors.Add("Id inválido");
                if (dto.FechaRegistro == default) errors.Add("FechaRegistro es obligatoria");
                if (dto.CombustibleEstimado < 0) errors.Add("CombustibleEstimado debe ser mayor o igual a 0");
                if (dto.CombustibleReal < 0) errors.Add("CombustibleReal debe ser mayor o igual a 0");
                if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

                // Verificar que el registro existe
                var registroExistente = await _registroRepo.GetByIdAsync(dto.Id);
                if (registroExistente == null)
                    throw new KeyNotFoundException($"Registro de consumo {dto.Id} no existe");

                // Verificar que la asignación de ruta existe
                var asignacion = await _asignacionRepo.GetByIdAsync(registroExistente.AsignacionRutaId);
                if (asignacion == null)
                    throw new KeyNotFoundException($"Asignación de ruta {registroExistente.AsignacionRutaId} no existe");

                // Validación especial: Si consumo real > estimado, motivo es obligatorio
                if (dto.CombustibleReal > dto.CombustibleEstimado && string.IsNullOrWhiteSpace(dto.Motivo))
                    throw new ArgumentException("El motivo es obligatorio cuando el consumo real supera el estimado");

                // Validar fecha del registro (no puede ser futura)
                if (dto.FechaRegistro.Date > DateTime.Now.Date)
                    throw new ArgumentException("La fecha de registro no puede ser futura");

                // Validar que la fecha de registro no sea anterior a la fecha de asignación
                if (dto.FechaRegistro.Date < asignacion.FechaAsignacion.Date)
                    throw new ArgumentException("La fecha de registro no puede ser anterior a la fecha de asignación");

                // Verificar estado si se proporciona
                if (dto.EstadoId.HasValue)
                {
                    var estado = await _estadoRepo.GetByIdAsync(dto.EstadoId.Value);
                    if (estado == null)
                        throw new KeyNotFoundException($"Estado {dto.EstadoId.Value} no existe");
                }

                // Actualizar los campos
                registroExistente.FechaRegistro = dto.FechaRegistro;
                registroExistente.CombustibleEstimado = dto.CombustibleEstimado;
                registroExistente.CombustibleReal = dto.CombustibleReal;
                registroExistente.Motivo = dto.Motivo?.Trim() ?? string.Empty;
                if (dto.EstadoId.HasValue)
                    registroExistente.EstadoId = dto.EstadoId.Value;
                registroExistente.FechaModificacion = DateTime.Now;

                var resultado = await _registroRepo.UpdateAsync(registroExistente);
                _logger.LogInformation($"Registro de consumo {dto.Id} actualizado exitosamente");
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar registro de consumo: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CambiarEstadoAsync(int id, string nuevoEstado)
        {
            try
            {
                var registro = await _registroRepo.GetByIdAsync(id);
                if (registro == null)
                    throw new KeyNotFoundException($"Registro de consumo con ID {id} no existe");

                var estado = await _estadoRepo.GetByNombreAsync(nuevoEstado);
                if (estado == null)
                    throw new KeyNotFoundException($"Estado '{nuevoEstado}' no existe");

                var estadoActual = await _estadoRepo.GetByIdAsync(registro.EstadoId);
                if (estadoActual != null)
                {
                    ValidarTransicionEstado(estadoActual.Nombre, nuevoEstado);
                }

                var resultado = await _registroRepo.UpdateEstadoAsync(id, estado.Id);
                _logger.LogInformation($"Estado del registro {id} cambiado a '{nuevoEstado}'");
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cambiar estado del registro: {ex.Message}");
                throw;
            }
        }

        private void ValidarTransicionEstado(string estadoActual, string nuevoEstado)
        {
            // Definir transiciones válidas para registros de consumo
            var transicionesValidas = new Dictionary<string, List<string>>
            {
                { "Borrador", new List<string> { "Pendiente", "Cancelado" } },
                { "Pendiente", new List<string> { "Aprobado", "Rechazado", "Cancelado" } },
                { "Aprobado", new List<string> { "Finalizado" } },
                { "Rechazado", new List<string> { "Pendiente", "Cancelado" } },
                { "Finalizado", new List<string>() },
                { "Cancelado", new List<string>() }
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

        public async Task<IEnumerable<RegistroConsumoDto>> GetAllAsync()
        {
            var registros = await _registroRepo.GetAllAsync();
            var resultado = new List<RegistroConsumoDto>();

            foreach (var registro in registros)
            {
                var estado = await _estadoRepo.GetByIdAsync(registro.EstadoId);
                resultado.Add(new RegistroConsumoDto
                {
                    Id = registro.Id,
                    AsignacionRutaId = registro.AsignacionRutaId,
                    FechaRegistro = registro.FechaRegistro,
                    CombustibleEstimado = registro.CombustibleEstimado,
                    CombustibleReal = registro.CombustibleReal,
                    Motivo = registro.Motivo,
                    EstadoId = registro.EstadoId,
                    EstadoNombre = estado?.Nombre ?? string.Empty,
                    FechaCreacion = registro.FechaCreacion,
                    FechaModificacion = registro.FechaModificacion
                });
            }

            return resultado;
        }

        public async Task<RegistroConsumoDto?> GetByIdAsync(int id)
        {
            var registro = await _registroRepo.GetByIdAsync(id);
            if (registro == null) return null;

            var estado = await _estadoRepo.GetByIdAsync(registro.EstadoId);
            return new RegistroConsumoDto
            {
                Id = registro.Id,
                AsignacionRutaId = registro.AsignacionRutaId,
                FechaRegistro = registro.FechaRegistro,
                CombustibleEstimado = registro.CombustibleEstimado,
                CombustibleReal = registro.CombustibleReal,
                Motivo = registro.Motivo,
                EstadoId = registro.EstadoId,
                EstadoNombre = estado?.Nombre ?? string.Empty,
                FechaCreacion = registro.FechaCreacion,
                FechaModificacion = registro.FechaModificacion
            };
        }

        public async Task<IEnumerable<RegistroConsumoDto>> GetByAsignacionRutaIdAsync(int asignacionRutaId)
        {
            var registros = await _registroRepo.GetByAsignacionRutaIdAsync(asignacionRutaId);
            var resultado = new List<RegistroConsumoDto>();

            foreach (var registro in registros)
            {
                var estado = await _estadoRepo.GetByIdAsync(registro.EstadoId);
                resultado.Add(new RegistroConsumoDto
                {
                    Id = registro.Id,
                    AsignacionRutaId = registro.AsignacionRutaId,
                    FechaRegistro = registro.FechaRegistro,
                    CombustibleEstimado = registro.CombustibleEstimado,
                    CombustibleReal = registro.CombustibleReal,
                    Motivo = registro.Motivo,
                    EstadoId = registro.EstadoId,
                    EstadoNombre = estado?.Nombre ?? string.Empty,
                    FechaCreacion = registro.FechaCreacion,
                    FechaModificacion = registro.FechaModificacion
                });
            }

            return resultado;
        }

        public async Task<IEnumerable<RegistroConsumoDto>> GetByEstadoAsync(string estadoNombre)
        {
            var estado = await _estadoRepo.GetByNombreAsync(estadoNombre);
            if (estado == null)
                throw new KeyNotFoundException($"Estado '{estadoNombre}' no existe");

            return await GetByEstadoAsync(estado.Id);
        }

        public async Task<IEnumerable<RegistroConsumoDto>> GetByEstadoAsync(int estadoId)
        {
            var registros = await _registroRepo.GetByEstadoIdAsync(estadoId);
            var estado = await _estadoRepo.GetByIdAsync(estadoId);
            var estadoNombre = estado?.Nombre ?? string.Empty;

            return registros.Select(registro => new RegistroConsumoDto
            {
                Id = registro.Id,
                AsignacionRutaId = registro.AsignacionRutaId,
                FechaRegistro = registro.FechaRegistro,
                CombustibleEstimado = registro.CombustibleEstimado,
                CombustibleReal = registro.CombustibleReal,
                Motivo = registro.Motivo,
                EstadoId = registro.EstadoId,
                EstadoNombre = estadoNombre,
                FechaCreacion = registro.FechaCreacion,
                FechaModificacion = registro.FechaModificacion
            });
        }

        public async Task<IEnumerable<RegistroConsumoDto>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
                throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha de fin");

            var registros = await _registroRepo.GetByFechaRangoAsync(fechaInicio, fechaFin);
            var resultado = new List<RegistroConsumoDto>();

            foreach (var registro in registros)
            {
                var estado = await _estadoRepo.GetByIdAsync(registro.EstadoId);
                resultado.Add(new RegistroConsumoDto
                {
                    Id = registro.Id,
                    AsignacionRutaId = registro.AsignacionRutaId,
                    FechaRegistro = registro.FechaRegistro,
                    CombustibleEstimado = registro.CombustibleEstimado,
                    CombustibleReal = registro.CombustibleReal,
                    Motivo = registro.Motivo,
                    EstadoId = registro.EstadoId,
                    EstadoNombre = estado?.Nombre ?? string.Empty,
                    FechaCreacion = registro.FechaCreacion,
                    FechaModificacion = registro.FechaModificacion
                });
            }

            return resultado;
        }
    }
}