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

        public override async Task<MS.Vehiculos.Protos.VehiculoDto> GetById(MS.Vehiculos.Protos.GetByIdRequest request, ServerCallContext context)
        {
            try
            {
                var vehiculo = await _vehiculoService.GetByIdAsync(request.Id);
                if (vehiculo == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Vehículo con ID {request.Id} no encontrado"));
                }

                return new MS.Vehiculos.Protos.VehiculoDto
                {
                    Id = vehiculo.Id,
                    Nombre = vehiculo.Nombre,
                    Placa = vehiculo.Placa,
                    Marca = vehiculo.Marca,
                    Modelo = vehiculo.Modelo,
                    TipoMaquinariaId = vehiculo.TipoMaquinariaId,
                    Disponible = vehiculo.Disponible,
                    ConsumoCombustibleKm = (double)vehiculo.ConsumoCombustibleKm,
                    CapacidadCombustible = (double)vehiculo.CapacidadCombustible
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar vehículo"));
            }
        }

        public override async Task<MS.Vehiculos.Protos.ActualizarVehiculoResponse> ActualizarVehiculo(MS.Vehiculos.Protos.ActualizarVehiculoRequest request, ServerCallContext context)
        {
                var dto = new AppDtos.ActualizarVehiculoDto
            {
                Id = request.Id,
                Nombre = request.Nombre,
                Placa = request.Placa,
                Marca = request.Marca,
                Modelo = request.Modelo,
                TipoMaquinariaId = request.TipoMaquinariaId,
                Disponible = request.Disponible,
                ConsumoCombustibleKm = (decimal)request.ConsumoCombustibleKm,
                CapacidadCombustible = (decimal)request.CapacidadCombustible,
                    Estado = request.Estado != null ? (bool?)request.Estado.Value : null
            };

            try
            {
                var affected = await _vehiculoService.ActualizarVehiculoAsync(dto);
                return new MS.Vehiculos.Protos.ActualizarVehiculoResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar vehículo"));
            }
        }

        public override async Task<MS.Vehiculos.Protos.ActualizarEstadoResponse> ActualizarEstadoVehiculo(MS.Vehiculos.Protos.ActualizarEstadoRequest request, ServerCallContext context)
        {
            try
            {
                if (request.Estado == null) throw new ArgumentException("Estado es requerido");
                var affected = await _vehiculoService.ActualizarEstadoAsync(request.Id, request.Estado.Value);
                return new MS.Vehiculos.Protos.ActualizarEstadoResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar estado"));
            }
        }

        public override async Task<MS.Vehiculos.Protos.ActualizarDisponibilidadResponse> ActualizarDisponibilidad(MS.Vehiculos.Protos.ActualizarDisponibilidadRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Disponible)) throw new ArgumentException("Disponible es requerido");
                var affected = await _vehiculoService.ActualizarDisponibilidadAsync(request.Id, request.Disponible);
                return new MS.Vehiculos.Protos.ActualizarDisponibilidadResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar disponibilidad"));
            }
        }

        public override async Task<MS.Vehiculos.Protos.ExistsByPlacaResponse> ExistsByPlaca(MS.Vehiculos.Protos.ExistsByPlacaRequest request, ServerCallContext context)
        {
            try
            {
                var exists = await _vehiculoService.ExistsByPlacaAsync(request.Placa);
                return new MS.Vehiculos.Protos.ExistsByPlacaResponse { Exists = exists };
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al verificar placa"));
            }
        }

        public override async Task Search(MS.Vehiculos.Protos.SearchRequest request, IServerStreamWriter<MS.Vehiculos.Protos.VehiculoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var filter = new AppDtos.VehiculoFilterDto
                {
                    Estado = request.Estado != null ? (bool?)request.Estado.Value : null,
                    TipoMaquinariaId = request.TipoMaquinariaId != null ? (int?)request.TipoMaquinariaId.Value : null,
                    Marca = string.IsNullOrWhiteSpace(request.Marca) ? null : request.Marca,
                    Modelo = string.IsNullOrWhiteSpace(request.Modelo) ? null : request.Modelo,
                    CapacidadMin = request.CapacidadMin != null ? (decimal?)request.CapacidadMin.Value : null,
                    CapacidadMax = request.CapacidadMax != null ? (decimal?)request.CapacidadMax.Value : null,
                    ConsumoMin = request.ConsumoMin != null ? (decimal?)request.ConsumoMin.Value : null,
                    ConsumoMax = request.ConsumoMax != null ? (decimal?)request.ConsumoMax.Value : null,
                    Disponible = string.IsNullOrWhiteSpace(request.Disponible) ? null : request.Disponible
                };

                var list = await _vehiculoService.SearchAsync(filter);
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
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar vehículos"));
            }
        }

        public override async Task SearchByTerm(MS.Vehiculos.Protos.SearchByTermRequest request, IServerStreamWriter<MS.Vehiculos.Protos.VehiculoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var list = await _vehiculoService.SearchByTermAsync(request.Term);
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
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar por término"));
            }
        }
    }
}
