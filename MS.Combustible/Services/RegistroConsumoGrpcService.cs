using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Combustible.Protos;
using MS.Combustible.Application.Services;
using AppDtos = MS.Combustible.Application.DTOs;

namespace MS.Combustible.Services
{
    public class RegistroConsumoGrpcService : Protos.RegistroConsumoService.RegistroConsumoServiceBase
    {
        private readonly Application.Services.RegistroConsumoService _registroService;

        public RegistroConsumoGrpcService(Application.Services.RegistroConsumoService registroService)
        {
            _registroService = registroService;
        }

        public override async Task<CrearRegistroConsumoResponse> CrearRegistroConsumo(CrearRegistroConsumoRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.CrearRegistroConsumoDto
            {
                AsignacionRutaId = request.AsignacionRutaId,
                FechaRegistro = request.FechaRegistro.ToDateTime(),
                CombustibleEstimado = request.CombustibleEstimado,
                CombustibleReal = request.CombustibleReal,
                Motivo = request.Motivo,
                EstadoId = request.EstadoId
            };

            try
            {
                var id = await _registroService.CrearRegistroConsumoAsync(dto);
                return new CrearRegistroConsumoResponse { Id = id };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al crear registro de consumo: " + ex.Message));
            }
        }

        public override async Task<ActualizarRegistroConsumoResponse> ActualizarRegistroConsumo(ActualizarRegistroConsumoRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.ActualizarRegistroConsumoDto
            {
                Id = request.Id,
                FechaRegistro = request.FechaRegistro.ToDateTime(),
                CombustibleEstimado = request.CombustibleEstimado,
                CombustibleReal = request.CombustibleReal,
                Motivo = request.Motivo,
                EstadoId = request.EstadoId > 0 ? request.EstadoId : null
            };

            try
            {
                var affected = await _registroService.ActualizarRegistroConsumoAsync(dto);
                return new ActualizarRegistroConsumoResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar registro de consumo: " + ex.Message));
            }
        }

        public override async Task<RegistroConsumoDto> GetById(GetRegistroConsumoByIdRequest request, ServerCallContext context)
        {
            try
            {
                var registro = await _registroService.GetByIdAsync(request.Id);
                if (registro == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Registro de consumo con ID {request.Id} no encontrado"));
                }

                return new RegistroConsumoDto
                {
                    Id = registro.Id,
                    AsignacionRutaId = registro.AsignacionRutaId,
                    FechaRegistro = Timestamp.FromDateTime(registro.FechaRegistro.ToUniversalTime()),
                    CombustibleEstimado = registro.CombustibleEstimado,
                    CombustibleReal = registro.CombustibleReal,
                    Motivo = registro.Motivo,
                    EstadoId = registro.EstadoId,
                    EstadoNombre = registro.EstadoNombre,
                    FechaCreacion = Timestamp.FromDateTime(registro.FechaCreacion.ToUniversalTime()),
                    FechaModificacion = Timestamp.FromDateTime(registro.FechaModificacion.ToUniversalTime())
                };
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al obtener registro de consumo: " + ex.Message));
            }
        }

        public override async Task ListarTodos(Empty request, IServerStreamWriter<RegistroConsumoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var registros = await _registroService.GetAllAsync();
                foreach (var registro in registros)
                {
                    await responseStream.WriteAsync(new RegistroConsumoDto
                    {
                        Id = registro.Id,
                        AsignacionRutaId = registro.AsignacionRutaId,
                        FechaRegistro = Timestamp.FromDateTime(registro.FechaRegistro.ToUniversalTime()),
                        CombustibleEstimado = registro.CombustibleEstimado,
                        CombustibleReal = registro.CombustibleReal,
                        Motivo = registro.Motivo,
                        EstadoId = registro.EstadoId,
                        EstadoNombre = registro.EstadoNombre,
                        FechaCreacion = Timestamp.FromDateTime(registro.FechaCreacion.ToUniversalTime()),
                        FechaModificacion = Timestamp.FromDateTime(registro.FechaModificacion.ToUniversalTime())
                    });
                }
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al listar registros de consumo: " + ex.Message));
            }
        }

