using MS.Rutas.Application.DTOs;
using MS.Rutas.Domain.Entities;
using MS.Rutas.Domain.Interfaces;
using System.Linq;

namespace MS.Rutas.Application.Services
{
    public class RutaService
    {
        private readonly IRutaRepository _repo;
        private readonly IPuntoRepository _puntoRepo;

        public RutaService(IRutaRepository repo, IPuntoRepository puntoRepo)
        {
            _repo = repo;
            _puntoRepo = puntoRepo;
        }

        public async Task<int> CrearRutaAsync(CrearRutaDto dto)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errors.Add("Nombre es obligatorio");
            if (dto.PuntoInicioId <= 0) errors.Add("PuntoInicioId inválido");
            if (dto.PuntoFinId <= 0) errors.Add("PuntoFinId inválido");
            if (dto.Distancia <= 0) errors.Add("Distancia debe ser mayor a 0");
            
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            // Validar que los puntos existen
            var puntoInicio = await _puntoRepo.GetByIdAsync(dto.PuntoInicioId);
            if (puntoInicio == null) throw new ArgumentException($"Punto de inicio con ID {dto.PuntoInicioId} no existe");

            var puntoFin = await _puntoRepo.GetByIdAsync(dto.PuntoFinId);
            if (puntoFin == null) throw new ArgumentException($"Punto final con ID {dto.PuntoFinId} no existe");

            // Validar que no sean el mismo punto
            if (dto.PuntoInicioId == dto.PuntoFinId)
                throw new ArgumentException("El punto de inicio y el punto final no pueden ser el mismo");

            // Verificar nombre único
            var existing = await _repo.GetByNombreAsync(dto.Nombre);
            if (existing != null) throw new ArgumentException("Ya existe una ruta con ese nombre");

            var ruta = new Ruta
            {
                Nombre = dto.Nombre,
                PuntoInicioId = dto.PuntoInicioId,
                PuntoFinId = dto.PuntoFinId,
                Distancia = dto.Distancia,
                Estado = true
            };

            return await _repo.CreateAsync(ruta);
        }

        public async Task<IEnumerable<RutaDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(r => new RutaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                PuntoInicioId = r.PuntoInicioId,
                PuntoFinId = r.PuntoFinId,
                Distancia = r.Distancia,
                Estado = r.Estado
            });
        }

        public async Task<IEnumerable<RutaDto>> GetAllIncludingInactiveAsync()
        {
            var list = await _repo.GetAllIncludingInactiveAsync();
            return list.Select(r => new RutaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                PuntoInicioId = r.PuntoInicioId,
                PuntoFinId = r.PuntoFinId,
                Distancia = r.Distancia,
                Estado = r.Estado
            });
        }

        public async Task<RutaDto?> GetByIdAsync(int id)
        {
            var ruta = await _repo.GetByIdAsync(id);
            if (ruta == null) return null;

            return new RutaDto
            {
                Id = ruta.Id,
                Nombre = ruta.Nombre,
                PuntoInicioId = ruta.PuntoInicioId,
                PuntoFinId = ruta.PuntoFinId,
                Distancia = ruta.Distancia,
                Estado = ruta.Estado
            };
        }

        public async Task<int> ActualizarRutaAsync(ActualizarRutaDto dto)
        {
            var errors = new List<string>();
            if (dto.Id <= 0) errors.Add("Id inválido");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errors.Add("Nombre es obligatorio");
            if (dto.PuntoInicioId <= 0) errors.Add("PuntoInicioId inválido");
            if (dto.PuntoFinId <= 0) errors.Add("PuntoFinId inválido");
            if (dto.Distancia <= 0) errors.Add("Distancia debe ser mayor a 0");
            
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null) throw new ArgumentException("Ruta no encontrada");

            // Validar que los puntos existen
            var puntoInicio = await _puntoRepo.GetByIdAsync(dto.PuntoInicioId);
            if (puntoInicio == null) throw new ArgumentException($"Punto de inicio con ID {dto.PuntoInicioId} no existe");

            var puntoFin = await _puntoRepo.GetByIdAsync(dto.PuntoFinId);
            if (puntoFin == null) throw new ArgumentException($"Punto final con ID {dto.PuntoFinId} no existe");

            // Validar que no sean el mismo punto
            if (dto.PuntoInicioId == dto.PuntoFinId)
                throw new ArgumentException("El punto de inicio y el punto final no pueden ser el mismo");

            var byNombre = await _repo.GetByNombreAsync(dto.Nombre);
            if (byNombre != null && byNombre.Id != dto.Id) 
                throw new ArgumentException("Ya existe una ruta con ese nombre");

            var ruta = new Ruta
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                PuntoInicioId = dto.PuntoInicioId,
                PuntoFinId = dto.PuntoFinId,
                Distancia = dto.Distancia,
                Estado = dto.Estado ?? existing.Estado
            };

            return await _repo.UpdateAsync(ruta, dto.Estado);
        }

        public async Task<int> ActualizarEstadoAsync(int id, bool estado)
        {
            if (id <= 0) throw new ArgumentException("Id inválido");
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new ArgumentException("Ruta no encontrada");
            return await _repo.UpdateEstadoAsync(id, estado);
        }

        public async Task<IEnumerable<RutaDto>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Enumerable.Empty<RutaDto>();
            var list = await _repo.SearchByTermAsync(term);
            return list.Select(r => new RutaDto
            {
                Id = r.Id,
                Nombre = r.Nombre,
                PuntoInicioId = r.PuntoInicioId,
                PuntoFinId = r.PuntoFinId,
                Distancia = r.Distancia,
                Estado = r.Estado
            });
        }
    }
}
