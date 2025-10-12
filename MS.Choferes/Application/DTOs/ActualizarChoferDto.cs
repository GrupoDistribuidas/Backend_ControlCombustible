using System;

namespace MS.Choferes.Application.DTOs
{
    public class ActualizarChoferDto
    {
        public int Id { get; set; }
        public string PrimerNombre { get; set; } = null!;
        public string? SegundoNombre { get; set; }
        public string PrimerApellido { get; set; } = null!;
        public string? SegundoApellido { get; set; }
        public string Identificacion { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public bool Disponible { get; set; }
        public int UsuarioId { get; set; }
        public int TipoMaquinariaId { get; set; }
        public bool? Estado { get; set; }
    }
}
