using System;

namespace MS.Rutas.Application.DTOs
{
    public class PuntoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Provincia { get; set; } = null!;
        public string TipoPunto { get; set; } = null!;
    }
}
