using System;

namespace MS.Combustible.Domain.Entities
{
    public class AsignacionRuta
    {
        public int Id { get; set; }
        public int ChoferId { get; set; }
        public int VehiculoId { get; set; }
        public int RutaId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public double CombustibleEstimado { get; set; }
        public int EstadoId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
