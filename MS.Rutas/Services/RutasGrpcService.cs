using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Rutas.Protos;
using MS.Rutas.Application.Services;
using AppDtos = MS.Rutas.Application.DTOs;

namespace MS.Rutas.Services
{
    public class RutasGrpcService : RutasService.RutasServiceBase
    {
        private readonly RutaService _rutaService;

        public RutasGrpcService(RutaService rutaService)
        {
            _rutaService = rutaService;
        }

        public override async Task<CrearRutaResponse> CrearRuta(CrearRutaRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.CrearRutaDto
            {
                Nombre = request.Nombre,
                PuntoInicioId = request.PuntoInicioId,
                PuntoFinId = request.PuntoFinId,
                Distancia = request.Distancia
            };

            try
            {
                var id = await _rutaService.CrearRutaAsync(dto);
                return new CrearRutaResponse { Id = id };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al crear ruta"));
            }
        }

        public override async Task ListarRutas(Empty request, IServerStreamWriter<MS.Rutas.Protos.RutaDto> responseStream, ServerCallContext context)
        {
            var list = await _rutaService.GetAllAsync();
            foreach (var r in list)
            {
                await responseStream.WriteAsync(new MS.Rutas.Protos.RutaDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    PuntoInicioId = r.PuntoInicioId,
                    PuntoFinId = r.PuntoFinId,
                    Distancia = r.Distancia,
                    Estado = r.Estado
                });
            }
        }

        public override async Task ListarTodasRutas(Empty request, IServerStreamWriter<MS.Rutas.Protos.RutaDto> responseStream, ServerCallContext context)
        {
            var list = await _rutaService.GetAllIncludingInactiveAsync();
            foreach (var r in list)
            {
                await responseStream.WriteAsync(new MS.Rutas.Protos.RutaDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    PuntoInicioId = r.PuntoInicioId,
                    PuntoFinId = r.PuntoFinId,
                    Distancia = r.Distancia,
                    Estado = r.Estado
                });
            }
        }

        public override async Task<MS.Rutas.Protos.RutaDto> GetById(MS.Rutas.Protos.GetByIdRequest request, ServerCallContext context)
        {
            try
            {
                var ruta = await _rutaService.GetByIdAsync(request.Id);
                if (ruta == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Ruta no encontrada"));
                }
                return new MS.Rutas.Protos.RutaDto
                {
                    Id = ruta.Id,
                    Nombre = ruta.Nombre,
                    PuntoInicioId = ruta.PuntoInicioId,
                    PuntoFinId = ruta.PuntoFinId,
                    Distancia = ruta.Distancia,
                    Estado = ruta.Estado
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar ruta"));
            }
        }

        public override async Task<ActualizarRutaResponse> ActualizarRuta(ActualizarRutaRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.ActualizarRutaDto
            {
                Id = request.Id,
                Nombre = request.Nombre,
                PuntoInicioId = request.PuntoInicioId,
                PuntoFinId = request.PuntoFinId,
                Distancia = request.Distancia,
                Estado = request.Estado != null ? (bool?)request.Estado.Value : null
            };

            try
            {
                var affected = await _rutaService.ActualizarRutaAsync(dto);
                return new ActualizarRutaResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar ruta"));
            }
        }

        public override async Task<MS.Rutas.Protos.ActualizarEstadoResponse> ActualizarEstadoRuta(MS.Rutas.Protos.ActualizarEstadoRequest request, ServerCallContext context)
        {
            try
            {
                var affected = await _rutaService.ActualizarEstadoAsync(request.Id, request.Estado);
                return new MS.Rutas.Protos.ActualizarEstadoResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar estado de ruta"));
            }
        }

        public override async Task SearchByTerm(MS.Rutas.Protos.SearchByTermRequest request, IServerStreamWriter<MS.Rutas.Protos.RutaDto> responseStream, ServerCallContext context)
        {
            try
            {
                var list = await _rutaService.SearchByTermAsync(request.Term);
                foreach (var r in list)
                {
                    await responseStream.WriteAsync(new MS.Rutas.Protos.RutaDto
                    {
                        Id = r.Id,
                        Nombre = r.Nombre,
                        PuntoInicioId = r.PuntoInicioId,
                        PuntoFinId = r.PuntoFinId,
                        Distancia = r.Distancia,
                        Estado = r.Estado
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
