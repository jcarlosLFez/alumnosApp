namespace Alumnos.Api.Entities;

public class Alumno
{
    public int Id { get; set; }
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public int TipoDocumentoId { get; set; }
    public TipoDocumento TipoDocumento { get; set; } = null!;
    public string NumeroDocumento { get; set; } = null!;
    public int PaisId { get; set; }
    public Pais Pais { get; set; } = null!;
    public int RegionId { get; set; }
    public Region Region { get; set; } = null!;
    public DateOnly FechaNacimiento { get; set; }

    public List<Correo> Correos { get; set; } = new();
    public List<Telefono> Telefonos { get; set; } = new();
}
