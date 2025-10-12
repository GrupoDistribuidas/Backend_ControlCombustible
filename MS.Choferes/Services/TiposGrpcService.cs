using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Choferes.Protos;
using MS.Choferes.Application.Services;
using AppDtos = MS.Choferes.Application.DTOs;

namespace MS.Choferes.Services
{
    public class TiposGrpcService : TiposService.TiposServiceBase
    {
        private readonly TipoMaquinariaService _tipoService;

        public TiposGrpcService(TipoMaquinariaService tipoService)
        {
            _tipoService = tipoService;
        }

        public override async Task ListarTipos(Empty request, IServerStreamWriter<MS.Choferes.Protos.TipoMaquinariaDto> responseStream, ServerCallContext context)
        {
            var list = await _tipoService.GetAllAsync();
            foreach (var t in list)
            {
                await responseStream.WriteAsync(new MS.Choferes.Protos.TipoMaquinariaDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion ?? string.Empty
                });
            }
        }
    }
}
