namespace Alumnos.Api.Dtos;

public record CorreoDto(string Email);
public record TelefonoDto(string Numero);

public record AlumnoCreateUpdateDto(
    string Nombres,
    string Apellidos,
    int TipoDocumentoId,
    string NumeroDocumento,
    int PaisId,
    int RegionId,
    DateOnly FechaNacimiento,
    List<CorreoDto> Correos,
    List<TelefonoDto> Telefonos
);

public record AlumnoListItemDto(
    int Id,
    string Nombres,
    string Apellidos,
    string TipoDocumento,
    string NumeroDocumento,
    string Pais,
    string Region,
    DateOnly FechaNacimiento,
    List<string> Correos,
    List<string> Telefonos
);
