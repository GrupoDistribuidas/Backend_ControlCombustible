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
    }
}
