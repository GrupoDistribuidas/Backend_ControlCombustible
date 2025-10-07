using System.ComponentModel.DataAnnotations;

namespace MS.Autenticacion.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Hash BCrypt

        public int RolId { get; set; }
        public int Estado { get; set; } // 1=Activo, 0=Inactivo

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}