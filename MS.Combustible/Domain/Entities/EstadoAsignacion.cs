using System;

namespace MS.Combustible.Domain.Entities
{
    public class EstadoAsignacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
