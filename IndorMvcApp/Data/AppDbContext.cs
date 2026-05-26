using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;

namespace IndorMvcApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Ejemplo de DbSet - agrega los tuyos aquí
    public DbSet<Propiedad> Propiedades { get; set; }
    public DbSet<Microservicio> Microservicios { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<Inspeccion> Inspecciones { get; set; }
    public DbSet<PlanMembresia> PlanesMembresia { get; set; }
    public DbSet<MembresiaUsuario> MembresiasUsuario { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<PlanInternet> PlanesInternet { get; set; }
    public DbSet<HistorialServicio> HistorialServicios { get; set; }
    public DbSet<MensajeSoporte> MensajesSoporte { get; set; }
    public DbSet<ProgramacionMicroservicio> ProgramacionesMicroservicio { get; set; }
    public DbSet<SolicitudInspeccion> SolicitudesInspeccion { get; set; }
    public DbSet<ArchivoReporteInspeccion> ArchivosReporteInspeccion { get; set; }
    public DbSet<SolicitudInspeccionElectrica> SolicitudesInspeccionElectrica { get; set; }
    public DbSet<ArchivoInspeccionElectrica> ArchivosInspeccionElectrica { get; set; }
    public DbSet<SolicitudInspeccionCompleta> SolicitudesInspeccionCompleta { get; set; }
    public DbSet<ArchivoInspeccionCompleta> ArchivosInspeccionCompleta { get; set; }

    // Agrega más DbSets según necesites:
    // public DbSet<Usuario> Usuarios { get; set; }
    // public DbSet<Cliente> Clientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones adicionales de tus modelos aquí
        modelBuilder.Entity<Propiedad>(entity =>
        {
            entity.Property(p => p.DatosJson).HasColumnType("nvarchar(max)");
            entity.HasOne(p => p.Usuario)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProgramacionMicroservicio>(entity =>
        {
            entity.Property(p => p.FechaProgramada).HasColumnType("date");
            entity.HasOne(p => p.Usuario)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.Microservicio)
                  .WithMany()
                  .HasForeignKey(p => p.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.Propiedad)
                  .WithMany()
                  .HasForeignKey(p => p.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SolicitudInspeccion>(entity =>
        {
            entity.Property(s => s.FechaCierreEstimada).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Inspeccion)
                  .WithMany()
                  .HasForeignKey(s => s.InspeccionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoReporteInspeccion>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionElectrica>(entity =>
        {
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Inspeccion)
                  .WithMany()
                  .HasForeignKey(s => s.InspeccionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoInspeccionElectrica>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionElectricaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionCompleta>(entity =>
        {
            entity.Property(s => s.FechaCitaProgramada).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Inspeccion)
                  .WithMany()
                  .HasForeignKey(s => s.InspeccionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoInspeccionCompleta>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionCompletaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

