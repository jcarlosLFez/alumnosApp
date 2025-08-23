namespace Alumnos.Api.Entities;

public class Pais
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public List<Region> Regiones { get; set; } = new();
}

public class Region
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int PaisId { get; set; }
    public Pais Pais { get; set; } = null!;
    public List<Alumno> Alumnos { get; set; } = new();
}