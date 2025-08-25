using Alumnos.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Alumnos.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Correo> Correos => Set<Correo>();
    public DbSet<Telefono> Telefonos => Set<Telefono>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Region> Regiones => Set<Region>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alumno>(e =>
        {
            e.Property(p => p.Nombres).IsRequired().HasMaxLength(100);
            e.Property(p => p.Apellidos).IsRequired().HasMaxLength(100);
            e.Property(p => p.NumeroDocumento).IsRequired().HasMaxLength(30);
            e.HasOne(x => x.TipoDocumento).WithMany(x => x.Alumnos)
                .HasForeignKey(x => x.TipoDocumentoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Pais).WithMany()
                .HasForeignKey(x => x.PaisId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Region).WithMany(r => r.Alumnos)
                .HasForeignKey(x => x.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Correos).WithOne(c => c.Alumno)
                .HasForeignKey(c => c.AlumnoId);
            e.HasMany(x => x.Telefonos).WithOne(t => t.Alumno)
                .HasForeignKey(t => t.AlumnoId);
        });

        modelBuilder.Entity<Correo>(e =>
        {
            e.Property(p => p.Email).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Telefono>(e =>
        {
            e.Property(p => p.Numero).IsRequired().HasMaxLength(30);
        });

        modelBuilder.Entity<TipoDocumento>().HasData(
            new TipoDocumento { Id = 1, Nombre = "DNI" },
            new TipoDocumento { Id = 2, Nombre = "CE" },
            new TipoDocumento { Id = 3, Nombre = "Pasaporte" }
        );

        modelBuilder.Entity<Pais>().HasData(
            new Pais { Id = 1, Nombre = "Perú" },
            new Pais { Id = 2, Nombre = "Chile" }
        );

        modelBuilder.Entity<Region>().HasData(
            new Region { Id = 1, Nombre = "Arequipa", PaisId = 1 },
            new Region { Id = 2, Nombre = "Lima", PaisId = 1 },
            new Region { Id = 3, Nombre = "Santiago", PaisId = 2 }
        );
    }
}
