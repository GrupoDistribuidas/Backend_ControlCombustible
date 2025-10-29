using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Rutas.Protos;
using MS.Rutas.Application.Services;
using AppDtos = MS.Rutas.Application.DTOs;

namespace MS.Rutas.Services
{
    public class PuntosGrpcService : PuntosService.PuntosServiceBase
    {
        private readonly PuntoService _puntoService;

        public PuntosGrpcService(PuntoService puntoService)
        {
            _puntoService = puntoService;
        }

        public override async Task<CrearPuntoResponse> CrearPunto(CrearPuntoRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.PuntoDto
            {
                Nombre = request.Nombre,
                Direccion = request.Direccion,
                Provincia = request.Provincia,
                TipoPunto = request.TipoPunto
            };

            try
            {
                var id = await _puntoService.CrearPuntoAsync(dto);
                return new CrearPuntoResponse { Id = id };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al crear punto"));
            }
        }

        public override async Task ListarPuntos(Empty request, IServerStreamWriter<MS.Rutas.Protos.PuntoDto> responseStream, ServerCallContext context)
        {
            var list = await _puntoService.GetAllAsync();
            foreach (var p in list)
            {
                await responseStream.WriteAsync(new MS.Rutas.Protos.PuntoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Direccion = p.Direccion,
                    Provincia = p.Provincia,
                    TipoPunto = p.TipoPunto
                });
            }
        }

        public override async Task<MS.Rutas.Protos.PuntoDto> GetById(MS.Rutas.Protos.GetPuntoByIdRequest request, ServerCallContext context)
        {
            try
            {
                var punto = await _puntoService.GetByIdAsync(request.Id);
                if (punto == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Punto no encontrado"));
                }
                return new MS.Rutas.Protos.PuntoDto
                {
                    Id = punto.Id,
                    Nombre = punto.Nombre,
                    Direccion = punto.Direccion,
                    Provincia = punto.Provincia,
                    TipoPunto = punto.TipoPunto
                };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar punto"));
            }
        }

        public override async Task<ActualizarPuntoResponse> ActualizarPunto(ActualizarPuntoRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.PuntoDto
            {
                Id = request.Id,
                Nombre = request.Nombre,
                Direccion = request.Direccion,
                Provincia = request.Provincia,
                TipoPunto = request.TipoPunto
            };

            try
            {
                var affected = await _puntoService.ActualizarPuntoAsync(dto);
                return new ActualizarPuntoResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar punto"));
            }
        }

        public override async Task SearchByTerm(MS.Rutas.Protos.SearchPuntoByTermRequest request, IServerStreamWriter<MS.Rutas.Protos.PuntoDto> responseStream, ServerCallContext context)
        {
            try
            {
                var list = await _puntoService.SearchByTermAsync(request.Term);
                foreach (var p in list)
                {
                    await responseStream.WriteAsync(new MS.Rutas.Protos.PuntoDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Direccion = p.Direccion,
                        Provincia = p.Provincia,
                        TipoPunto = p.TipoPunto
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
