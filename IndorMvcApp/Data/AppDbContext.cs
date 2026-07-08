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
    public DbSet<PropiedadHvacSistema> PropiedadHvacSistemas { get; set; }
    public DbSet<PropiedadWaterHeaterSistema> PropiedadWaterHeaterSistemas { get; set; }
    public DbSet<SolicitudRealtor> SolicitudesRealtor { get; set; }
    public DbSet<MovingSetupConfig> MovingSetupConfig { get; set; }
    public DbSet<MovingSetupServicio> MovingSetupServicios { get; set; }
    public DbSet<MovingSetupEnlaceRapido> MovingSetupEnlacesRapidos { get; set; }
    public DbSet<HomeCarePrioritiesConfig> HomeCarePrioritiesConfig { get; set; }
    public DbSet<HomeCarePriority> HomeCarePriorities { get; set; }
    public DbSet<MovingServicioLanding> MovingServicioLanding { get; set; }
    public DbSet<SolicitudMoving> SolicitudesMoving { get; set; }
    public DbSet<ArchivoMoving> ArchivosMoving { get; set; }
    public DbSet<CleaningServicioLanding> CleaningServicioLanding { get; set; }
    public DbSet<SolicitudCleaning> SolicitudesCleaning { get; set; }
    public DbSet<ArchivoCleaning> ArchivosCleaning { get; set; }
    public DbSet<PackingServicioLanding> PackingServicioLanding { get; set; }
    public DbSet<SolicitudPacking> SolicitudesPacking { get; set; }
    public DbSet<ArchivoPacking> ArchivosPacking { get; set; }
    public DbSet<FurnitureAssemblyServicioLanding> FurnitureAssemblyServicioLanding { get; set; }
    public DbSet<SolicitudFurnitureAssembly> SolicitudesFurnitureAssembly { get; set; }
    public DbSet<ArchivoFurnitureAssembly> ArchivosFurnitureAssembly { get; set; }
    public DbSet<TvWallMountingServicioLanding> TvWallMountingServicioLanding { get; set; }
    public DbSet<SolicitudTvWallMounting> SolicitudesTvWallMounting { get; set; }
    public DbSet<ArchivoTvWallMounting> ArchivosTvWallMounting { get; set; }
    public DbSet<UtilitiesSetupProveedorInternet> UtilitiesSetupProveedorInternet { get; set; }
    public DbSet<SolicitudUtilitiesSetup> SolicitudesUtilitiesSetup { get; set; }
    public DbSet<UtilitiesSetupContacto> UtilitiesSetupContactos { get; set; }
    public DbSet<SolicitudGeneralHelp> SolicitudesGeneralHelp { get; set; }
    public DbSet<ArchivoGeneralHelp> ArchivosGeneralHelp { get; set; }
    public DbSet<SolicitudRemodelingServicio> SolicitudesRemodelingServicio { get; set; }
    public DbSet<ArchivoRemodelingServicio> ArchivosRemodelingServicio { get; set; }
    public DbSet<SafeAirServicioLanding> SafeAirServicioLanding { get; set; }
    public DbSet<SolicitudSafeAir> SolicitudesSafeAir { get; set; }
    public DbSet<ArchivoSafeAir> ArchivosSafeAir { get; set; }
    public DbSet<LawnServicioLanding> LawnServicioLanding { get; set; }
    public DbSet<SolicitudLawn> SolicitudesLawn { get; set; }
    public DbSet<LawnCatalogOption> LawnCatalogOptions { get; set; }
    public DbSet<TrashServicioLanding> TrashServicioLanding { get; set; }
    public DbSet<SolicitudTrash> SolicitudesTrash { get; set; }
    public DbSet<CleaningProServicioLanding> CleaningProServicioLanding { get; set; }
    public DbSet<SolicitudCleaningPro> SolicitudesCleaningPro { get; set; }
    public DbSet<HvacMaintenanceServicioLanding> HvacMaintenanceServicioLanding { get; set; }
    public DbSet<SolicitudHvacMaintenance> SolicitudesHvacMaintenance { get; set; }
    public DbSet<ArchivoHvacMaintenance> ArchivosHvacMaintenance { get; set; }
    public DbSet<WaterHeaterFlushServicioLanding> WaterHeaterFlushServicioLanding { get; set; }
    public DbSet<SolicitudWaterHeaterFlush> SolicitudesWaterHeaterFlush { get; set; }
    public DbSet<ArchivoWaterHeaterFlush> ArchivosWaterHeaterFlush { get; set; }
    public DbSet<RoofInspectionServicioLanding> RoofInspectionServicioLanding { get; set; }
    public DbSet<SolicitudRoofInspection> SolicitudesRoofInspection { get; set; }
    public DbSet<ArchivoRoofInspection> ArchivosRoofInspection { get; set; }
    public DbSet<CrawlspaceCheckServicioLanding> CrawlspaceCheckServicioLanding { get; set; }
    public DbSet<SolicitudCrawlspaceCheck> SolicitudesCrawlspaceCheck { get; set; }
    public DbSet<ExteriorPaintServicioLanding> ExteriorPaintServicioLanding { get; set; }
    public DbSet<SolicitudExteriorPaint> SolicitudesExteriorPaint { get; set; }
    public DbSet<ArchivoExteriorPaint> ArchivosExteriorPaint { get; set; }
    public DbSet<GutterCleaningServicioLanding> GutterCleaningServicioLanding { get; set; }
    public DbSet<SolicitudGutterCleaning> SolicitudesGutterCleaning { get; set; }
    public DbSet<ArchivoGutterCleaning> ArchivosGutterCleaning { get; set; }
    public DbSet<PowerWashServicioLanding> PowerWashServicioLanding { get; set; }
    public DbSet<SolicitudPowerWash> SolicitudesPowerWash { get; set; }
    public DbSet<ArchivoPowerWash> ArchivosPowerWash { get; set; }
    public DbSet<PestControlServicioLanding> PestControlServicioLanding { get; set; }
    public DbSet<SolicitudPestControl> SolicitudesPestControl { get; set; }
    public DbSet<SmokeDetectorServicioLanding> SmokeDetectorServicioLanding { get; set; }
    public DbSet<SolicitudSmokeDetector> SolicitudesSmokeDetector { get; set; }

    public DbSet<IndorProveedorCategoriaCatalogo> IndorProveedorCategoriasCatalogo { get; set; }
    public DbSet<IndorProveedorOfertaCatalogo> IndorProveedorOfertasCatalogo { get; set; }
    public DbSet<IndorProveedor> IndorProveedores { get; set; }
    public DbSet<IndorProveedorCategoriaSel> IndorProveedorCategoriasSel { get; set; }
    public DbSet<IndorProveedorOfertaSel> IndorProveedorOfertasSel { get; set; }
    public DbSet<IndorProveedorExamPregunta> IndorProveedorExamPreguntas { get; set; }
    public DbSet<IndorProveedorExamRespuesta> IndorProveedorExamRespuestas { get; set; }
    public DbSet<IndorProveedorDocumento> IndorProveedorDocumentos { get; set; }
    public DbSet<IndorProveedorAlcanceRegla> IndorProveedorAlcanceReglas { get; set; }
    public DbSet<IndorProveedorCliente> IndorProveedorClientes { get; set; }
    public DbSet<IndorProveedorJob> IndorProveedorJobs { get; set; }
    public DbSet<IndorProveedorLead> IndorProveedorLeads { get; set; }
    public DbSet<IndorProveedorEstimate> IndorProveedorEstimates { get; set; }
    public DbSet<IndorProveedorInvoice> IndorProveedorInvoices { get; set; }
    public DbSet<IndorProveedorApproval> IndorProveedorApprovals { get; set; }
    public DbSet<IndorPasswordResetCode> IndorPasswordResetCodes { get; set; }
    public DbSet<IndorProviderInsuranceQuote> IndorProviderInsuranceQuotes { get; set; }
    public DbSet<IndorProveedorReport> IndorProveedorReports { get; set; }
    public DbSet<IndorProveedorReportPhoto> IndorProveedorReportPhotos { get; set; }
    public DbSet<IndorProveedorReportTemplate> IndorProveedorReportTemplates { get; set; }
    public DbSet<IndorProveedorReportTemplateSection> IndorProveedorReportTemplateSections { get; set; }
    public DbSet<IndorProveedorConversation> IndorProveedorConversations { get; set; }
    public DbSet<IndorProveedorMessage> IndorProveedorMessages { get; set; }

    // Contractor Network (subcontractor marketplace)
    public DbSet<IndorProveedorNetworkJob> IndorProveedorNetworkJobs { get; set; }
    public DbSet<IndorProveedorNetworkHire> IndorProveedorNetworkHires { get; set; }
    public DbSet<IndorProveedorNetworkResena> IndorProveedorNetworkResenas { get; set; }
    public DbSet<IndorProveedorNetworkGuardado> IndorProveedorNetworkGuardados { get; set; }
    public DbSet<IndorProveedorNetworkQuote> IndorProveedorNetworkQuotes { get; set; }
    public DbSet<IndorProveedorNetworkInvitacion> IndorProveedorNetworkInvitaciones { get; set; }

    // Contractor verification console (operator review queue)
    public DbSet<IndorProveedorVerificacion> IndorProveedorVerificaciones { get; set; }

    public DbSet<IndorPropertyAdministrator> IndorPropertyAdministrators { get; set; }
    public DbSet<IndorPropertyAdminPortfolioProperty> IndorPropertyAdminPortfolioProperties { get; set; }
    public DbSet<IndorPropertyAdminServiceRequest> IndorPropertyAdminServiceRequests { get; set; }
    public DbSet<IndorPropertyAdminHomecarePlan> IndorPropertyAdminHomecarePlans { get; set; }
    public DbSet<IndorPropertyAdminScheduledVisit> IndorPropertyAdminScheduledVisits { get; set; }
    public DbSet<IndorPropertyAdminServiceCatalogItem> IndorPropertyAdminServiceCatalog { get; set; }
    public DbSet<IndorPropertyAdminPreventiveServiceCatalogItem> IndorPropertyAdminPreventiveServiceCatalog { get; set; }
    public DbSet<IndorPropertyAdminPreventivePlan> IndorPropertyAdminPreventivePlans { get; set; }
    public DbSet<IndorRealtor> IndorRealtors { get; set; }
    public DbSet<IndorRealtorDocumento> IndorRealtorDocumentos { get; set; }
    public DbSet<IndorRealtorPropertyFile> IndorRealtorPropertyFiles { get; set; }
    public DbSet<IndorRealtorPropertyFileDraft> IndorRealtorPropertyFileDrafts { get; set; }
    public DbSet<IndorRealtorPropertyFileDraftCategory> IndorRealtorPropertyFileDraftCategories { get; set; }
    public DbSet<IndorRealtorPropertyFileDraftItem> IndorRealtorPropertyFileDraftItems { get; set; }
    public DbSet<IndorRealtorPropertyFileItem> IndorRealtorPropertyFileItems { get; set; }
    public DbSet<IndorRealtorQuote> IndorRealtorQuotes { get; set; }
    public DbSet<IndorRealtorQuoteProvider> IndorRealtorQuoteProviders { get; set; }
    public DbSet<IndorRealtorQuoteRequestDraft> IndorRealtorQuoteRequestDrafts { get; set; }
    public DbSet<IndorRealtorQuoteRequestDraftProvider> IndorRealtorQuoteRequestDraftProviders { get; set; }
    public DbSet<IndorRealtorQuoteSentProvider> IndorRealtorQuoteSentProviders { get; set; }
    public DbSet<IndorRealtorInspectionUploadDraft> IndorRealtorInspectionUploadDrafts { get; set; }
    public DbSet<IndorRealtorInspectionUploadFinding> IndorRealtorInspectionUploadFindings { get; set; }
    public DbSet<IndorRealtorInspectionDraftProvider> IndorRealtorInspectionDraftProviders { get; set; }
    public DbSet<IndorRealtorUrgentQuoteDraft> IndorRealtorUrgentQuoteDrafts { get; set; }
    public DbSet<IndorRealtorUrgentQuoteDraftPhoto> IndorRealtorUrgentQuoteDraftPhotos { get; set; }
    public DbSet<IndorRealtorSharedPackage> IndorRealtorSharedPackages { get; set; }
    public DbSet<IndorRealtorClient> IndorRealtorClients { get; set; }
    public DbSet<IndorRealtorInvitation> IndorRealtorInvitations { get; set; }
    public DbSet<IndorRealtorActivity> IndorRealtorActivities { get; set; }
    public DbSet<IndorRealtorQuoteBid> IndorRealtorQuoteBids { get; set; }
    public DbSet<IndorRealtorSharedQuote> IndorRealtorSharedQuotes { get; set; }
    public DbSet<IndorNearbyNetworkSetting> IndorNearbyNetworkSettings { get; set; }
    public DbSet<IndorNearbyNetworkItem> IndorNearbyNetworkItems { get; set; }
    public DbSet<IndorNeighborRequest> IndorNeighborRequests { get; set; }
    public DbSet<IndorNeighborRequestCategory> IndorNeighborRequestCategories { get; set; }
    public DbSet<IndorNeighborRequestPhoto> IndorNeighborRequestPhotos { get; set; }
    public DbSet<IndorNeighborRequestOffer> IndorNeighborRequestOffers { get; set; }

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

        modelBuilder.Entity<SolicitudMoving>(entity =>
        {
            entity.Property(s => s.FechaMovimiento).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.MovingSetupServicio)
                  .WithMany()
                  .HasForeignKey(s => s.MovingSetupServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoMoving>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudMovingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudCleaning>(entity =>
        {
            entity.Property(s => s.FechaServicio).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.MovingSetupServicio)
                  .WithMany()
                  .HasForeignKey(s => s.MovingSetupServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoCleaning>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudCleaningId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudPacking>(entity =>
        {
            entity.Property(s => s.FechaServicio).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.MovingSetupServicio)
                  .WithMany()
                  .HasForeignKey(s => s.MovingSetupServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoPacking>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudPackingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudFurnitureAssembly>(entity =>
        {
            entity.Property(s => s.FechaServicio).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.MovingSetupServicio)
                  .WithMany()
                  .HasForeignKey(s => s.MovingSetupServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoFurnitureAssembly>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudFurnitureAssemblyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudTvWallMounting>(entity =>
        {
            entity.Property(s => s.FechaServicio).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.MovingSetupServicio)
                  .WithMany()
                  .HasForeignKey(s => s.MovingSetupServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoTvWallMounting>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudTvWallMountingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudUtilitiesSetup>(entity =>
        {
            entity.Property(s => s.FechaServicio).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.MovingSetupServicio)
                  .WithMany()
                  .HasForeignKey(s => s.MovingSetupServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(s => s.ProveedorInternet)
                  .WithMany()
                  .HasForeignKey(s => s.ProveedorInternetId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UtilitiesSetupContacto>(entity =>
        {
            entity.HasOne(c => c.Solicitud)
                  .WithMany(s => s.Contactos)
                  .HasForeignKey(c => c.SolicitudUtilitiesSetupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudGeneralHelp>(entity =>
        {
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.MovingSetupServicio)
                  .WithMany()
                  .HasForeignKey(s => s.MovingSetupServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoGeneralHelp>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudGeneralHelpId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolicitudRemodelingServicio>(entity =>
        {
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Servicio)
                  .WithMany()
                  .HasForeignKey(s => s.ServicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoRemodelingServicio>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudRemodelingServicioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SafeAirServicioLanding>(entity =>
        {
            entity.HasOne(l => l.Microservicio)
                  .WithMany()
                  .HasForeignKey(l => l.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudSafeAir>(entity =>
        {
            entity.Property(s => s.FiltroAncho).HasColumnType("decimal(5,2)");
            entity.Property(s => s.FiltroAlto).HasColumnType("decimal(5,2)");
            entity.Property(s => s.FiltroProfundidad).HasColumnType("decimal(5,2)");
            entity.Property(s => s.FechaProximoRecordatorio).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Microservicio)
                  .WithMany()
                  .HasForeignKey(s => s.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoSafeAir>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudSafeAirId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LawnServicioLanding>(entity =>
        {
            entity.HasOne(l => l.Microservicio)
                  .WithMany()
                  .HasForeignKey(l => l.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudLawn>(entity =>
        {
            entity.Property(s => s.PrecioBase).HasColumnType("decimal(10,2)");
            entity.Property(s => s.PrecioAddons).HasColumnType("decimal(10,2)");
            entity.Property(s => s.DescuentoSuscripcion).HasColumnType("decimal(10,2)");
            entity.Property(s => s.PrecioTotal).HasColumnType("decimal(10,2)");
            entity.Property(s => s.FechaPreferida).HasColumnType("date");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Microservicio)
                  .WithMany()
                  .HasForeignKey(s => s.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LawnCatalogOption>(entity =>
        {
            entity.Property(o => o.Price).HasColumnType("decimal(10,2)");
            entity.HasOne(o => o.Microservicio)
                  .WithMany()
                  .HasForeignKey(o => o.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrashServicioLanding>(entity =>
        {
            entity.HasOne(l => l.Microservicio)
                  .WithMany()
                  .HasForeignKey(l => l.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudTrash>(entity =>
        {
            entity.Property(s => s.PrecioMensual).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Microservicio)
                  .WithMany()
                  .HasForeignKey(s => s.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CleaningProServicioLanding>(entity =>
        {
            entity.HasOne(l => l.Microservicio)
                  .WithMany()
                  .HasForeignKey(l => l.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudCleaningPro>(entity =>
        {
            entity.Property(s => s.HorasEstimadas).HasColumnType("decimal(4,1)");
            entity.Property(s => s.TarifaHoraria).HasColumnType("decimal(10,2)");
            entity.Property(s => s.Subtotal).HasColumnType("decimal(10,2)");
            entity.Property(s => s.ImpuestoVenta).HasColumnType("decimal(10,2)");
            entity.Property(s => s.PrecioTotal).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Microservicio)
                  .WithMany()
                  .HasForeignKey(s => s.MicroservicioId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<HvacMaintenanceServicioLanding>(entity =>
        {
            entity.Property(l => l.PrecioDesde).HasColumnType("decimal(10,2)");
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudHvacMaintenance>(entity =>
        {
            entity.Property(s => s.PrecioEstimado).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoHvacMaintenance>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudHvacMaintenanceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WaterHeaterFlushServicioLanding>(entity =>
        {
            entity.Property(l => l.PrecioDesde).HasColumnType("decimal(10,2)");
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudWaterHeaterFlush>(entity =>
        {
            entity.Property(s => s.PrecioEstimado).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoWaterHeaterFlush>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudWaterHeaterFlushId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoofInspectionServicioLanding>(entity =>
        {
            entity.Property(l => l.PrecioDesde).HasColumnType("decimal(10,2)");
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudRoofInspection>(entity =>
        {
            entity.Property(s => s.PrecioEstimado).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoRoofInspection>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudRoofInspectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CrawlspaceCheckServicioLanding>(entity =>
        {
            entity.Property(l => l.PrecioDesde).HasColumnType("decimal(10,2)");
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudCrawlspaceCheck>(entity =>
        {
            entity.Property(s => s.PrecioEstimado).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExteriorPaintServicioLanding>(entity =>
        {
            entity.Property(l => l.PrecioDesde).HasColumnType("decimal(10,2)");
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudExteriorPaint>(entity =>
        {
            entity.Property(s => s.PrecioEstimado).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoExteriorPaint>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudExteriorPaintId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GutterCleaningServicioLanding>(entity =>
        {
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudGutterCleaning>(entity =>
        {
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoGutterCleaning>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudGutterCleaningId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PowerWashServicioLanding>(entity =>
        {
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudPowerWash>(entity =>
        {
            entity.Property(s => s.PrecioEstimado).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArchivoPowerWash>(entity =>
        {
            entity.HasOne(a => a.Solicitud)
                  .WithMany(s => s.Archivos)
                  .HasForeignKey(a => a.SolicitudPowerWashId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PestControlServicioLanding>(entity =>
        {
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudPestControl>(entity =>
        {
            entity.Property(s => s.PrecioEstimado).HasColumnType("decimal(10,2)");
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SmokeDetectorServicioLanding>(entity =>
        {
            entity.HasOne(l => l.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(l => l.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SolicitudSmokeDetector>(entity =>
        {
            entity.HasOne(s => s.Usuario)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.HomeCarePriority)
                  .WithMany()
                  .HasForeignKey(s => s.HomeCarePriorityId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Propiedad)
                  .WithMany()
                  .HasForeignKey(s => s.PropiedadId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IndorProveedorCategoriaSel>(entity =>
        {
            entity.HasKey(e => new { e.ProveedorId, e.CategoriaId });
            entity.HasOne(e => e.Proveedor)
                  .WithMany(p => p.Categorias)
                  .HasForeignKey(e => e.ProveedorId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Categoria)
                  .WithMany()
                  .HasForeignKey(e => e.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IndorProveedorOfertaSel>(entity =>
        {
            entity.HasKey(e => new { e.ProveedorId, e.OfertaId });
            entity.HasOne(e => e.Proveedor)
                  .WithMany(p => p.Ofertas)
                  .HasForeignKey(e => e.ProveedorId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Oferta)
                  .WithMany()
                  .HasForeignKey(e => e.OfertaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IndorProveedorExamPregunta>(entity =>
        {
            entity.HasKey(e => new { e.TradeCode, e.QuestionNumber });
        });

        modelBuilder.Entity<IndorProveedor>(entity =>
        {
            entity.HasIndex(e => e.RegistrationToken).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IndorNearbyNetworkSetting>(entity =>
        {
            entity.HasOne(e => e.Realtor)
                  .WithOne(r => r.NetworkSetting)
                  .HasForeignKey<IndorNearbyNetworkSetting>(e => e.RealtorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IndorNearbyNetworkItem>(entity =>
        {
            entity.HasOne(e => e.OwnerRealtor)
                  .WithMany(r => r.NetworkItems)
                  .HasForeignKey(e => e.OwnerRealtorId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.RelatedClient)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedClientId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<IndorNeighborRequest>(entity =>
        {
            entity.HasOne(e => e.Propiedad)
                  .WithMany()
                  .HasForeignKey(e => e.PropiedadId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IndorNeighborRequestPhoto>(entity =>
        {
            entity.HasOne(e => e.Request)
                  .WithMany(r => r.Photos)
                  .HasForeignKey(e => e.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IndorNeighborRequestOffer>(entity =>
        {
            entity.HasOne(e => e.Request)
                  .WithMany(r => r.Offers)
                  .HasForeignKey(e => e.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

