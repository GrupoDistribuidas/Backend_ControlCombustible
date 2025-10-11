namespace MS.Vehiculos.Application.DTOs
{
    public class CrearVehiculoDto
    {
        public string Nombre { get; set; } = null!;
        public string Placa { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public int TipoMaquinariaId { get; set; }
        public string Disponible { get; set; } = "Disponible";
        public decimal ConsumoCombustibleKm { get; set; }
        public decimal CapacidadCombustible { get; set; }
    }

    public class ActualizarVehiculoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Placa { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public int TipoMaquinariaId { get; set; }
        public string Disponible { get; set; } = "Disponible";
        public decimal ConsumoCombustibleKm { get; set; }
        public decimal CapacidadCombustible { get; set; }
        public bool? Estado { get; set; } = null;
    }
}
