using System;

namespace MS.Combustible.Application.DTOs
{
    public class CrearAsignacionRutaDto
    {
        public int ChoferId { get; set; }
        public int VehiculoId { get; set; }
        public int RutaId { get; set; }
        public DateTime FechaAsignacion { get; set; }
    }
}
