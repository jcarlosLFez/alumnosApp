namespace Alumnos.Api.Entities;

public class Correo
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public int AlumnoId { get; set; }
    public Alumno Alumno { get; set; } = null!;
}
