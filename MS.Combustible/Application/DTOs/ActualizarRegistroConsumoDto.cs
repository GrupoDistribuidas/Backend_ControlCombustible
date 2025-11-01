using System.ComponentModel.DataAnnotations;

namespace MS.Combustible.Application.DTOs
{
    public class ActualizarRegistroConsumoDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        public DateTime FechaRegistro { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El combustible estimado debe ser mayor o igual a 0")]
        public double CombustibleEstimado { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El combustible real debe ser mayor o igual a 0")]
        public double CombustibleReal { get; set; }
        
        public string Motivo { get; set; } = string.Empty;
        
        public int? EstadoId { get; set; }
    }
}