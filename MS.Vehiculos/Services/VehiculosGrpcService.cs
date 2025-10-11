using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Vehiculos.Protos;
using MS.Vehiculos.Application.Services;
using AppDtos = MS.Vehiculos.Application.DTOs;

namespace MS.Vehiculos.Services
{
    public class VehiculosGrpcService : VehiculosService.VehiculosServiceBase
    {
        private readonly VehiculoService _vehiculoService;

        public VehiculosGrpcService(VehiculoService vehiculoService)
        {
            _vehiculoService = vehiculoService;
        }

        public override async Task<CrearVehiculoResponse> CrearVehiculo(CrearVehiculoRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.CrearVehiculoDto
            {
                Nombre = request.Nombre,
                Placa = request.Placa,
                Marca = request.Marca,
                Modelo = request.Modelo,
                TipoMaquinariaId = request.TipoMaquinariaId,
                Disponible = request.Disponible,
                ConsumoCombustibleKm = (decimal)request.ConsumoCombustibleKm,
                CapacidadCombustible = (decimal)request.CapacidadCombustible
            };

            try
            {
                var id = await _vehiculoService.CrearVehiculoAsync(dto);
                return new CrearVehiculoResponse { Id = id };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                // unexpected
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al crear vehículo"));
            }
        }

        public override async Task ListarVehiculos(Empty request, IServerStreamWriter<MS.Vehiculos.Protos.VehiculoDto> responseStream, ServerCallContext context)
        {
            var list = await _vehiculoService.GetAllAsync();
            foreach (var v in list)
            {
                await responseStream.WriteAsync(new MS.Vehiculos.Protos.VehiculoDto
                {
                    Id = v.Id,
                    Nombre = v.Nombre,
                    Placa = v.Placa,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    TipoMaquinariaId = v.TipoMaquinariaId,
                    Disponible = v.Disponible,
                    ConsumoCombustibleKm = (double)v.ConsumoCombustibleKm,
                    CapacidadCombustible = (double)v.CapacidadCombustible
                });
            }
        }
    }
}
