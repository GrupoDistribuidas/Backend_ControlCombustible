using System;

namespace MS.Rutas.Domain.Entities
{
    public class Ruta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public int PuntoInicioId { get; set; }
        public int PuntoFinId { get; set; }
        public double Distancia { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