        public override async Task GetByAsignacionRutaId(GetRegistroConsumoByAsignacionRequest request, IServerStreamWriter<RegistroConsumoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var registros = await _registroService.GetByAsignacionRutaIdAsync(request.AsignacionRutaId);
                foreach (var registro in registros)
                {
                    await responseStream.WriteAsync(new RegistroConsumoDto
                    {
                        Id = registro.Id,
                        AsignacionRutaId = registro.AsignacionRutaId,
                        FechaRegistro = Timestamp.FromDateTime(registro.FechaRegistro.ToUniversalTime()),
                        CombustibleEstimado = registro.CombustibleEstimado,
                        CombustibleReal = registro.CombustibleReal,
                        Motivo = registro.Motivo,
                        EstadoId = registro.EstadoId,
                        EstadoNombre = registro.EstadoNombre,
                        FechaCreacion = Timestamp.FromDateTime(registro.FechaCreacion.ToUniversalTime()),
                        FechaModificacion = Timestamp.FromDateTime(registro.FechaModificacion.ToUniversalTime())
                    });
                }
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al obtener registros por asignación: " + ex.Message));
            }
        }

        public override async Task GetByEstadoId(GetRegistroConsumoByEstadoRequest request, IServerStreamWriter<RegistroConsumoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var registros = await _registroService.GetByEstadoAsync(request.EstadoId);
                foreach (var registro in registros)
                {
                    await responseStream.WriteAsync(new RegistroConsumoDto
                    {
                        Id = registro.Id,
                        AsignacionRutaId = registro.AsignacionRutaId,
                        FechaRegistro = Timestamp.FromDateTime(registro.FechaRegistro.ToUniversalTime()),
                        CombustibleEstimado = registro.CombustibleEstimado,
                        CombustibleReal = registro.CombustibleReal,
                        Motivo = registro.Motivo,
                        EstadoId = registro.EstadoId,
                        EstadoNombre = registro.EstadoNombre,
                        FechaCreacion = Timestamp.FromDateTime(registro.FechaCreacion.ToUniversalTime()),
                        FechaModificacion = Timestamp.FromDateTime(registro.FechaModificacion.ToUniversalTime())
                    });
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al obtener registros por estado: " + ex.Message));
            }
        }

        public override async Task GetByFechaRango(GetRegistroConsumoByFechaRangoRequest request, IServerStreamWriter<RegistroConsumoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var fechaInicio = request.FechaInicio.ToDateTime();
                var fechaFin = request.FechaFin.ToDateTime();
                
                var registros = await _registroService.GetByFechaRangoAsync(fechaInicio, fechaFin);
                foreach (var registro in registros)
                {
                    await responseStream.WriteAsync(new RegistroConsumoDto
                    {
                        Id = registro.Id,
                        AsignacionRutaId = registro.AsignacionRutaId,
                        FechaRegistro = Timestamp.FromDateTime(registro.FechaRegistro.ToUniversalTime()),
                        CombustibleEstimado = registro.CombustibleEstimado,
                        CombustibleReal = registro.CombustibleReal,
                        Motivo = registro.Motivo,
                        EstadoId = registro.EstadoId,
                        EstadoNombre = registro.EstadoNombre,
                        FechaCreacion = Timestamp.FromDateTime(registro.FechaCreacion.ToUniversalTime()),
                        FechaModificacion = Timestamp.FromDateTime(registro.FechaModificacion.ToUniversalTime())
                    });
                }
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al obtener registros por rango de fechas: " + ex.Message));
            }
        }

        public override async Task<CambiarEstadoRegistroResponse> CambiarEstado(CambiarEstadoRegistroRequest request, ServerCallContext context)
        {
            try
            {
                var affected = await _registroService.CambiarEstadoAsync(request.Id, request.NuevoEstado);
                return new CambiarEstadoRegistroResponse { Affected = affected };
            }
            catch (KeyNotFoundException ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al cambiar estado: " + ex.Message));
            }
        }
    }
}