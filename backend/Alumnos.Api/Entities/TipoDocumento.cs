namespace Alumnos.Api.Entities;

public class TipoDocumento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public List<Alumno> Alumnos { get; set; } = new();
}
