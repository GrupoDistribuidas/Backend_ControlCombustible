using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Choferes.Protos;
using MS.Choferes.Application.Services;
using AppDtos = MS.Choferes.Application.DTOs;

namespace MS.Choferes.Services
{
    public class ChoferesGrpcService : ChoferesService.ChoferesServiceBase
    {
        private readonly ChoferService _choferService;

        public ChoferesGrpcService(ChoferService choferService)
        {
            _choferService = choferService;
        }

        public override async Task<CrearChoferResponse> CrearChofer(CrearChoferRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.CrearChoferDto
            {
                PrimerNombre = request.PrimerNombre,
                SegundoNombre = request.SegundoNombre,
                PrimerApellido = request.PrimerApellido,
                SegundoApellido = request.SegundoApellido,
                Identificacion = request.Identificacion,
                FechaNacimiento = DateTime.Parse(request.FechaNacimiento),
                Disponible = request.Disponible,
                UsuarioId = request.UsuarioId,
                TipoMaquinariaId = request.TipoMaquinariaId
            };

            try
            {
                var id = await _choferService.CrearChoferAsync(dto);
                return new CrearChoferResponse { Id = id };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al crear chofer"));
            }
        }

        public override async Task ListarChoferes(Empty request, IServerStreamWriter<MS.Choferes.Protos.ChoferDto> responseStream, ServerCallContext context)
        {
            var list = await _choferService.GetAllAsync();
            foreach (var c in list)
            {
                await responseStream.WriteAsync(new MS.Choferes.Protos.ChoferDto
                {
                    Id = c.Id,
                    PrimerNombre = c.PrimerNombre,
                    SegundoNombre = c.SegundoNombre ?? string.Empty,
                    PrimerApellido = c.PrimerApellido,
                    SegundoApellido = c.SegundoApellido ?? string.Empty,
                    NombreCompleto = c.NombreCompleto,
                    Identificacion = c.Identificacion,
                    FechaNacimiento = c.FechaNacimiento.ToString("yyyy-MM-dd"),
                    Disponible = c.Disponible,
                    UsuarioId = c.UsuarioId,
                    TipoMaquinariaId = c.TipoMaquinariaId
                });
            }
        }

        public override async Task<ActualizarChoferResponse> ActualizarChofer(ActualizarChoferRequest request, ServerCallContext context)
        {
            var dto = new AppDtos.ActualizarChoferDto
            {
                Id = request.Id,
                PrimerNombre = request.PrimerNombre,
                SegundoNombre = request.SegundoNombre,
                PrimerApellido = request.PrimerApellido,
                SegundoApellido = request.SegundoApellido,
                Identificacion = request.Identificacion,
                FechaNacimiento = DateTime.Parse(request.FechaNacimiento),
                Disponible = request.Disponible,
                UsuarioId = request.UsuarioId,
                TipoMaquinariaId = request.TipoMaquinariaId,
                Estado = request.Estado != null ? (bool?)request.Estado.Value : null
            };

            try
            {
                var affected = await _choferService.ActualizarChoferAsync(dto);
                return new ActualizarChoferResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al actualizar chofer"));
            }
        }

        public override async Task Search(SearchChoferRequest request, IServerStreamWriter<MS.Choferes.Protos.ChoferDto> responseStream, ServerCallContext context)
        {
            var filter = new AppDtos.ChoferFilterDto
            {
                Estado = request.Estado != null ? (bool?)request.Estado.Value : null,
                TipoMaquinariaId = request.TipoMaquinariaId != null ? (int?)request.TipoMaquinariaId.Value : null,
                Disponible = request.Disponible != null ? (bool?)request.Disponible.Value : null,
                FechaNacimientoDesde = string.IsNullOrWhiteSpace(request.FechaNacimientoDesde) ? (DateTime?)null : DateTime.Parse(request.FechaNacimientoDesde),
                FechaNacimientoHasta = string.IsNullOrWhiteSpace(request.FechaNacimientoHasta) ? (DateTime?)null : DateTime.Parse(request.FechaNacimientoHasta)
            };

            try
            {
                var list = await _choferService.SearchAsync(filter);
                foreach (var c in list)
                {
                    await responseStream.WriteAsync(new MS.Choferes.Protos.ChoferDto
                    {
                        Id = c.Id,
                        PrimerNombre = c.PrimerNombre,
                        SegundoNombre = c.SegundoNombre ?? string.Empty,
                        PrimerApellido = c.PrimerApellido,
                        SegundoApellido = c.SegundoApellido ?? string.Empty,
                        NombreCompleto = c.NombreCompleto,
                        Identificacion = c.Identificacion,
                        FechaNacimiento = c.FechaNacimiento.ToString("yyyy-MM-dd"),
                        Disponible = c.Disponible,
                        UsuarioId = c.UsuarioId,
                        TipoMaquinariaId = c.TipoMaquinariaId
                    });
                }
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar choferes"));
            }
        }

        public override async Task SearchByTerm(SearchByTermRequest request, IServerStreamWriter<MS.Choferes.Protos.ChoferDto> responseStream, ServerCallContext context)
        {
            try
            {
                var list = await _choferService.SearchByTermAsync(request.Term);
                foreach (var c in list)
                {
                    await responseStream.WriteAsync(new MS.Choferes.Protos.ChoferDto
                    {
                        Id = c.Id,
                        PrimerNombre = c.PrimerNombre,
                        SegundoNombre = c.SegundoNombre ?? string.Empty,
                        PrimerApellido = c.PrimerApellido,
                        SegundoApellido = c.SegundoApellido ?? string.Empty,
                        NombreCompleto = c.NombreCompleto,
                        Identificacion = c.Identificacion,
                        FechaNacimiento = c.FechaNacimiento.ToString("yyyy-MM-dd"),
                        Disponible = c.Disponible,
                        UsuarioId = c.UsuarioId,
                        TipoMaquinariaId = c.TipoMaquinariaId
                    });
                }
            }
            catch (Exception)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Error interno al buscar por término"));
            }
        }

        public override async Task<ActualizarEstadoResponse> ActualizarEstadoChofer(ActualizarEstadoRequest request, ServerCallContext context)
        {
            try
            {
                if (request.Estado == null) throw new ArgumentException("Estado es requerido");
                var affected = await _choferService.ActualizarEstadoAsync(request.Id, request.Estado.Value);
                return new ActualizarEstadoResponse { Affected = affected };
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

        public override async Task<ActualizarDisponibilidadResponse> ActualizarDisponibilidadChofer(ActualizarDisponibilidadRequest request, ServerCallContext context)
        {
            try
            {
                if (request.Disponible == null) throw new ArgumentException("Disponible es requerido");
                var affected = await _choferService.ActualizarDisponibilidadAsync(request.Id, request.Disponible.Value);
                return new ActualizarDisponibilidadResponse { Affected = affected };
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

        public override async Task<AsignarUsuarioResponse> AsignarUsuario(AsignarUsuarioRequest request, ServerCallContext context)
        {
            try
            {
                if (request.ChoferId <= 0) throw new ArgumentException("ChoferId inválido");
                if (request.UsuarioId <= 0) throw new ArgumentException("UsuarioId inválido");
                
                var affected = await _choferService.AsignarUsuarioAsync(request.ChoferId, request.UsuarioId);
                return new AsignarUsuarioResponse { Affected = affected };
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Error interno al asignar usuario: {ex.Message}"));
            }
        }
    }
}
