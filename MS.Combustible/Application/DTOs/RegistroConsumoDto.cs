namespace MS.Combustible.Application.DTOs
{
    public class RegistroConsumoDto
    {
        public int Id { get; set; }
        public int AsignacionRutaId { get; set; }
        public DateTime FechaRegistro { get; set; }
        public double CombustibleEstimado { get; set; }
        public double CombustibleReal { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public int EstadoId { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}