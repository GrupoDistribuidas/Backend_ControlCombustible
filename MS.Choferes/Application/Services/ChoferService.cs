using MS.Choferes.Application.DTOs;
using MS.Choferes.Domain.Entities;
using MS.Choferes.Domain.Interfaces;
using Grpc.Core;
using System.Linq;

namespace MS.Choferes.Application.Services
{
    public class ChoferService
    {
        private readonly IChoferRepository _repo;
        private readonly ITipoMaquinariaRepository _tipoRepo;
        private readonly MS.Autenticacion.Grpc.UserService.UserServiceClient _userClient;

        public ChoferService(
            IChoferRepository repo, 
            ITipoMaquinariaRepository tipoRepo,
            MS.Autenticacion.Grpc.UserService.UserServiceClient userClient)
        {
            _repo = repo;
            _tipoRepo = tipoRepo;
            _userClient = userClient;
        }

        public async Task<int> CrearChoferAsync(CrearChoferDto dto)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(dto.PrimerNombre)) errors.Add("PrimerNombre es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.PrimerApellido)) errors.Add("PrimerApellido es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.Identificacion)) errors.Add("Identificacion es obligatoria");
            if (dto.TipoMaquinariaId <= 0) errors.Add("TipoMaquinariaId inválido");
            // UsuarioId es opcional - se puede asignar después (0 significa sin usuario)
            
            // Validar edad >= 18
            var edad = DateTime.Today.Year - dto.FechaNacimiento.Year;
            if (dto.FechaNacimiento > DateTime.Today.AddYears(-edad)) edad--;
            if (edad < 18) errors.Add("El chofer debe ser mayor o igual a 18 años");

            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            // ✅ Validar que el usuario existe en MS.Autenticacion (solo si se proporciona un usuario)
            if (dto.UsuarioId > 0)
            {
                try
                {
                    var usuarioExisteRequest = new MS.Autenticacion.Grpc.ExisteUsuarioRequest { Id = dto.UsuarioId };
                    var usuarioExiste = await _userClient.ExisteUsuarioAsync(usuarioExisteRequest);
                    
                    if (!usuarioExiste.Existe)
                    {
                        throw new ArgumentException($"Usuario con ID {dto.UsuarioId} no existe");
                    }
                }
                catch (RpcException ex)
                {
                    throw new Exception($"Error al validar usuario en MS.Autenticacion: {ex.Status.Detail}", ex);
                }

                // ✅ Verificar que el usuario no esté asignado a otro chofer
                var choferConUsuario = await _repo.GetByUsuarioIdAsync(dto.UsuarioId);
                if (choferConUsuario != null)
                {
                    throw new ArgumentException($"El usuario con ID {dto.UsuarioId} ya está asignado al chofer '{choferConUsuario.NombreCompleto}'");
                }
            }

            // Verificar tipo
            var tipo = await _tipoRepo.GetByIdAsync(dto.TipoMaquinariaId);
            if (tipo == null) throw new ArgumentException("TipoMaquinaria no existe");

            // Verificar identificacion única
            var existing = await _repo.GetByIdentificacionAsync(dto.Identificacion);
            if (existing != null) throw new ArgumentException("Identificacion ya registrada");

            var chofer = new Chofer
            {
                PrimerNombre = dto.PrimerNombre,
                SegundoNombre = dto.SegundoNombre,
                PrimerApellido = dto.PrimerApellido,
                SegundoApellido = dto.SegundoApellido,
                Identificacion = dto.Identificacion,
                FechaNacimiento = dto.FechaNacimiento,
                Disponible = dto.Disponible,
                UsuarioId = dto.UsuarioId,
                TipoMaquinariaId = dto.TipoMaquinariaId,
                Estado = true
            };

            return await _repo.CreateAsync(chofer);
        }

        public async Task<IEnumerable<ChoferDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(c => new ChoferDto
            {
                Id = c.Id,
                PrimerNombre = c.PrimerNombre,
                SegundoNombre = c.SegundoNombre,
                PrimerApellido = c.PrimerApellido,
                SegundoApellido = c.SegundoApellido,
                NombreCompleto = c.NombreCompleto,
                Identificacion = c.Identificacion,
                FechaNacimiento = c.FechaNacimiento,
                Disponible = c.Disponible,
                UsuarioId = c.UsuarioId,
                TipoMaquinariaId = c.TipoMaquinariaId
            });
        }

        public async Task<int> ActualizarChoferAsync(ActualizarChoferDto dto)
        {
            var errors = new List<string>();
            if (dto.Id <= 0) errors.Add("Id inválido");
            if (string.IsNullOrWhiteSpace(dto.PrimerNombre)) errors.Add("PrimerNombre es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.PrimerApellido)) errors.Add("PrimerApellido es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.Identificacion)) errors.Add("Identificacion es obligatoria");
            if (dto.TipoMaquinariaId <= 0) errors.Add("TipoMaquinariaId inválido");
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null) throw new ArgumentException("Chofer no encontrado");

            var byIdent = await _repo.GetByIdentificacionAsync(dto.Identificacion);
            if (byIdent != null && byIdent.Id != dto.Id) throw new ArgumentException("Identificacion ya registrada");

            // Validar edad >=18
            var edad = DateTime.Today.Year - dto.FechaNacimiento.Year;
            if (dto.FechaNacimiento > DateTime.Today.AddYears(-edad)) edad--;
            if (edad < 18) throw new ArgumentException("El chofer debe ser mayor o igual a 18 años");

            var tipo = await _tipoRepo.GetByIdAsync(dto.TipoMaquinariaId);
            if (tipo == null) throw new ArgumentException("TipoMaquinaria no existe");

            // ✅ Validar usuario si se proporciona (solo si es > 0)
            if (dto.UsuarioId > 0)
            {
                try
                {
                    var usuarioExisteRequest = new MS.Autenticacion.Grpc.ExisteUsuarioRequest { Id = dto.UsuarioId };
                    var usuarioExiste = await _userClient.ExisteUsuarioAsync(usuarioExisteRequest);
                    
                    if (!usuarioExiste.Existe)
                    {
                        throw new ArgumentException($"Usuario con ID {dto.UsuarioId} no existe");
                    }
                }
                catch (RpcException ex)
                {
                    throw new Exception($"Error al validar usuario en MS.Autenticacion: {ex.Status.Detail}", ex);
                }

                // ✅ Verificar que el usuario no esté asignado a otro chofer (excepto al chofer actual)
                var choferConUsuario = await _repo.GetByUsuarioIdAsync(dto.UsuarioId);
                if (choferConUsuario != null && choferConUsuario.Id != dto.Id)
                {
                    throw new ArgumentException($"El usuario con ID {dto.UsuarioId} ya está asignado al chofer '{choferConUsuario.NombreCompleto}'");
                }
            }

            var chofer = new Chofer
            {
                Id = dto.Id,
                PrimerNombre = dto.PrimerNombre,
                SegundoNombre = dto.SegundoNombre,
                PrimerApellido = dto.PrimerApellido,
                SegundoApellido = dto.SegundoApellido,
                Identificacion = dto.Identificacion,
                FechaNacimiento = dto.FechaNacimiento,
                Disponible = dto.Disponible,
                UsuarioId = dto.UsuarioId,
                TipoMaquinariaId = dto.TipoMaquinariaId,
                Estado = dto.Estado ?? existing.Estado
            };

            return await _repo.UpdateAsync(chofer, dto.Estado);
        }

        public async Task<IEnumerable<ChoferDto>> SearchAsync(ChoferFilterDto filter)
        {
            var list = await _repo.SearchAsync(filter);
            return list.Select(c => new ChoferDto
            {
                Id = c.Id,
                PrimerNombre = c.PrimerNombre,
                SegundoNombre = c.SegundoNombre,
                PrimerApellido = c.PrimerApellido,
                SegundoApellido = c.SegundoApellido,
                NombreCompleto = c.NombreCompleto,
                Identificacion = c.Identificacion,
                FechaNacimiento = c.FechaNacimiento,
                Disponible = c.Disponible,
                UsuarioId = c.UsuarioId,
                TipoMaquinariaId = c.TipoMaquinariaId
            });
        }

        public async Task<int> AsignarUsuarioAsync(int choferId, int usuarioId)
        {
            // Validar que el chofer existe
            var chofer = await _repo.GetByIdAsync(choferId);
            if (chofer == null) throw new ArgumentException($"Chofer con ID {choferId} no encontrado");

            // Validar que el usuario existe en MS.Autenticacion
            try
            {
                var usuarioExisteRequest = new MS.Autenticacion.Grpc.ExisteUsuarioRequest { Id = usuarioId };
                var usuarioExiste = await _userClient.ExisteUsuarioAsync(usuarioExisteRequest);
                
                if (!usuarioExiste.Existe)
                {
                    throw new ArgumentException($"Usuario con ID {usuarioId} no existe");
                }
            }
            catch (RpcException ex)
            {
                throw new Exception($"Error al validar usuario en MS.Autenticacion: {ex.Status.Detail}", ex);
            }

            // Verificar que el usuario no esté asignado a otro chofer
            var choferConUsuario = await _repo.GetByUsuarioIdAsync(usuarioId);
            if (choferConUsuario != null && choferConUsuario.Id != choferId)
            {
                throw new ArgumentException($"El usuario con ID {usuarioId} ya está asignado al chofer '{choferConUsuario.NombreCompleto}'");
            }

            // Actualizar el usuario del chofer
            return await _repo.UpdateUsuarioAsync(choferId, usuarioId);
        }

        public async Task<IEnumerable<ChoferDto>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Enumerable.Empty<ChoferDto>();
            var list = await _repo.SearchByTermAsync(term);
            return list.Select(c => new ChoferDto
            {
                Id = c.Id,
                PrimerNombre = c.PrimerNombre,
                SegundoNombre = c.SegundoNombre,
                PrimerApellido = c.PrimerApellido,
                SegundoApellido = c.SegundoApellido,
                NombreCompleto = c.NombreCompleto,
                Identificacion = c.Identificacion,
                FechaNacimiento = c.FechaNacimiento,
                Disponible = c.Disponible,
                UsuarioId = c.UsuarioId,
                TipoMaquinariaId = c.TipoMaquinariaId
            });
        }

        public async Task<int> ActualizarEstadoAsync(int id, bool estado)
        {
            if (id <= 0) throw new ArgumentException("Id inválido");
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new ArgumentException("Chofer no encontrado");
            return await _repo.UpdateEstadoAsync(id, estado);
        }

        public async Task<int> ActualizarDisponibilidadAsync(int id, bool disponible)
        {
            if (id <= 0) throw new ArgumentException("Id inválido");
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new ArgumentException("Chofer no encontrado");
            return await _repo.UpdateDisponibilidadAsync(id, disponible);
        }

        public async Task<bool> ExistsByIdentificacionAsync(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion)) return false;
            var existing = await _repo.GetByIdentificacionAsync(identificacion);
            return existing != null;
        }
    }
}
