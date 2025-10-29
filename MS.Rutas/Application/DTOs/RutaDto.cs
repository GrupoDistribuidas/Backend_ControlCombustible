using System;

namespace MS.Rutas.Application.DTOs
{
    public class RutaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public int PuntoInicioId { get; set; }
        public int PuntoFinId { get; set; }
        public double Distancia { get; set; }
        public bool Estado { get; set; }
    }
}
