using MS.Choferes.Application.DTOs;
using MS.Choferes.Domain.Interfaces;
using System.Linq;

namespace MS.Choferes.Application.Services
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
            var list = await _repo.GetAllAsync();
            return list.Select(t => new TipoMaquinariaDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion
            });
        }
    }
}
