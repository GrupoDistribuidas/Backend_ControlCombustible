using System;

namespace MS.Combustible.Application.DTOs
{
    public class ActualizarAsignacionRutaDto
    {
        public int Id { get; set; }
        public int ChoferId { get; set; }
        public int VehiculoId { get; set; }
        public int RutaId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public int? EstadoId { get; set; }
    }
}
