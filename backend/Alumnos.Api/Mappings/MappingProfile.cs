using Alumnos.Api.Dtos;
using Alumnos.Api.Entities;
using AutoMapper;

namespace Alumnos.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Alumno, AlumnoListItemDto>()
            .ForMember(d => d.TipoDocumento, m => m.MapFrom(s => s.TipoDocumento.Nombre))
            .ForMember(d => d.Pais, m => m.MapFrom(s => s.Pais.Nombre))
            .ForMember(d => d.Region, m => m.MapFrom(s => s.Region.Nombre))
            .ForMember(d => d.Correos, m => m.MapFrom(s => s.Correos.Select(c => c.Email).ToList()))
            .ForMember(d => d.Telefonos, m => m.MapFrom(s => s.Telefonos.Select(t => t.Numero).ToList()));

        CreateMap<AlumnoCreateUpdateDto, Alumno>()
            .ForMember(d => d.Correos, m => m.MapFrom(s => s.Correos.Select(x => new Correo { Email = x.Email })))
            .ForMember(d => d.Telefonos, m => m.MapFrom(s => s.Telefonos.Select(x => new Telefono { Numero = x.Numero })));
    }
}
