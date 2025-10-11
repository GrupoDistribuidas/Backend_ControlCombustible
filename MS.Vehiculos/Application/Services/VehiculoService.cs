using MS.Vehiculos.Application.DTOs;
using MS.Vehiculos.Domain.Entities;
using MS.Vehiculos.Domain.Interfaces;
using System.Linq;

namespace MS.Vehiculos.Application.Services
{
    public class VehiculoService
    {
        private readonly IVehiculoRepository _repo;
        private readonly MS.Vehiculos.Domain.Interfaces.ITipoMaquinariaRepository _tipoRepo;

        public VehiculoService(IVehiculoRepository repo, MS.Vehiculos.Domain.Interfaces.ITipoMaquinariaRepository tipoRepo)
        {
            _repo = repo;
            _tipoRepo = tipoRepo;
        }

        public async Task<int> CrearVehiculoAsync(CrearVehiculoDto dto)
        {
            // Validaciones
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(dto.Placa)) errors.Add("Placa es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errors.Add("Nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.Marca)) errors.Add("Marca es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.Modelo)) errors.Add("Modelo es obligatorio");
            if (dto.TipoMaquinariaId <= 0) errors.Add("TipoMaquinariaId inválido");
            if (dto.ConsumoCombustibleKm <= 0) errors.Add("ConsumoCombustibleKm debe ser mayor que 0");
            if (dto.CapacidadCombustible <= 0) errors.Add("CapacidadCombustible debe ser mayor que 0");
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            // Verificar existencia de tipo de maquinaria si el repo está disponible
            if (_tipoRepo != null)
            {
                var tipo = await _tipoRepo.GetByIdAsync(dto.TipoMaquinariaId);
                if (tipo == null) throw new ArgumentException("TipoMaquinaria no existe");
            }

            // Verificar placa única
            var existing = await _repo.GetByPlacaAsync(dto.Placa);
            if (existing != null) throw new ArgumentException("Placa ya registrada");

            var veh = new Vehiculo
            {
                Nombre = dto.Nombre,
                Placa = dto.Placa,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                TipoMaquinariaId = dto.TipoMaquinariaId,
                Disponible = dto.Disponible,
                ConsumoCombustibleKm = dto.ConsumoCombustibleKm,
                CapacidadCombustible = dto.CapacidadCombustible,
                Estado = true
            };

            return await _repo.CreateAsync(veh);
        }

        public async Task<IEnumerable<VehiculoDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(v => new VehiculoDto
            {
                Id = v.Id,
                Nombre = v.Nombre,
                Placa = v.Placa,
                Marca = v.Marca,
                Modelo = v.Modelo,
                TipoMaquinariaId = v.TipoMaquinariaId,
                Disponible = v.Disponible,
                ConsumoCombustibleKm = v.ConsumoCombustibleKm,
                CapacidadCombustible = v.CapacidadCombustible
            });
        }

        public async Task<int> ActualizarVehiculoAsync(ActualizarVehiculoDto dto)
        {
            // Validaciones
            var errors = new List<string>();
            if (dto.Id <= 0) errors.Add("Id inválido");
            if (string.IsNullOrWhiteSpace(dto.Placa)) errors.Add("Placa es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.Nombre)) errors.Add("Nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(dto.Marca)) errors.Add("Marca es obligatoria");
            if (string.IsNullOrWhiteSpace(dto.Modelo)) errors.Add("Modelo es obligatorio");
            if (dto.TipoMaquinariaId <= 0) errors.Add("TipoMaquinariaId inválido");
            if (dto.ConsumoCombustibleKm <= 0) errors.Add("ConsumoCombustibleKm debe ser mayor que 0");
            if (dto.CapacidadCombustible <= 0) errors.Add("CapacidadCombustible debe ser mayor que 0");
            if (errors.Any()) throw new ArgumentException(string.Join("; ", errors));

            // Existe vehículo?
            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null) throw new ArgumentException("Vehículo no encontrado");

            // placa única
            var byPlaca = await _repo.GetByPlacaAsync(dto.Placa);
            if (byPlaca != null && byPlaca.Id != dto.Id) throw new ArgumentException("Placa ya registrada");

            // Tipo existe
            var tipo = await _tipoRepo.GetByIdAsync(dto.TipoMaquinariaId);
            if (tipo == null) throw new ArgumentException("TipoMaquinaria no existe");

            var veh = new MS.Vehiculos.Domain.Entities.Vehiculo
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Placa = dto.Placa,
                Marca = dto.Marca,
                Modelo = dto.Modelo,
                TipoMaquinariaId = dto.TipoMaquinariaId,
                Disponible = dto.Disponible,
                ConsumoCombustibleKm = dto.ConsumoCombustibleKm,
                CapacidadCombustible = dto.CapacidadCombustible,
                Estado = dto.Estado ?? existing.Estado
            };

            return await _repo.UpdateAsync(veh, dto.Estado);
        }

        public async Task<int> ActualizarEstadoAsync(int id, bool estado)
        {
            if (id <= 0) throw new ArgumentException("Id inválido");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new ArgumentException("Vehículo no encontrado");

            existing.Estado = estado;
            return await _repo.UpdateAsync(existing, estado);
        }

        public async Task<bool> ExistsByPlacaAsync(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa)) return false;
            var existing = await _repo.GetByPlacaAsync(placa);
            return existing != null;
        }

        public async Task<IEnumerable<VehiculoDto>> SearchAsync(VehiculoFilterDto filter)
        {
            var list = await _repo.SearchAsync(filter);
            return list.Select(v => new VehiculoDto
            {
                Id = v.Id,
                Nombre = v.Nombre,
                Placa = v.Placa,
                Marca = v.Marca,
                Modelo = v.Modelo,
                TipoMaquinariaId = v.TipoMaquinariaId,
                Disponible = v.Disponible,
                ConsumoCombustibleKm = v.ConsumoCombustibleKm,
                CapacidadCombustible = v.CapacidadCombustible
            });
        }

        public async Task<IEnumerable<VehiculoDto>> SearchByTermAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Enumerable.Empty<VehiculoDto>();

            var list = await _repo.SearchByTermAsync(term);
            return list.Select(v => new VehiculoDto
            {
                Id = v.Id,
                Nombre = v.Nombre,
                Placa = v.Placa,
                Marca = v.Marca,
                Modelo = v.Modelo,
                TipoMaquinariaId = v.TipoMaquinariaId,
                Disponible = v.Disponible,
                ConsumoCombustibleKm = v.ConsumoCombustibleKm,
                CapacidadCombustible = v.CapacidadCombustible
            });
        }
    }
}
