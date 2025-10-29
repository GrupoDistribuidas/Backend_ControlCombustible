using System;

namespace MS.Rutas.Domain.Entities
{
    public class Punto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Provincia { get; set; } = null!;
        public string TipoPunto { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
