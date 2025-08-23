namespace Alumnos.Api.Entities;

public class Telefono
{
    public int Id { get; set; }
    public string Numero { get; set; } = null!;
    public int AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;
}
