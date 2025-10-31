using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Combustible.Protos;
using MS.Combustible.Application.Services;
using AppDtos = MS.Combustible.Application.DTOs;

namespace MS.Combustible.Services
{
    public class AsignacionesGrpcService : AsignacionesService.AsignacionesServiceBase
    {
        private readonly AsignacionRutaService _asignacionService;

        public AsignacionesGrpcService(AsignacionRutaService asignacionService)
        {
            _asignacionService = asignacionService;
        }

        public override async Task<CrearAsignacionResponse> CrearAsignacion(CrearAsignacionRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.CrearAsignacionRutaDto
            {
                ChoferId = request.ChoferId,
                VehiculoId = request.VehiculoId,
                RutaId = request.RutaId,
                FechaAsignacion = DateTime.Parse(request.FechaAsignacion)
            };

            try
            {
                var id = await _asignacionService.CrearAsignacionAsync(dto);
                return new CrearAsignacionResponse { Id = id };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al crear asignación: " + ex.Message));
            }
        }

        public override async Task ListarAsignaciones(Empty request, IServerStreamWriter<AsignacionDto> responseStream, ServerCallContext context)
        {
            var list = await _asignacionService.GetAllAsync();
            foreach (var a in list)
            {
                await responseStream.WriteAsync(new AsignacionDto
                {
                    Id = a.Id,
                    ChoferId = a.ChoferId,
                    VehiculoId = a.VehiculoId,
                    RutaId = a.RutaId,
                    FechaAsignacion = a.FechaAsignacion.ToString("yyyy-MM-dd"),
                    CombustibleEstimado = a.CombustibleEstimado,
                    EstadoId = a.EstadoId
                });
            }
        }

        public override async Task<AsignacionDto> GetById(GetByIdRequest request, ServerCallContext context)
        {
            try
            {
                var asignacion = await _asignacionService.GetByIdAsync(request.Id);
                if (asignacion == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Asignación no encontrada"));
                }
                
                return new AsignacionDto
                {
                    Id = asignacion.Id,
                    ChoferId = asignacion.ChoferId,
                    VehiculoId = asignacion.VehiculoId,
                    RutaId = asignacion.RutaId,
                    FechaAsignacion = asignacion.FechaAsignacion.ToString("yyyy-MM-dd"),
                    CombustibleEstimado = asignacion.CombustibleEstimado,
                    EstadoId = asignacion.EstadoId
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar asignación"));
            }
        }

        public override async Task<ActualizarAsignacionResponse> ActualizarAsignacion(ActualizarAsignacionRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.ActualizarAsignacionRutaDto
            {
                Id = request.Id,
                ChoferId = request.ChoferId,
                VehiculoId = request.VehiculoId,
                RutaId = request.RutaId,
                FechaAsignacion = DateTime.Parse(request.FechaAsignacion),
                EstadoId = request.EstadoId
            };

            try
            {
                var affected = await _asignacionService.ActualizarAsignacionAsync(dto);
                return new ActualizarAsignacionResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar asignación"));
            }
        }

        public override async Task<CambiarEstadoResponse> CambiarEstado(CambiarEstadoRequest request, ServerCallContext context)
        {
            try
            {
                var affected = await _asignacionService.CambiarEstadoAsync(request.Id, request.NuevoEstado);
                return new CambiarEstadoResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al cambiar estado"));
            }
        }

        public override async Task GetByChoferId(GetByChoferIdRequest request, IServerStreamWriter<AsignacionDto> responseStream, ServerCallContext context)
        {
            var list = await _asignacionService.GetByChoferIdAsync(request.ChoferId);
            foreach (var a in list)
            {
                await responseStream.WriteAsync(new AsignacionDto
                {
                    Id = a.Id,
                    ChoferId = a.ChoferId,
                    VehiculoId = a.VehiculoId,
                    RutaId = a.RutaId,
                    FechaAsignacion = a.FechaAsignacion.ToString("yyyy-MM-dd"),
                    CombustibleEstimado = a.CombustibleEstimado,
                    EstadoId = a.EstadoId
                });
            }
        }

        public override async Task GetByVehiculoId(GetByVehiculoIdRequest request, IServerStreamWriter<AsignacionDto> responseStream, ServerCallContext context)
        {
            var list = await _asignacionService.GetByVehiculoIdAsync(request.VehiculoId);
            foreach (var a in list)
            {
                await responseStream.WriteAsync(new AsignacionDto
                {
                    Id = a.Id,
                    ChoferId = a.ChoferId,
                    VehiculoId = a.VehiculoId,
                    RutaId = a.RutaId,
                    FechaAsignacion = a.FechaAsignacion.ToString("yyyy-MM-dd"),
                    CombustibleEstimado = a.CombustibleEstimado,
                    EstadoId = a.EstadoId
                });
            }
        }

        public override async Task GetByEstado(GetByEstadoRequest request, IServerStreamWriter<AsignacionDto> responseStream, ServerCallContext context)
        {
            try
            {
                var list = await _asignacionService.GetByEstadoAsync(request.EstadoNombre);
                foreach (var a in list)
                {
                    await responseStream.WriteAsync(new AsignacionDto
                    {
                        Id = a.Id,
                        ChoferId = a.ChoferId,
                        VehiculoId = a.VehiculoId,
                        RutaId = a.RutaId,
                        FechaAsignacion = a.FechaAsignacion.ToString("yyyy-MM-dd"),
                        CombustibleEstimado = a.CombustibleEstimado,
                        EstadoId = a.EstadoId
                    });
                }
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
        }
    }
}
