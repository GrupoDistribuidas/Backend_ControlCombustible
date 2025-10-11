namespace MS.Vehiculos.Application.DTOs
{
    public class VehiculoFilterDto
    {
        public bool? Estado { get; set; }
        public int? TipoMaquinariaId { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public decimal? CapacidadMin { get; set; }
        public decimal? CapacidadMax { get; set; }
        public decimal? ConsumoMin { get; set; }
        public decimal? ConsumoMax { get; set; }
        public string? Disponible { get; set; }
    }
}
