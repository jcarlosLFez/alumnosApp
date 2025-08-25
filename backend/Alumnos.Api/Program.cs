/*var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
*/

using System.Text.RegularExpressions;
using Alumnos.Api.Data;
using Alumnos.Api.Dtos;
using Alumnos.Api.Entities;
using Alumnos.Api.Mappings;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core SQLite
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

app.MapGet("/api/alumnos", async (AppDbContext db, IMapper mapper, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("ui", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("ui");
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok("Alumnos API v1"));

// GET: listado con paginado básico
app.MapGet("/api/alumnos", async ([FromQuery] int page = 1, [FromQuery] int pageSize = 20, AppDbContext db, IMapper mapper) =>
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var query = db.Alumnos
        .Include(a => a.TipoDocumento)
        .Include(a => a.Pais)
        .Include(a => a.Region)
        .Include(a => a.Correos)
        .Include(a => a.Telefonos)
        .AsNoTracking();

    var total = await query.CountAsync();
    var items = await query
        .OrderBy(a => a.Apellidos).ThenBy(a => a.Nombres)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var result = mapper.Map<List<AlumnoListItemDto>>(items);
    return Results.Ok(new { total, page, pageSize, items = result });
})
.WithName("GetAlumnos");

// GET: detalle por id
app.MapGet("/api/alumnos/{id:int}", async (int id, AppDbContext db, IMapper mapper) =>
{
    var alumno = await db.Alumnos
        .Include(a => a.TipoDocumento)
        .Include(a => a.Pais)
        .Include(a => a.Region)
        .Include(a => a.Correos)
        .Include(a => a.Telefonos)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (alumno is null) return Results.NotFound();

    return Results.Ok(mapper.Map<AlumnoListItemDto>(alumno));
})
.WithName("GetAlumnoById");

// POST/PUT (upsert): crear o editar
app.MapPost("/api/alumnos", async ([FromBody] AlumnoCreateUpdateDto dto, AppDbContext db, IMapper mapper) =>
{
    var validation = ValidateAlumnoDto(dto);
    if (validation.Count > 0) return Results.BadRequest(new { errors = validation });

    // Validar FK existentes
    if (!await db.TiposDocumento.AnyAsync(x => x.Id == dto.TipoDocumentoId))
        return Results.BadRequest(new { errors = new[] { "TipoDocumentoId inválido." } });
    if (!await db.Paises.AnyAsync(x => x.Id == dto.PaisId))
        return Results.BadRequest(new { errors = new[] { "PaisId inválido." } });
    if (!await db.Regiones.AnyAsync(x => x.Id == dto.RegionId))
        return Results.BadRequest(new { errors = new[] { "RegionId inválido." } });

    var alumno = mapper.Map<Alumno>(dto);
    db.Alumnos.Add(alumno);
    await db.SaveChangesAsync();

    return Results.Created($"/api/alumnos/{alumno.Id}", new { id = alumno.Id });
})
.WithName("CreateAlumno");

// PUT: actualizar
app.MapPut("/api/alumnos/{id:int}", async (int id, [FromBody] AlumnoCreateUpdateDto dto, AppDbContext db, IMapper mapper) =>
{
    var validation = ValidateAlumnoDto(dto);
    if (validation.Count > 0) return Results.BadRequest(new { errors = validation });

    var alumno = await db.Alumnos
        .Include(a => a.Correos)
        .Include(a => a.Telefonos)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (alumno is null) return Results.NotFound();

    // validar FKs
    if (!await db.TiposDocumento.AnyAsync(x => x.Id == dto.TipoDocumentoId))
        return Results.BadRequest(new { errors = new[] { "TipoDocumentoId inválido." } });
    if (!await db.Paises.AnyAsync(x => x.Id == dto.PaisId))
        return Results.BadRequest(new { errors = new[] { "PaisId inválido." } });
    if (!await db.Regiones.AnyAsync(x => x.Id == dto.RegionId))
        return Results.BadRequest(new { errors = new[] { "RegionId inválido." } });

    // actualizar campos simples
    alumno.Nombres = dto.Nombres;
    alumno.Apellidos = dto.Apellidos;
    alumno.TipoDocumentoId = dto.TipoDocumentoId;
    alumno.NumeroDocumento = dto.NumeroDocumento;
    alumno.PaisId = dto.PaisId;
    alumno.RegionId = dto.RegionId;
    alumno.FechaNacimiento = dto.FechaNacimiento;

    // Reemplazar colecciones
    db.Correos.RemoveRange(alumno.Correos);
    db.Telefonos.RemoveRange(alumno.Telefonos);
    alumno.Correos = dto.Correos.Select(c => new Correo { Email = c.Email }).ToList();
    alumno.Telefonos = dto.Telefonos.Select(t => new Telefono { Numero = t.Numero }).ToList();

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("UpdateAlumno");

app.MapGet("/api/regiones", async (AppDbContext db, [FromQuery] int? paisId) =>
app.MapDelete("/api/alumnos/{id:int}", async (int id, AppDbContext db) =>
{
    var alumno = await db.Alumnos.FindAsync(id);
    if (alumno is null) return Results.NotFound();

    db.Alumnos.Remove(alumno);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteAlumno");

// Catálogos
app.MapGet("/api/tipos-documento", async (AppDbContext db) =>
    Results.Ok(await db.TiposDocumento.AsNoTracking().OrderBy(x => x.Id).ToListAsync()));

app.MapGet("/api/paises", async (AppDbContext db) =>
    Results.Ok(await db.Paises.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync()));

app.MapGet("/api/regiones", async ([FromQuery] int? paisId, AppDbContext db) =>
{
    var q = db.Regiones.AsNoTracking();
    if (paisId.HasValue) q = q.Where(r => r.PaisId == paisId.Value);
    return Results.Ok(await q.OrderBy(x => x.Nombre).ToListAsync());
});

app.Run();

static List<string> ValidateAlumnoDto(AlumnoCreateUpdateDto dto)
{
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(dto.Nombres)) errors.Add("Nombres es obligatorio.");
    if (string.IsNullOrWhiteSpace(dto.Apellidos)) errors.Add("Apellidos es obligatorio.");
    if (string.IsNullOrWhiteSpace(dto.NumeroDocumento)) errors.Add("NumeroDocumento es obligatorio.");

    // emails válidos y al menos uno
    if (dto.Correos is null || dto.Correos.Count == 0) errors.Add("Debe incluir al menos un correo.");
    else
    {
        var rx = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (dto.Correos.Any(c => string.IsNullOrWhiteSpace(c.Email) || !rx.IsMatch(c.Email)))
            errors.Add("Formato de correo inválido.");
    }

    // teléfonos válidos y al menos uno (solo dígitos, + y espacios mínimos)
    if (dto.Telefonos is null || dto.Telefonos.Count == 0) errors.Add("Debe incluir al menos un número móvil.");
    else
    {
        var rxTel = new Regex(@"^[0-9+()\-\s]{6,20}$");
        if (dto.Telefonos.Any(t => string.IsNullOrWhiteSpace(t.Numero) || !rxTel.IsMatch(t.Numero)))
            errors.Add("Formato de número móvil inválido.");
    }

    // fecha razonable (>= 1900 y no futuro)
    var min = new DateOnly(1900, 1, 1);
    if (dto.FechaNacimiento < min || dto.FechaNacimiento > DateOnly.FromDateTime(DateTime.UtcNow.Date))
        errors.Add("FechaNacimiento fuera de rango.");

    return errors;
}
