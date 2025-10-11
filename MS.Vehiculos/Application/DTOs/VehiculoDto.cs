namespace MS.Vehiculos.Application.DTOs
{
    public class VehiculoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Placa { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public int TipoMaquinariaId { get; set; }
        public string Disponible { get; set; } = null!;
        public decimal ConsumoCombustibleKm { get; set; }
        public decimal CapacidadCombustible { get; set; }
    }
}
