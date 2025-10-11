using MS.Vehiculos.Application.DTOs;
using MS.Vehiculos.Domain.Interfaces;
using MS.Vehiculos.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MS.Vehiculos.Application.Services
{
    public class TipoMaquinariaService
    {
        private readonly ITipoMaquinariaRepository _repo;

        public TipoMaquinariaService(ITipoMaquinariaRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<TipoMaquinariaDto>> GetAllAsync()
        {
            var tipos = await _repo.GetAllAsync();
            return tipos.Select(t => new TipoMaquinariaDto { Id = t.Id, Nombre = t.Nombre, Descripcion = t.Descripcion });
        }
    }
}
