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
    public DbSet<ServicioEmergencia> ServiciosEmergencia { get; set; }
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
    public DbSet<SolicitudInspeccionPlomeria> SolicitudesInspeccionPlomeria { get; set; }
    public DbSet<ArchivoInspeccionPlomeria> ArchivosInspeccionPlomeria { get; set; }
    public DbSet<SolicitudInspeccionHvac> SolicitudesInspeccionHvac { get; set; }
    public DbSet<ArchivoInspeccionHvac> ArchivosInspeccionHvac { get; set; }
    public DbSet<SolicitudInspeccionStructural> SolicitudesInspeccionStructural { get; set; }
    public DbSet<ArchivoInspeccionStructural> ArchivosInspeccionStructural { get; set; }
    public DbSet<SolicitudInspeccionRoof> SolicitudesInspeccionRoof { get; set; }
    public DbSet<ArchivoInspeccionRoof> ArchivosInspeccionRoof { get; set; }
    public DbSet<SolicitudInspeccionMoldMoisture> SolicitudesInspeccionMoldMoisture { get; set; }
    public DbSet<ArchivoInspeccionMoldMoisture> ArchivosInspeccionMoldMoisture { get; set; }
    public DbSet<SolicitudInspeccionWindowsInsulation> SolicitudesInspeccionWindowsInsulation { get; set; }
    public DbSet<ArchivoInspeccionWindowsInsulation> ArchivosInspeccionWindowsInsulation { get; set; }
    public DbSet<SolicitudInspeccionHomeSafety> SolicitudesInspeccionHomeSafety { get; set; }
    public DbSet<ArchivoInspeccionHomeSafety> ArchivosInspeccionHomeSafety { get; set; }
    public DbSet<SolicitudInspeccionInvestor> SolicitudesInspeccionInvestor { get; set; }
    public DbSet<ArchivoInspeccionInvestor> ArchivosInspeccionInvestor { get; set; }
    public DbSet<SolicitudEmergenciaPlomeria> SolicitudesEmergenciaPlomeria { get; set; }
    public DbSet<ArchivoEmergenciaPlomeria> ArchivosEmergenciaPlomeria { get; set; }
    public DbSet<SolicitudEmergenciaHvac> SolicitudesEmergenciaHvac { get; set; }
    public DbSet<ArchivoEmergenciaHvac> ArchivosEmergenciaHvac { get; set; }
    public DbSet<SolicitudEmergenciaWaterHeater> SolicitudesEmergenciaWaterHeater { get; set; }
    public DbSet<ArchivoEmergenciaWaterHeater> ArchivosEmergenciaWaterHeater { get; set; }
    public DbSet<SolicitudEmergenciaFlood> SolicitudesEmergenciaFlood { get; set; }
    public DbSet<ArchivoEmergenciaFlood> ArchivosEmergenciaFlood { get; set; }
    public DbSet<SolicitudEmergenciaElectrical> SolicitudesEmergenciaElectrical { get; set; }
    public DbSet<ArchivoEmergenciaElectrical> ArchivosEmergenciaElectrical { get; set; }
    public DbSet<SolicitudEmergenciaTreeDamage> SolicitudesEmergenciaTreeDamage { get; set; }
    public DbSet<ArchivoEmergenciaTreeDamage> ArchivosEmergenciaTreeDamage { get; set; }
    public DbSet<SolicitudEmergenciaRoofLeak> SolicitudesEmergenciaRoofLeak { get; set; }
    public DbSet<ArchivoEmergenciaRoofLeak> ArchivosEmergenciaRoofLeak { get; set; }
    public DbSet<SolicitudEmergenciaSmokeDetector> SolicitudesEmergenciaSmokeDetector { get; set; }
    public DbSet<PropiedadHistorial> PropiedadHistorial { get; set; }
    public DbSet<PropiedadProveedor> PropiedadProveedores { get; set; }
    public DbSet<PropiedadMantenimiento> PropiedadMantenimiento { get; set; }
    public DbSet<PropiedadDocumento> PropiedadDocumentos { get; set; }

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

        modelBuilder.Entity<SolicitudInspeccionPlomeria>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionPlomeria>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionPlomeriaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionHvac>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionHvac>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionHvacId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionStructural>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionStructural>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionStructuralId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionRoof>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionRoof>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionRoofId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionMoldMoisture>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionMoldMoisture>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionMoldMoistureId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionWindowsInsulation>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionWindowsInsulation>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionWindowsInsulationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionHomeSafety>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionHomeSafety>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionHomeSafetyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudInspeccionInvestor>(entity =>
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

        modelBuilder.Entity<ArchivoInspeccionInvestor>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudInspeccionInvestorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

