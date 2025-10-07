using System.ComponentModel.DataAnnotations;

namespace MS.Autenticacion.Domain.Entities
{
    public class Rol
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; }
        // FechaModificacion no existe en la tabla Roles según el schema
    }
}