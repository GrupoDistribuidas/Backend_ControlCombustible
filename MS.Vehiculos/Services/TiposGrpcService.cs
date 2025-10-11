using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MS.Vehiculos.Protos;
using MS.Vehiculos.Application.Services;

namespace MS.Vehiculos.Services
{
    public class TiposGrpcService : TiposService.TiposServiceBase
    {
        private readonly TipoMaquinariaService _tipoService;

        public TiposGrpcService(TipoMaquinariaService tipoService)
        {
            _tipoService = tipoService;
        }

        public override async Task ListarTipos(Empty request, IServerStreamWriter<TipoMaquinariaDto> responseStream, ServerCallContext context)
        {
            var list = await _tipoService.GetAllAsync();
            foreach (var t in list)
            {
                await responseStream.WriteAsync(new TipoMaquinariaDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion ?? string.Empty
                });
            }
        }
    }
}
