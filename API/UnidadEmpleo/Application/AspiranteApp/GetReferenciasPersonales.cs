using API.Seguridad.Application.Core;
using API.UnidadEmpleo.Domain;
using API.UnidadEmpleo.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.UnidadEmpleo.Application.AspiranteApp;

public class GetReferenciasPersonales
{
    public class Query : IRequest<Result<ReferenciasPersonalesDto>>
    {
        public string Curp { get; set; }
        public string CuerpoId { get; set; }
        public int Perfil { get; set; }
        public int Region { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ReferenciasPersonalesDto>>
    {
        private readonly UnidadEmpleoDBContextFactoryInterface _factory;

        public Handler(UnidadEmpleoDBContextFactoryInterface factory)
        {
            _factory = factory;
        }

        public async Task<Result<ReferenciasPersonalesDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            await using var db = await _factory.CreateAsync();

            IQueryable<Solicitud> consulta = db.Set<Solicitud>()
                .Include(x => x.Referencias)
                .AsNoTracking();

            if (request.Perfil != 8 &&
                request.Perfil != 2 &&
                request.Perfil != 1)
            {
                consulta = consulta.Where(x =>
                    x.Curp == request.Curp &&
                    x.RegionId == request.Region &&
                    x.CorporacionId == request.CuerpoId);
            }
            else
            {
                consulta = consulta.Where(x =>
                    x.Curp == request.Curp);
            }

            var solicitud = await consulta.FirstOrDefaultAsync(cancellationToken);

            if (solicitud == null)
                return Result<ReferenciasPersonalesDto>.Failure(
                    "No se encontró la solicitud.", 404);

            var dto = new ReferenciasPersonalesDto();

            dto.Referencia1 = ObtenerReferencia(
                solicitud.Referencias,
                Parentesco.Padre,
                Parentesco.Madre,
                Parentesco.Esposa,
                Parentesco.Hermano);

            dto.Referencia2 = ObtenerReferencia(
                solicitud.Referencias,
                Parentesco.Tio,
                Parentesco.Primo,
                Parentesco.Sobrino,
                Parentesco.Cuñado,
                Parentesco.Abuelo,
                Parentesco.Suegro);

            dto.Referencia3 = ObtenerReferencia(
                solicitud.Referencias,
                Parentesco.Amigo,
                Parentesco.Vecino,
                Parentesco.Conocido);

            return Result<ReferenciasPersonalesDto>.Success(dto);
        }

        private ReferenciaDto ObtenerReferencia(
            List<Referencia> referencias,
            params Parentesco[] parentescos)
        {
            var r = referencias.FirstOrDefault(x =>
                parentescos.Contains(x.Parentesco));

            if (r == null)
                return new ReferenciaDto();

            return new ReferenciaDto
            {
                ApellidoPaterno = r.Apellido_Paterno,
                ApellidoMaterno = r.Apellido_Materno,
                Nombre = r.Nombre,
                Calle = r.Calle,
                Numero = r.numero,
                EntreCalles = r.EntreCalles,
                Colonia = r.Colonia,
                CodigoPostal = r.CodigoPostal.ToString(),
                Estado = r.Estado,
                Municipio = r.Municipio,
                Parentesco = r.Parentesco.ToString(),
                Telefono = r.TelefonoLocal
            };
        }
    }
}