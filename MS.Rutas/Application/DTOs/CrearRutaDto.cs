using System;

namespace MS.Rutas.Application.DTOs
{
    public class CrearRutaDto
    {
        public string Nombre { get; set; } = null!;
        public int PuntoInicioId { get; set; }
        public int PuntoFinId { get; set; }
        public double Distancia { get; set; }
    }
}
