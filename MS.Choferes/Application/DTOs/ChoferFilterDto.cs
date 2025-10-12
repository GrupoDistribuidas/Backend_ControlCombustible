using System;

namespace MS.Choferes.Application.DTOs
{
    public class ChoferFilterDto
    {
        public bool? Estado { get; set; }
        public int? TipoMaquinariaId { get; set; }
        public bool? Disponible { get; set; }
        public DateTime? FechaNacimientoDesde { get; set; }
        public DateTime? FechaNacimientoHasta { get; set; }
    }
}
