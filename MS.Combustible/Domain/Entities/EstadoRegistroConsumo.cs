using System;

namespace MS.Combustible.Domain.Entities
{
    public class EstadoRegistroConsumo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
