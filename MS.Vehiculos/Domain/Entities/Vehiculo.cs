using System;

namespace MS.Vehiculos.Domain.Entities
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Placa { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public int TipoMaquinariaId { get; set; }
        public string Disponible { get; set; } = "Disponible"; // Enum-like values: Disponible, En mantenimiento, No Disponible
        public decimal ConsumoCombustibleKm { get; set; }
        public decimal CapacidadCombustible { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
