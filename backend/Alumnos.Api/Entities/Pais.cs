namespace Alumnos.Api.Entities;

public class Pais
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public List<Region> Regiones { get; set; } = new();
}

