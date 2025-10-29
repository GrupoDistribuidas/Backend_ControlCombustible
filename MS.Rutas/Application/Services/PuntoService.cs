using MS.Rutas.Application.DTOs;
using MS.Rutas.Domain.Entities;
using MS.Rutas.Domain.Interfaces;
using System.Linq;

namespace MS.Rutas.Application.Services
{
    public class PuntoService
    {
        private readonly IPuntoRepository _repo;

        public PuntoService(IPuntoRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> CrearPuntoAsync(PuntoDto dto)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errors.Add("Nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.Direccion)) errors.Add("Direccion es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.Provincia)) errors.Add("Provincia es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.TipoPunto)) errors.Add("TipoPunto es obligatorio");
            
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            // Verificar nombre único
            var existing = await _repo.GetByNombreAsync(dto.Nombre);
            if (existing != null) throw new ArgumentException("Ya existe un punto con ese nombre");

            var punto = new Punto
            {
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                Provincia = dto.Provincia,
                TipoPunto = dto.TipoPunto
            };

            return await _repo.CreateAsync(punto);
        }

        public async Task<IEnumerable<PuntoDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(p => new PuntoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Direccion = p.Direccion,
                Provincia = p.Provincia,
                TipoPunto = p.TipoPunto
            });
        }

        public async Task<PuntoDto?> GetByIdAsync(int id)
        {
            var punto = await _repo.GetByIdAsync(id);
            if (punto == null) return null;

            return new PuntoDto
            {
                Id = punto.Id,
                Nombre = punto.Nombre,
                Direccion = punto.Direccion,
                Provincia = punto.Provincia,
                TipoPunto = punto.TipoPunto
            };
        }

        public async Task<int> ActualizarPuntoAsync(PuntoDto dto)
        {
            var errors = new List<string>();
            if (dto.Id <= 0) errors.Add("Id inválido");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errors.Add("Nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.Direccion)) errors.Add("Direccion es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.Provincia)) errors.Add("Provincia es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.TipoPunto)) errors.Add("TipoPunto es obligatorio");
            
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null) throw new ArgumentException("Punto no encontrado");

            var byNombre = await _repo.GetByNombreAsync(dto.Nombre);
            if (byNombre != null && byNombre.Id != dto.Id) 
                throw new ArgumentException("Ya existe un punto con ese nombre");

            var punto = new Punto
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                Provincia = dto.Provincia,
                TipoPunto = dto.TipoPunto
            };

            return await _repo.UpdateAsync(punto);
        }

        public async Task<IEnumerable<PuntoDto>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Enumerable.Empty<PuntoDto>();
            var list = await _repo.SearchByTermAsync(term);
            return list.Select(p => new PuntoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Direccion = p.Direccion,
                Provincia = p.Provincia,
                TipoPunto = p.TipoPunto
            });
        }
    }
}
