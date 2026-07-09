using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndorMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredUiCulture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AttomLastSyncUtc",
                table: "Propiedades",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AttomPropertyId",
                table: "Propiedades",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttomRawJson",
                table: "Propiedades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttomSyncError",
                table: "Propiedades",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttomSyncStatus",
                table: "Propiedades",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MantenimientoRecomendadoJson",
                table: "Propiedades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MantenimientoRecomendadoUtc",
                table: "Propiedades",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredUiCulture",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistorialServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    NombreItem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialServicios_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeCarePriorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IconoClase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkController = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkAction = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeCarePriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeCarePrioritiesConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IconoClase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ViewAllTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ViewAllController = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ViewAllAction = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeCarePrioritiesConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborRequestCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborRequestCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorPasswordResetCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ResetToken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    UsedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPasswordResetCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdministrators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RegistrationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PortfolioBusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TermsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    MarketingOptIn = table.Column<bool>(type: "bit", nullable: false),
                    TermsAcceptedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PropertyCountRange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PortfolioType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OwnershipType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PrimaryMarket = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ManagementStyle = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ToolMaintenanceRequests = table.Column<bool>(type: "bit", nullable: false),
                    ToolTurnoverCleaning = table.Column<bool>(type: "bit", nullable: false),
                    ToolGuestMessaging = table.Column<bool>(type: "bit", nullable: false),
                    ToolInvoicesPayments = table.Column<bool>(type: "bit", nullable: false),
                    ToolDocumentsWarranties = table.Column<bool>(type: "bit", nullable: false),
                    ToolServiceProviders = table.Column<bool>(type: "bit", nullable: false),
                    ToolTeamAccess = table.Column<bool>(type: "bit", nullable: false),
                    NotifyUrgentMaintenance = table.Column<bool>(type: "bit", nullable: false),
                    NotifyWeeklySummary = table.Column<bool>(type: "bit", nullable: false),
                    NotifyBookingLeaseUpdates = table.Column<bool>(type: "bit", nullable: false),
                    NotifyPushEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NotifyEmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NotifySmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NotifyPropertyUpdates = table.Column<bool>(type: "bit", nullable: false),
                    NotifyServiceUpdates = table.Column<bool>(type: "bit", nullable: false),
                    NotifyTaskReminders = table.Column<bool>(type: "bit", nullable: false),
                    NotifyPaymentsBilling = table.Column<bool>(type: "bit", nullable: false),
                    QuietHoursStart = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    QuietHoursEnd = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    PlatformTermsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationCompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdministrators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorPropertyAdministrators_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdminPreventiveServiceCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceKey = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DefaultFrequency = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToneClass = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdminPreventiveServiceCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdminServiceCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CategoryTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CategoryOrder = table.Column<int>(type: "int", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceSlug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToneClass = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LinkController = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkAction = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkRouteId = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdminServiceCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorAlcanceReglas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TradeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsAllowed = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorAlcanceReglas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorCategoriasCatalogo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RequiresTradeExam = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorCategoriasCatalogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RegistrationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    ProviderType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DbaName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PrimaryContact = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    YearsExperience = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LanguagesJson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EpaCertificationNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    BackgroundCheckConsent = table.Column<bool>(type: "bit", nullable: false),
                    ServiceDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsInsured = table.Column<bool>(type: "bit", nullable: false),
                    IsLicensed = table.Column<bool>(type: "bit", nullable: false),
                    TeamSize = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BusinessAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrimaryCity = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    TravelRadiusMiles = table.Column<int>(type: "int", nullable: false),
                    ZipNeighborhoodsJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmergencyService = table.Column<bool>(type: "bit", nullable: false),
                    SameDayJobs = table.Column<bool>(type: "bit", nullable: false),
                    AvailableDaysJson = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PreferredHours = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    JobSizesJson = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LogoUploaded = table.Column<bool>(type: "bit", nullable: false),
                    ScopeTradeUnderstood = table.Column<bool>(type: "bit", nullable: false),
                    ScopeStandardsAgreed = table.Column<bool>(type: "bit", nullable: false),
                    ExamScorePercent = table.Column<int>(type: "int", nullable: true),
                    ExamPassed = table.Column<bool>(type: "bit", nullable: true),
                    ExamSubmittedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfileSubmittedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OnboardingMetaJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedores_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorExamPreguntas",
                columns: table => new
                {
                    TradeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QuestionNumber = table.Column<int>(type: "int", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    TextEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectIndex = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorExamPreguntas", x => new { x.TradeCode, x.QuestionNumber });
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorNetworkGuardados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerProveedorId = table.Column<int>(type: "int", nullable: false),
                    SubcontractorProveedorId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorNetworkGuardados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorNetworkHires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HirerProveedorId = table.Column<int>(type: "int", nullable: false),
                    SubcontractorProveedorId = table.Column<int>(type: "int", nullable: false),
                    NetworkJobId = table.Column<int>(type: "int", nullable: true),
                    ProjectTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    TradeLabel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BudgetRange = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorNetworkHires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorNetworkInvitaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InviterProveedorId = table.Column<int>(type: "int", nullable: false),
                    SubcontractorProveedorId = table.Column<int>(type: "int", nullable: false),
                    NetworkJobId = table.Column<int>(type: "int", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    TradeId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ServiceCategory = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PropertyAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleToday = table.Column<bool>(type: "bit", nullable: false),
                    BudgetRange = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    TimingPreference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AttachmentsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorNetworkInvitaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorNetworkJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosterProveedorId = table.Column<int>(type: "int", nullable: false),
                    TradeId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TradeLabel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DateNeeded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BudgetRange = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Urgency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PropertyType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WhoMeets = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    QuoteType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccessNotes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PhotoUrlsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorNetworkJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorNetworkQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NetworkJobId = table.Column<int>(type: "int", nullable: false),
                    SubcontractorProveedorId = table.Column<int>(type: "int", nullable: false),
                    AmountLow = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AmountHigh = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    QuotedAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ResponseMinutes = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorNetworkQuotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorNetworkResenas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubcontractorProveedorId = table.Column<int>(type: "int", nullable: false),
                    AuthorProveedorId = table.Column<int>(type: "int", nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorNetworkResenas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorOfertasCatalogo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorOfertasCatalogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorQuoteProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Categories = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(2,1)", nullable: false),
                    DistanceMiles = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    BadgeLabel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsRecommended = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorQuoteProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RegistrationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BrokerageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LicenseState = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ServiceAreas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfessionalTermsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    TermsAcceptedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationSkipped = table.Column<bool>(type: "bit", nullable: false),
                    ProfileCompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfilePhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublicTagline = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PublicBio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OfficeAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LanguagesJson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IndorMessagingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PublicDisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RealtorTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OfficeCity = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    OfficeState = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OfficeZip = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    YearsOfExperience = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SpecialtiesJson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BrokerInCharge = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtors_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Inspecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DescripcionCompleta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Incluye = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frecuencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PrecioPrefijo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrecioTexto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspecciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MensajesSoporte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Remitente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Leido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesSoporte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensajesSoporte_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetodosPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Ultimos4 = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Titular = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Expiracion = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    EsPredeterminado = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetodosPago_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Microservicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DescripcionCompleta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Incluye = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frecuencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PrecioPrefijo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ImagenBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Microservicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovingSetupConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IconoClase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ViewAllTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ViewAllUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FeaturedEtiqueta = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FeaturedTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FeaturedDescripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FeaturedImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FeaturedCaracteristicas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FeaturedIconosCaracteristicas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FeaturedCtaTexto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FeaturedCtaController = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FeaturedCtaAction = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FeaturedCtaRouteId = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovingSetupConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovingSetupEnlacesRapidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IconoClase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkController = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkAction = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkRouteId = table.Column<int>(type: "int", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovingSetupEnlacesRapidos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovingSetupServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IconoClase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkController = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkAction = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LinkRouteId = table.Column<int>(type: "int", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovingSetupServicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesInternet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Proveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VelocidadDescargaMbps = table.Column<int>(type: "int", nullable: false),
                    VelocidadSubidaMbps = table.Column<int>(type: "int", nullable: false),
                    PrecioMensual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Caracteristicas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsPlanActual = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesInternet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesMembresia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrecioMensual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Caracteristicas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Recomendado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesMembresia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadDocumentos_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadHvacSistemas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SystemType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    InstallYear = table.Column<int>(type: "int", nullable: true),
                    FilterSize = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FilterRemindersEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FilterReminderDays = table.Column<int>(type: "int", nullable: false),
                    HasPets = table.Column<bool>(type: "bit", nullable: true),
                    FilterScheduleMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NextFilterChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemindOneWeekBefore = table.Column<bool>(type: "bit", nullable: false),
                    RemindOneDayBefore = table.Column<bool>(type: "bit", nullable: false),
                    FilterReminderSetupComplete = table.Column<bool>(type: "bit", nullable: false),
                    FilterNotificationsConsent = table.Column<bool>(type: "bit", nullable: false),
                    OpenAiDataSource = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LabelImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadHvacSistemas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadHvacSistemas_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadProveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ServiceCategory = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadProveedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadProveedores_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadWaterHeaterSistemas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    HeaterType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    InstallYear = table.Column<int>(type: "int", nullable: true),
                    TankSize = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlushRemindersEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FlushReminderDays = table.Column<int>(type: "int", nullable: false),
                    NextFlushDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlushLocation = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    RemindOneWeekBefore = table.Column<bool>(type: "bit", nullable: false),
                    RemindOneDayBefore = table.Column<bool>(type: "bit", nullable: false),
                    AutoRepeatEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FlushReminderSetupComplete = table.Column<bool>(type: "bit", nullable: false),
                    FlushNotificationsConsent = table.Column<bool>(type: "bit", nullable: false),
                    OpenAiDataSource = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LabelImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadWaterHeaterSistemas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadWaterHeaterSistemas_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Subtitulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DescripcionCompleta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Incluye = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frecuencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PrecioPrefijo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrecioTexto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiciosEmergencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TituloEmergencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TiempoLlegadaMinutos = table.Column<int>(type: "int", nullable: false),
                    IconoClase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BadgeTexto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EsPredeterminado = table.Column<bool>(type: "bit", nullable: false),
                    Caracteristicas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconosCaracteristicas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosEmergencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesRealtor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NeedType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PreferredArea = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Timeframe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PriceRange = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GuidanceStep = table.Column<int>(type: "int", nullable: false),
                    RentComfortRange = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    HomeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Bedrooms = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Bathrooms = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Occupants = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Pets = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OutdoorSpaceImportance = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ParkingNeed = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OpenToNearbyAreas = table.Column<bool>(type: "bit", nullable: false),
                    Priorities = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PreferredContactMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GuidanceNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesRealtor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesRealtor_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UtilitiesSetupProveedorInternet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Etiqueta = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Velocidad = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    DetalleExtra = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilitiesSetupProveedorInternet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrawlspaceCheckServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PreocupacionItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreocupacionIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ResumenServicioTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrawlspaceCheckServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrawlspaceCheckServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExteriorPaintServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    WhyItMattersItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WhyItMattersIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NextStepsItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NextStepsIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReminderTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ResumenServicioTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExteriorPaintServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExteriorPaintServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GutterCleaningServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WhyItMattersItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WhyItMattersIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NextStepsItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NextStepsIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecommendedTimingItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecommendedTimingIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoConfirmacionTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GutterCleaningServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GutterCleaningServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HvacMaintenanceServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PreviewItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviewIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HvacMaintenanceServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HvacMaintenanceServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PestControlServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WhyItMattersItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WhyItMattersIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BestForTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    InfoPlanTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    WhyYearlyItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WhyYearlyIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PestControlServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PestControlServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowerWashServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BestForItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BestForIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PreviewTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TipConfirmacionTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoCondicionTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerWashServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerWashServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoofInspectionServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RecomendacionItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecomendacionIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoofInspectionServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoofInspectionServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmokeDetectorServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TrackItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrackDescriptions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrackIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WhereTrackItems = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WhereTrackIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReminderBannerTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmokeDetectorServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmokeDetectorServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesCrawlspaceCheck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    Encapsulacion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Aislamiento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BarreraVapor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoAcceso = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UltimaRevision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PreocupacionesSeleccionadas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TimingPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RecordatorioAnual = table.Column<bool>(type: "bit", nullable: false),
                    FechaPreferida = table.Column<DateTime>(type: "date", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCrawlspaceCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCrawlspaceCheck_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesCrawlspaceCheck_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCrawlspaceCheck_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesExteriorPaint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    UltimaPintura = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoSuperficie = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MantenerMismoColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ProblemasSeleccionados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AreasSeleccionadas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActualizacionColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LavadoPresionReciente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NumeroPisos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TimingPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RecordatorioAnual = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesExteriorPaint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesExteriorPaint_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesExteriorPaint_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesExteriorPaint_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesGutterCleaning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    TipoAccionInicial = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroPisos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoCanaletas = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProtectorCanaletas = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UltimaLimpieza = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CantidadBajantes = table.Column<int>(type: "int", nullable: true),
                    ProblemasSeleccionados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AreaProblema = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ObjetivoHoy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PreferenciaRecordatorio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaRecordatorioPersonalizada = table.Column<DateTime>(type: "date", nullable: true),
                    FechaVisitaPreferida = table.Column<DateTime>(type: "date", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecordatorioPrimaveraOtono = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesGutterCleaning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesGutterCleaning_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesGutterCleaning_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesGutterCleaning_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesHvacMaintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    NumeroSerieAc = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SerialDesconocido = table.Column<bool>(type: "bit", nullable: false),
                    UltimoMantenimiento = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UltimoMantenimientoDesconocido = table.Column<bool>(type: "bit", nullable: false),
                    TamanioFiltro = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    NotasTecnico = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaVisita = table.Column<DateTime>(type: "date", nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoServicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RecordatorioAnual = table.Column<bool>(type: "bit", nullable: false),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesHvacMaintenance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesHvacMaintenance_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesHvacMaintenance_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesHvacMaintenance_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesPestControl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    TipoAccionInicial = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UltimoServicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SignosSeleccionados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AreasPreocupacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MascotasONinos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoServicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimingPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RecordatorioAnual = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPestControl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesPestControl_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesPestControl_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesPestControl_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesPowerWash",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    AreasSeleccionadas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaterialExterior = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NumeroPisos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ProblemasSeleccionados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AreasDelicadas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccesoGrifo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TimingPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaPreferida = table.Column<DateTime>(type: "date", nullable: true),
                    RecordatorioAnual = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPowerWash", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesPowerWash_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesPowerWash_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesPowerWash_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesRoofInspection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    MotivoRevision = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoTecho = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EdadTecho = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UltimaInspeccion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoServicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Frecuencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimingPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaPreferida = table.Column<DateTime>(type: "date", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesRoofInspection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesRoofInspection_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesRoofInspection_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesRoofInspection_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesSmokeDetector",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    CantidadAlarmas = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UbicacionesSeleccionadas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TiposAlarmas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UltimaPrueba = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UltimoCambioBateria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AnioInstalacion = table.Column<int>(type: "int", nullable: true),
                    AnioInstalacionDesconocido = table.Column<bool>(type: "bit", nullable: false),
                    ProblemasSeleccionados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecordatorioMensual = table.Column<bool>(type: "bit", nullable: false),
                    RecordatorioBateriaAnual = table.Column<bool>(type: "bit", nullable: false),
                    RecordatorioReemplazo10Anos = table.Column<bool>(type: "bit", nullable: false),
                    RecordatorioRevisionEstacional = table.Column<bool>(type: "bit", nullable: false),
                    TipoAccionAyuda = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaInstalacionReferencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesSmokeDetector", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesSmokeDetector_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesSmokeDetector_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesSmokeDetector_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesWaterHeaterFlush",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    TipoCalentador = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    FuenteEnergia = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    NumeroSerie = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SerialDesconocido = table.Column<bool>(type: "bit", nullable: false),
                    MarcaModelo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UltimoFlush = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SintomasSeleccionados = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoServicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RecordatorioAnual = table.Column<bool>(type: "bit", nullable: false),
                    PreferenciaTiempo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaVisita = table.Column<DateTime>(type: "date", nullable: true),
                    NotasAdicionales = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesWaterHeaterFlush", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesWaterHeaterFlush_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesWaterHeaterFlush_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesWaterHeaterFlush_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WaterHeaterFlushServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCarePriorityId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PreviewItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviewIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ResumenServicioTexto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterHeaterFlushServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterHeaterFlushServicioLanding_HomeCarePriorities_HomeCarePriorityId",
                        column: x => x.HomeCarePriorityId,
                        principalTable: "HomeCarePriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DetailsSummary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LocationAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NeededByDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeWindowStart = table.Column<TimeOnly>(type: "time", nullable: true),
                    TimeWindowEnd = table.Column<TimeOnly>(type: "time", nullable: true),
                    TimelineCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AudienceCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BudgetAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReasonCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CancelNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborRequests_IndorNeighborRequestCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "IndorNeighborRequestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndorNeighborRequests_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdminHomecarePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdministratorId = table.Column<int>(type: "int", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    HomesCovered = table.Column<int>(type: "int", nullable: false),
                    NextDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToneClass = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdminHomecarePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorPropertyAdminHomecarePlans_IndorPropertyAdministrators_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "IndorPropertyAdministrators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdminPortfolioProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdministratorId = table.Column<int>(type: "int", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PropertyType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdminPortfolioProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorPropertyAdminPortfolioProperties_IndorPropertyAdministrators_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "IndorPropertyAdministrators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndorPropertyAdminPortfolioProperties_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdminPreventivePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdministratorId = table.Column<int>(type: "int", nullable: false),
                    PortfolioPropertyId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlanTier = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BundlePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SelectedServicesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PreferredTiming = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PreferredDay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntryAccess = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UpdateRecipients = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AutoReminders = table.Column<bool>(type: "bit", nullable: false),
                    NextVisitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdminPreventivePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorPropertyAdminPreventivePlans_IndorPropertyAdministrators_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "IndorPropertyAdministrators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdminScheduledVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdministratorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeWindow = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdminScheduledVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorPropertyAdminScheduledVisits_IndorPropertyAdministrators_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "IndorPropertyAdministrators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorPropertyAdminServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdministratorId = table.Column<int>(type: "int", nullable: false),
                    PortfolioPropertyId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ScheduledUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EtaLabel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TeamLabel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsEmergency = table.Column<bool>(type: "bit", nullable: false),
                    DetailsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TechnicianName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TechnicianRating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TechnicianTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    VehicleLabel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TimelineStep = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorPropertyAdminServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorPropertyAdminServiceRequests_IndorPropertyAdministrators_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "IndorPropertyAdministrators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorCategoriasSel",
                columns: table => new
                {
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorCategoriasSel", x => new { x.ProveedorId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_IndorProveedorCategoriasSel_IndorProveedorCategoriasCatalogo_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "IndorProveedorCategoriasCatalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndorProveedorCategoriasSel_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CustomerType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PreferredContactMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CityState = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AptUnit = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    City = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    State = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PropertyType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    IsBillingAddressSame = table.Column<bool>(type: "bit", nullable: false),
                    PropertyPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessNotes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    EstimateDeliveryPref = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InvoiceDeliveryPref = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CustomerSource = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TagsJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SendIndorInvite = table.Column<bool>(type: "bit", nullable: false),
                    AllowServiceUpdates = table.Column<bool>(type: "bit", nullable: false),
                    ConnectionStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPropertyVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsAppConnected = table.Column<bool>(type: "bit", nullable: false),
                    PropertiesCount = table.Column<int>(type: "int", nullable: false),
                    HouseFactsCount = table.Column<int>(type: "int", nullable: false),
                    LastActivityNote = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MemberSince = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorClientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorClientes_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorDocumentos_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorExamRespuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    TradeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QuestionNumber = table.Column<int>(type: "int", nullable: false),
                    SelectedIndex = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AnsweredUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorExamRespuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorExamRespuestas_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorLeads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Urgency = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LeadCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsHomeownerVerified = table.Column<bool>(type: "bit", nullable: false),
                    ProblemDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhotosJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DistanceMiles = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    TimelineNote = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HomeType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SquareFeet = table.Column<int>(type: "int", nullable: true),
                    YearBuilt = table.Column<int>(type: "int", nullable: true),
                    Stories = table.Column<int>(type: "int", nullable: true),
                    AccessNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuggestedScopeItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuggestedLaborAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    SuggestedMaterialsAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    SuggestedTimeline = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SuggestedWarranty = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SuggestedHomeownerNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DefaultVisitType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DefaultAssignedTechnician = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DefaultVisitNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DefaultVisitAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DefaultVisitTimeLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DefaultChecklistJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultMaterialsUsedJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DefaultLaborWarranty = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RealtorQuoteId = table.Column<int>(type: "int", nullable: true),
                    LeadSource = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    InspectionReportUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FindingsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnalysisSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorLeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorLeads_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorReportTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Badge = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorReportTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorReportTemplates_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorVerificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LicenseVerified = table.Column<bool>(type: "bit", nullable: false),
                    LicenseExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsuranceVerified = table.Column<bool>(type: "bit", nullable: false),
                    InsuranceExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    W9Verified = table.Column<bool>(type: "bit", nullable: false),
                    BackgroundStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OperatorNotes = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    FollowUpNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReviewerName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ApprovedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorVerificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorVerificaciones_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProviderInsuranceQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    QuoteCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Coverages = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Trade = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BusinessAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    State = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OwnerName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    OwnerDateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    OwnerPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OwnerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NumberOfEmployees = table.Column<int>(type: "int", nullable: true),
                    EmployeePayroll = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompanyGrossRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    YearsInBusiness = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WorksAtCustomerHomes = table.Column<bool>(type: "bit", nullable: true),
                    UsesSubcontractors = table.Column<bool>(type: "bit", nullable: true),
                    NeedsCOI = table.Column<bool>(type: "bit", nullable: true),
                    PayTodayAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CardLast4 = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    AutoPayMonthly = table.Column<bool>(type: "bit", nullable: true),
                    FirstBillingDate = table.Column<DateTime>(type: "date", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PaymentAuthorized = table.Column<bool>(type: "bit", nullable: false),
                    PaidUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ConfirmedAccurate = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubmittedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProviderInsuranceQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProviderInsuranceQuotes_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorOfertasSel",
                columns: table => new
                {
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    OfertaId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorOfertasSel", x => new { x.ProveedorId, x.OfertaId });
                    table.ForeignKey(
                        name: "FK_IndorProveedorOfertasSel_IndorProveedorOfertasCatalogo_OfertaId",
                        column: x => x.OfertaId,
                        principalTable: "IndorProveedorOfertasCatalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndorProveedorOfertasSel_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorNearbyNetworkSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    CenterLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CenterAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CenterLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CenterLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    RadiusMiles = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNearbyNetworkSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNearbyNetworkSettings_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CategoryTag = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OccurredUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorActivities_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ClientRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PropertyAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StatusSummary = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LastActiveUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorClients_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorDocumentos_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorInspectionUploadDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PropertyFileId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CityRegion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportFileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportFileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReportPageCount = table.Column<int>(type: "int", nullable: false),
                    UploadMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AnalysisProgress = table.Column<int>(type: "int", nullable: false),
                    AnalysisStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResponseDeadlineHours = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnalysisSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorInspectionUploadDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorInspectionUploadDrafts_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorPropertyFileDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourcePropertyId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CityRegion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilePhase = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    NoteText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreateAndContinueLater = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorPropertyFileDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorPropertyFileDrafts_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorPropertyFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CityRegion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    StateCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    PropertyType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Beds = table.Column<int>(type: "int", nullable: true),
                    Baths = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    SqFt = table.Column<int>(type: "int", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FilePhase = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RepairItemsCount = table.Column<int>(type: "int", nullable: false),
                    QuotesReceivedCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorPropertyFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorPropertyFiles_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorQuoteRequestDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PropertyFileId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CityRegion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilePhase = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SharePhotosVideos = table.Column<bool>(type: "bit", nullable: false),
                    ShareInspectionReport = table.Column<bool>(type: "bit", nullable: false),
                    ShareRepairItems = table.Column<bool>(type: "bit", nullable: false),
                    ShareNotes = table.Column<bool>(type: "bit", nullable: false),
                    ResponseDeadlineHours = table.Column<int>(type: "int", nullable: false),
                    ProviderSelectionMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProviderCountTarget = table.Column<int>(type: "int", nullable: false),
                    VerifiedOnly = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CoverageMiles = table.Column<int>(type: "int", nullable: false),
                    SendNow = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledSendUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllowProviderQuestions = table.Column<bool>(type: "bit", nullable: false),
                    AllowFullProjectQuote = table.Column<bool>(type: "bit", nullable: false),
                    AllowItemizedQuote = table.Column<bool>(type: "bit", nullable: false),
                    OptionalMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorQuoteRequestDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorQuoteRequestDrafts_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    QuoteCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    RequestedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProviderQuotesReceived = table.Column<int>(type: "int", nullable: false),
                    FooterNote = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    PropertyFileId = table.Column<int>(type: "int", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ResponseDeadlineHours = table.Column<int>(type: "int", nullable: true),
                    ProviderSelectionMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OptionalMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelectedBidId = table.Column<int>(type: "int", nullable: true),
                    AcceptedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorQuotes_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorSharedPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SharedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorSharedPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorSharedPackages_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorUrgentQuoteDrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PropertyFileId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CityRegion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Beds = table.Column<int>(type: "int", nullable: true),
                    Baths = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    SqFt = table.Column<int>(type: "int", nullable: true),
                    RequestCategory = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UrgencyLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuickDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestTypeTag = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OptionalNote = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ProviderSelectionMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SendPayload = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NotifyClient = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorUrgentQuoteDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorUrgentQuoteDrafts_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    BajoContrato = table.Column<bool>(type: "bit", nullable: false),
                    FechaCierreEstimada = table.Column<DateTime>(type: "date", nullable: true),
                    TieneReporteExistente = table.Column<bool>(type: "bit", nullable: false),
                    RolComprador = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ObjetivoPrincipal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotasRevision = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccion_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccion_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccion_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionCompleta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MotivoInspeccion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AreasEnfoque = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TamanoPropiedad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsUrgente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionCompleta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionCompleta_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionCompleta_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionCompleta_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionElectrica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MotivoRevision = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PreocupacionPrincipal = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OcurreAhora = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionElectrica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionElectrica_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionElectrica_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionElectrica_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionHomeSafety",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TiposProblema = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AreasAtencion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UbicacionProblema = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RiesgoActivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotivosRevision = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MotivoRevision = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroPisos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AreasEnfoque = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccesoPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OcupantesHogar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionHomeSafety", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionHomeSafety_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionHomeSafety_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionHomeSafety_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionHvac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ParteAtencion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SistemaFuncionando = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoEquipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CantidadSistemas = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ComponentesRevision = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EdadSistema = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FiltroCambiado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoTermostato = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DescripcionProblema = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotasOpcionales = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionHvac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionHvac_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionHvac_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionHvac_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionInvestor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoInversion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EnfoquesInversion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ocupacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NivelRehab = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AreasRevision = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccesoPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionInvestor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionInvestor_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionInvestor_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionInvestor_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionMoldMoisture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TiposProblema = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UbicacionProblema = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HumedadActiva = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotivoRevision = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UbicacionPrincipal = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IntrusionAguaReciente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccesoPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AreasEnfoque = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionMoldMoisture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionMoldMoisture_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionMoldMoisture_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionMoldMoisture_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionPlomeria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UbicacionProblema = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FugaAguaAhora = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SituacionesActuales = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CuandoEmpezo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AguaCerrada = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DescripcionProblema = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotasAdicionales = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionPlomeria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionPlomeria_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionPlomeria_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionPlomeria_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionRoof",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TiposProblema = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UbicacionProblema = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotivoRevision = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroPisos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaterialTecho = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccesoPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AreasEnfoque = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionRoof", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionRoof_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionRoof_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionRoof_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionStructural",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MotivoRevision = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TipoPreocupacion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TiposPreocupacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AreaPreocupacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DanoVisible = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SignosVisibles = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SeveridadApariencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UbicacionEspecifica = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CuandoNotadoTexto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DuracionProblema = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Severidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReparacionesPrevias = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CondicionesInseguras = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MejorHorarioVisita = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoFundacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TieneReporte = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CambiosRecientes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccesoPreferido = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AreasEnfoque = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CuandoNotado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EdadPropiedad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RemodelReciente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DescripcionProblema = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotasOpcionales = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionStructural", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionStructural_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionStructural_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionStructural_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesInspeccionWindowsInsulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InspeccionId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TiposProblema = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AreasAtencion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UbicacionProblema = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DanoHumedadVisible = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotivosRevision = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MotivoRevision = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroPisos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AreasEnfoque = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccesoPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoVentana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccesoAtticCrawlSpace = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ComentariosProveedor = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCitaProgramada = table.Column<DateTime>(type: "date", nullable: true),
                    HoraCitaProgramada = table.Column<TimeSpan>(type: "time", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesInspeccionWindowsInsulation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionWindowsInsulation_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionWindowsInsulation_Inspecciones_InspeccionId",
                        column: x => x.InspeccionId,
                        principalTable: "Inspecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesInspeccionWindowsInsulation_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MetodoPagoId = table.Column<int>(type: "int", nullable: true),
                    Cuotas = table.Column<int>(type: "int", nullable: true),
                    CuotasPagadas = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pagos_MetodosPago_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalTable: "MetodosPago",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CleaningProServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningProServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleaningProServicioLanding_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LawnCatalogOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    OptionGroup = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RequiresQuote = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawnCatalogOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawnCatalogOptions_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LawnServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ReminderBannerTitulo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ReminderBannerTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReminderDefaultOn = table.Column<bool>(type: "bit", nullable: false),
                    RemindOnlyCtaTexto = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawnServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawnServicioLanding_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgramacionesMicroservicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    FechaProgramada = table.Column<DateTime>(type: "date", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramacionesMicroservicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramacionesMicroservicio_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramacionesMicroservicio_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramacionesMicroservicio_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SafeAirServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaScheduleTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CtaChangedTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafeAirServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafeAirServicioLanding_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesCleaningPro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Frecuencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CantidadLimpiadores = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AreasLimpieza = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HorasEstimadas = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    AddonsSeleccionados = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    NotasLimpiador = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaServicio = table.Column<DateTime>(type: "date", nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TarifaHoraria = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ImpuestoVenta = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PrecioTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCleaningPro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCleaningPro_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesCleaningPro_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCleaningPro_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesLawn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoServicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Frecuencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AreaServicio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AddonsSeleccionados = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PreferenciaExtra = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaPreferida = table.Column<DateTime>(type: "date", nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PrecioBase = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PrecioAddons = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DescuentoSuscripcion = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PrecioTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModoServicio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordatorioActivo = table.Column<bool>(type: "bit", nullable: false),
                    RecordatorioAvisoDias = table.Column<int>(type: "int", nullable: false),
                    RecordatorioCanales = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProximoRecordatorioUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesLawn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesLawn_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesLawn_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesLawn_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesSafeAir",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    TipoNecesidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CantidadFiltros = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FiltroAncho = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FiltroAlto = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FiltroProfundidad = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FiltroTamanioDesconocido = table.Column<bool>(type: "bit", nullable: false),
                    UbicacionFiltro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProveedorFiltro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RecordatorioActivo = table.Column<bool>(type: "bit", nullable: false),
                    VentanaTiempo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DetallesAcceso = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    NotasAcceso = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaProximoRecordatorio = table.Column<DateTime>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesSafeAir", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesSafeAir_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesSafeAir_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesSafeAir_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesTrash",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BinsSeleccionados = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CantidadBins = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Frecuencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaRecoleccion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TipoAyuda = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RecordatorioCuando = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VentanaRecoleccion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NotasEspeciales = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioMensual = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesTrash", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesTrash_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesTrash_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesTrash_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TrashServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MicroservicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioTexto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CtaTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrashServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrashServicioLanding_Microservicios_MicroservicioId",
                        column: x => x.MicroservicioId,
                        principalTable: "Microservicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CleaningServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BestForLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestForOptions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BestForIcons = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BestForValues = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InfoBoxTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CtaContinueTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CtaUploadTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PrecioBaseEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DisclaimerTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleaningServicioLanding_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FurnitureAssemblyServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BadgesTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BadgesIconos = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstimatedTimeLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EstimatedTimeValue = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EstimatedTimeNote = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BestForLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestForValue = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BestForNote = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CtaContinueTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CtaUploadTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PrecioBaseEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DisclaimerTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FurnitureAssemblyServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FurnitureAssemblyServicioLanding_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovingServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EstimatedTimeLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EstimatedTimeValue = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EstimatedTimeNote = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BestForLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestForValue = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BestForNote = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    MoveTypes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoveTypeIcons = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoveTypeValues = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CtaContinueTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CtaEstimateTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PrecioEstimadoMin = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioEstimadoMax = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DuracionEstimadaMinHoras = table.Column<int>(type: "int", nullable: false),
                    DuracionEstimadaMaxHoras = table.Column<int>(type: "int", nullable: false),
                    DisclaimerTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovingServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovingServicioLanding_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackingServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BestForLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestForOptions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BestForIcons = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BestForValues = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EstimatedTimeLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EstimatedTimeValue = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestTimingLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestTimingValue = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CtaContinueTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CtaUploadTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PrecioBaseEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DisclaimerTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackingServicioLanding_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesCleaning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoLimpieza = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NumeroHabitaciones = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroBanos = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CondicionActual = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaServicio = table.Column<DateTime>(type: "date", nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AreasPrioridad = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TareasExtra = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuministrosNecesarios = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MetodoAcceso = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCleaning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCleaning_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesCleaning_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCleaning_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesFurnitureAssembly",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TiposMueble = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CantidadItems = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CondicionItems = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AnclajePared = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Habitacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DetallesAcceso = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AyudaMover = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaServicio = table.Column<DateTime>(type: "date", nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesFurnitureAssembly", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesFurnitureAssembly_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesFurnitureAssembly_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesFurnitureAssembly_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesGeneralHelp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoAyuda = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    VentanaTiempo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactoPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotasAcceso = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesGeneralHelp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesGeneralHelp_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesGeneralHelp_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesGeneralHelp_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesMoving",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TamanoHogar = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DireccionOrigen = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DireccionDestino = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "date", nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    TipoServicio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ItemsMover = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TamanoMovimiento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CondicionesAcceso = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RequiereMontaje = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioEstimadoMin = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PrecioEstimadoMax = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DuracionEstimadaMinHoras = table.Column<int>(type: "int", nullable: true),
                    DuracionEstimadaMaxHoras = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesMoving", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesMoving_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesMoving_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesMoving_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesPacking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoEmpaque = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CuandoMudanza = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoPropiedad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TamanoHogar = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaServicio = table.Column<DateTime>(type: "date", nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    HabitacionesEmpacar = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ItemsEspeciales = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    SuministrosNecesarios = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DetallesAcceso = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPacking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesPacking_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesPacking_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesPacking_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesTvWallMounting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoSolicitud = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TamanoTv = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CantidadTvs = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Habitacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoPared = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TieneSoporte = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ConfiguracionCables = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TomaCercana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MontajePrevio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DetallesAcceso = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    VentanaHorario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaServicio = table.Column<DateTime>(type: "date", nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesTvWallMounting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesTvWallMounting_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesTvWallMounting_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesTvWallMounting_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TvWallMountingServicioLanding",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LandingTitulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingTagline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LandingSubtitulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioDesde = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IncluyeItems = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncluyeIconos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BestForLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestForOptions = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BestForIcons = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BestForValues = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InfoBoxTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EstimatedTimeLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EstimatedTimeValue = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestTimingLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    BestTimingValue = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CtaContinueTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CtaUploadTexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PrecioBaseEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DisclaimerTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TvWallMountingServicioLanding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TvWallMountingServicioLanding_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembresiasUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanMembresiaId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembresiasUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembresiasUsuario_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembresiasUsuario_PlanesMembresia_PlanMembresiaId",
                        column: x => x.PlanMembresiaId,
                        principalTable: "PlanesMembresia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadHistorial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: false),
                    RecordType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PropiedadProveedorId = table.Column<int>(type: "int", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WarrantyStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadHistorial_PropiedadProveedores_PropiedadProveedorId",
                        column: x => x.PropiedadProveedorId,
                        principalTable: "PropiedadProveedores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropiedadHistorial_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadMantenimiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropiedadId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PropiedadProveedorId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadMantenimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropiedadMantenimiento_PropiedadProveedores_PropiedadProveedorId",
                        column: x => x.PropiedadProveedorId,
                        principalTable: "PropiedadProveedores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropiedadMantenimiento_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesRemodelingServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AlcanceProyecto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    VentanaTiempo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PresupuestoEstimado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactoPreferido = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesRemodelingServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesRemodelingServicio_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesRemodelingServicio_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitudesRemodelingServicio_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaElectrical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PuedeApagarBreaker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UbicacionProblema = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SintomasNotados = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EnergiaEncendida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PuedeAlejarse = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AceptaTextos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaElectrical", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaElectrical_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaElectrical_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaElectrical_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaFlood",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CausaAgua = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UbicacionAgua = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AguaActiva = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PuedeCerrarAgua = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UbicacionCierreAgua = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PuedeApagarElectricidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CantidadAgua = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaFlood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaFlood_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaFlood_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaFlood_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaHvac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SucedeAhora = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PuedeLlamarYa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EnCasaAhora = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaHvac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaHvac_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaHvac_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaHvac_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaPlomeria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AguaFluyendo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PuedeCerrarAgua = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AccesoSiAusente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaPlomeria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaPlomeria_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaPlomeria_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaPlomeria_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaRoofLeak",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UbicacionFuga = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PuedeColocarCubeta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaRoofLeak", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaRoofLeak_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaRoofLeak_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaRoofLeak_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaSmokeDetector",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TiposProblema = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UbicacionesDetectores = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SituacionActual = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PuedePermanecerAdentro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccesoPropiedad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaSmokeDetector", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaSmokeDetector_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaSmokeDetector_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaSmokeDetector_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaTreeDamage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UbicacionDanio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeligroInmediato = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RiesgoUtilidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccesoCasa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EntradaBloqueada = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PuedeAlejarse = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaTreeDamage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaTreeDamage_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaTreeDamage_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaTreeDamage_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesEmergenciaWaterHeater",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServicioEmergenciaId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TiposProblema = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TipoProblema = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Urgencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnidadFuncionando = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UbicacionProblema = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoUnidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SintomasVisibles = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NotaCorta = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DetallesAcceso = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesEmergenciaWaterHeater", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaWaterHeater_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaWaterHeater_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesEmergenciaWaterHeater_ServiciosEmergencia_ServicioEmergenciaId",
                        column: x => x.ServicioEmergenciaId,
                        principalTable: "ServiciosEmergencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesUtilitiesSetup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovingSetupServicioId = table.Column<int>(type: "int", nullable: false),
                    PropiedadId = table.Column<int>(type: "int", nullable: true),
                    DireccionPropiedad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ServiciosConectar = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaServicio = table.Column<DateTime>(type: "date", nullable: true),
                    PreferenciaContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ProveedorInternetId = table.Column<int>(type: "int", nullable: true),
                    OpcionCable = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OmitirInternet = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesUtilitiesSetup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesUtilitiesSetup_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesUtilitiesSetup_MovingSetupServicios_MovingSetupServicioId",
                        column: x => x.MovingSetupServicioId,
                        principalTable: "MovingSetupServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesUtilitiesSetup_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitudesUtilitiesSetup_UtilitiesSetupProveedorInternet_ProveedorInternetId",
                        column: x => x.ProveedorInternetId,
                        principalTable: "UtilitiesSetupProveedorInternet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosExteriorPaint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudExteriorPaintId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoriaFoto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosExteriorPaint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosExteriorPaint_SolicitudesExteriorPaint_SolicitudExteriorPaintId",
                        column: x => x.SolicitudExteriorPaintId,
                        principalTable: "SolicitudesExteriorPaint",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosGutterCleaning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudGutterCleaningId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosGutterCleaning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosGutterCleaning_SolicitudesGutterCleaning_SolicitudGutterCleaningId",
                        column: x => x.SolicitudGutterCleaningId,
                        principalTable: "SolicitudesGutterCleaning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosHvacMaintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudHvacMaintenanceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosHvacMaintenance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosHvacMaintenance_SolicitudesHvacMaintenance_SolicitudHvacMaintenanceId",
                        column: x => x.SolicitudHvacMaintenanceId,
                        principalTable: "SolicitudesHvacMaintenance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosPowerWash",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudPowerWashId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosPowerWash", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosPowerWash_SolicitudesPowerWash_SolicitudPowerWashId",
                        column: x => x.SolicitudPowerWashId,
                        principalTable: "SolicitudesPowerWash",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosRoofInspection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudRoofInspectionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosRoofInspection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosRoofInspection_SolicitudesRoofInspection_SolicitudRoofInspectionId",
                        column: x => x.SolicitudRoofInspectionId,
                        principalTable: "SolicitudesRoofInspection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosWaterHeaterFlush",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudWaterHeaterFlushId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosWaterHeaterFlush", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosWaterHeaterFlush_SolicitudesWaterHeaterFlush_SolicitudWaterHeaterFlushId",
                        column: x => x.SolicitudWaterHeaterFlushId,
                        principalTable: "SolicitudesWaterHeaterFlush",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborRequestOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    OfferType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: true),
                    ResponderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    OffererName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    OffererPhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriceAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    DistanceMiles = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    ScheduleLabel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborRequestOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborRequestOffers_IndorNeighborRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "IndorNeighborRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorNeighborRequestPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNeighborRequestPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNeighborRequestPhotos_IndorNeighborRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "IndorNeighborRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    JobCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ChecklistStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PhotosCount = table.Column<int>(type: "int", nullable: false),
                    HouseFactsStatus = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ViewedByCustomer = table.Column<bool>(type: "bit", nullable: false),
                    EstimateAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    EstimateCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    InvoiceStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DistanceMiles = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    ScopeOfWork = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MaterialsNeeded = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AccessInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    JobNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssignedTechnician = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChecklistJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterialsUsedJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PhotoUrlsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HomeownerSignature = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HomeownerSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WorkPerformed = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LaborWarranty = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FinalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReminderSetting = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AddToCalendar = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorJobs_IndorProveedorClientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "IndorProveedorClientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorJobs_IndorProveedorLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "IndorProveedorLeads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorJobs_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorReportTemplateSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsIncluded = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorReportTemplateSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorReportTemplateSections_IndorProveedorReportTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "IndorProveedorReportTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorNearbyNetworkItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerRealtorId = table.Column<int>(type: "int", nullable: true),
                    CardType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FilterCategory = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BadgeLabel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BadgeCss = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Bedrooms = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    Bathrooms = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    SquareFeet = table.Column<int>(type: "int", nullable: true),
                    SpecsLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    MetaLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TagsJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DistanceMiles = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    StatusBadge = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StatusCss = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PrimaryActionLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PrimaryActionUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SecondaryActionLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    SecondaryActionUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOwnedListing = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RelatedClientId = table.Column<int>(type: "int", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorNearbyNetworkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorNearbyNetworkItems_IndorRealtorClients_RelatedClientId",
                        column: x => x.RelatedClientId,
                        principalTable: "IndorRealtorClients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorNearbyNetworkItems_IndorRealtors_OwnerRealtorId",
                        column: x => x.OwnerRealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorInspectionDraftProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(type: "int", nullable: false),
                    Trade = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorInspectionDraftProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorInspectionDraftProviders_IndorRealtorInspectionUploadDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "IndorRealtorInspectionUploadDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorInspectionUploadFindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Trade = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TradeLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AiScore = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceExcerpt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceSection = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SourceSectionNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SourcePage = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorInspectionUploadFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorInspectionUploadFindings_IndorRealtorInspectionUploadDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "IndorRealtorInspectionUploadDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorPropertyFileDraftCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(type: "int", nullable: false),
                    CategoryType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorPropertyFileDraftCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorPropertyFileDraftCategories_IndorRealtorPropertyFileDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "IndorRealtorPropertyFileDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorPropertyFileDraftItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(type: "int", nullable: false),
                    CategoryType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ItemLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoteText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ExpirationUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorPropertyFileDraftItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorPropertyFileDraftItems_IndorRealtorPropertyFileDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "IndorRealtorPropertyFileDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    InvitationToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ClientRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QuickNote = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PropertyFileId = table.Column<int>(type: "int", nullable: true),
                    PropertyAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PropertyLabel = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PropertyCityRegion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PropertyStatusLabel = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AccessPropertyOverview = table.Column<bool>(type: "bit", nullable: false),
                    AccessFilesReports = table.Column<bool>(type: "bit", nullable: false),
                    AccessQuotesEstimates = table.Column<bool>(type: "bit", nullable: false),
                    AccessMessages = table.Column<bool>(type: "bit", nullable: false),
                    AccessProjectUpdates = table.Column<bool>(type: "bit", nullable: false),
                    AccessPayments = table.Column<bool>(type: "bit", nullable: false),
                    CollaborationLevel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DeliveryEmail = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryText = table.Column<bool>(type: "bit", nullable: false),
                    WelcomeMessage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SendReminder48h = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorInvitations_IndorRealtorPropertyFiles_PropertyFileId",
                        column: x => x.PropertyFileId,
                        principalTable: "IndorRealtorPropertyFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorRealtorInvitations_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorPropertyFileItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyFileId = table.Column<int>(type: "int", nullable: false),
                    CategoryType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ItemLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoteText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ExpirationUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorPropertyFileItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorPropertyFileItems_IndorRealtorPropertyFiles_PropertyFileId",
                        column: x => x.PropertyFileId,
                        principalTable: "IndorRealtorPropertyFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorQuoteRequestDraftProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorQuoteRequestDraftProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorQuoteRequestDraftProviders_IndorRealtorQuoteProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "IndorRealtorQuoteProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndorRealtorQuoteRequestDraftProviders_IndorRealtorQuoteRequestDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "IndorRealtorQuoteRequestDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorQuoteBids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(2,1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: true),
                    EstimateId = table.Column<int>(type: "int", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    SubmittedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorQuoteBids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorQuoteBids_IndorRealtorQuotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "IndorRealtorQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorQuoteSentProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorQuoteSentProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorQuoteSentProviders_IndorRealtorQuotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "IndorRealtorQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorUrgentQuoteDraftPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftId = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorUrgentQuoteDraftPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorUrgentQuoteDraftPhotos_IndorRealtorUrgentQuoteDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "IndorRealtorUrgentQuoteDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosReporteInspeccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosReporteInspeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosReporteInspeccion_SolicitudesInspeccion_SolicitudInspeccionId",
                        column: x => x.SolicitudInspeccionId,
                        principalTable: "SolicitudesInspeccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionCompleta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionCompletaId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionCompleta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionCompleta_SolicitudesInspeccionCompleta_SolicitudInspeccionCompletaId",
                        column: x => x.SolicitudInspeccionCompletaId,
                        principalTable: "SolicitudesInspeccionCompleta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionElectrica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionElectricaId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionElectrica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionElectrica_SolicitudesInspeccionElectrica_SolicitudInspeccionElectricaId",
                        column: x => x.SolicitudInspeccionElectricaId,
                        principalTable: "SolicitudesInspeccionElectrica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionHomeSafety",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionHomeSafetyId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionHomeSafety", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionHomeSafety_SolicitudesInspeccionHomeSafety_SolicitudInspeccionHomeSafetyId",
                        column: x => x.SolicitudInspeccionHomeSafetyId,
                        principalTable: "SolicitudesInspeccionHomeSafety",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionHvac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionHvacId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionHvac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionHvac_SolicitudesInspeccionHvac_SolicitudInspeccionHvacId",
                        column: x => x.SolicitudInspeccionHvacId,
                        principalTable: "SolicitudesInspeccionHvac",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionInvestor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionInvestorId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionInvestor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionInvestor_SolicitudesInspeccionInvestor_SolicitudInspeccionInvestorId",
                        column: x => x.SolicitudInspeccionInvestorId,
                        principalTable: "SolicitudesInspeccionInvestor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionMoldMoisture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionMoldMoistureId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionMoldMoisture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionMoldMoisture_SolicitudesInspeccionMoldMoisture_SolicitudInspeccionMoldMoistureId",
                        column: x => x.SolicitudInspeccionMoldMoistureId,
                        principalTable: "SolicitudesInspeccionMoldMoisture",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionPlomeria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionPlomeriaId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionPlomeria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionPlomeria_SolicitudesInspeccionPlomeria_SolicitudInspeccionPlomeriaId",
                        column: x => x.SolicitudInspeccionPlomeriaId,
                        principalTable: "SolicitudesInspeccionPlomeria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionRoof",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionRoofId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionRoof", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionRoof_SolicitudesInspeccionRoof_SolicitudInspeccionRoofId",
                        column: x => x.SolicitudInspeccionRoofId,
                        principalTable: "SolicitudesInspeccionRoof",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionStructural",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionStructuralId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionStructural", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionStructural_SolicitudesInspeccionStructural_SolicitudInspeccionStructuralId",
                        column: x => x.SolicitudInspeccionStructuralId,
                        principalTable: "SolicitudesInspeccionStructural",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosInspeccionWindowsInsulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudInspeccionWindowsInsulationId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosInspeccionWindowsInsulation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosInspeccionWindowsInsulation_SolicitudesInspeccionWindowsInsulation_SolicitudInspeccionWindowsInsulationId",
                        column: x => x.SolicitudInspeccionWindowsInsulationId,
                        principalTable: "SolicitudesInspeccionWindowsInsulation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosSafeAir",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudSafeAirId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosSafeAir", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosSafeAir_SolicitudesSafeAir_SolicitudSafeAirId",
                        column: x => x.SolicitudSafeAirId,
                        principalTable: "SolicitudesSafeAir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosCleaning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudCleaningId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosCleaning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosCleaning_SolicitudesCleaning_SolicitudCleaningId",
                        column: x => x.SolicitudCleaningId,
                        principalTable: "SolicitudesCleaning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosFurnitureAssembly",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudFurnitureAssemblyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosFurnitureAssembly", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosFurnitureAssembly_SolicitudesFurnitureAssembly_SolicitudFurnitureAssemblyId",
                        column: x => x.SolicitudFurnitureAssemblyId,
                        principalTable: "SolicitudesFurnitureAssembly",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosGeneralHelp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudGeneralHelpId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosGeneralHelp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosGeneralHelp_SolicitudesGeneralHelp_SolicitudGeneralHelpId",
                        column: x => x.SolicitudGeneralHelpId,
                        principalTable: "SolicitudesGeneralHelp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosMoving",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudMovingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosMoving", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosMoving_SolicitudesMoving_SolicitudMovingId",
                        column: x => x.SolicitudMovingId,
                        principalTable: "SolicitudesMoving",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosPacking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudPackingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosPacking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosPacking_SolicitudesPacking_SolicitudPackingId",
                        column: x => x.SolicitudPackingId,
                        principalTable: "SolicitudesPacking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosTvWallMounting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudTvWallMountingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosTvWallMounting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosTvWallMounting_SolicitudesTvWallMounting_SolicitudTvWallMountingId",
                        column: x => x.SolicitudTvWallMountingId,
                        principalTable: "SolicitudesTvWallMounting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosRemodelingServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudRemodelingServicioId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosRemodelingServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosRemodelingServicio_SolicitudesRemodelingServicio_SolicitudRemodelingServicioId",
                        column: x => x.SolicitudRemodelingServicioId,
                        principalTable: "SolicitudesRemodelingServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmergenciaElectrical",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudEmergenciaElectricalId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosEmergenciaElectrical", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmergenciaElectrical_SolicitudesEmergenciaElectrical_SolicitudEmergenciaElectricalId",
                        column: x => x.SolicitudEmergenciaElectricalId,
                        principalTable: "SolicitudesEmergenciaElectrical",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmergenciaFlood",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudEmergenciaFloodId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosEmergenciaFlood", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmergenciaFlood_SolicitudesEmergenciaFlood_SolicitudEmergenciaFloodId",
                        column: x => x.SolicitudEmergenciaFloodId,
                        principalTable: "SolicitudesEmergenciaFlood",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmergenciaHvac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudEmergenciaHvacId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosEmergenciaHvac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmergenciaHvac_SolicitudesEmergenciaHvac_SolicitudEmergenciaHvacId",
                        column: x => x.SolicitudEmergenciaHvacId,
                        principalTable: "SolicitudesEmergenciaHvac",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmergenciaPlomeria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudEmergenciaPlomeriaId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosEmergenciaPlomeria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmergenciaPlomeria_SolicitudesEmergenciaPlomeria_SolicitudEmergenciaPlomeriaId",
                        column: x => x.SolicitudEmergenciaPlomeriaId,
                        principalTable: "SolicitudesEmergenciaPlomeria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmergenciaRoofLeak",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudEmergenciaRoofLeakId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosEmergenciaRoofLeak", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmergenciaRoofLeak_SolicitudesEmergenciaRoofLeak_SolicitudEmergenciaRoofLeakId",
                        column: x => x.SolicitudEmergenciaRoofLeakId,
                        principalTable: "SolicitudesEmergenciaRoofLeak",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmergenciaTreeDamage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudEmergenciaTreeDamageId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosEmergenciaTreeDamage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmergenciaTreeDamage_SolicitudesEmergenciaTreeDamage_SolicitudEmergenciaTreeDamageId",
                        column: x => x.SolicitudEmergenciaTreeDamageId,
                        principalTable: "SolicitudesEmergenciaTreeDamage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchivosEmergenciaWaterHeater",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudEmergenciaWaterHeaterId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CategoriaArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TamanioBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosEmergenciaWaterHeater", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosEmergenciaWaterHeater_SolicitudesEmergenciaWaterHeater_SolicitudEmergenciaWaterHeaterId",
                        column: x => x.SolicitudEmergenciaWaterHeaterId,
                        principalTable: "SolicitudesEmergenciaWaterHeater",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UtilitiesSetupContactos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudUtilitiesSetupId = table.Column<int>(type: "int", nullable: false),
                    TipoUtilidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IconoClase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilitiesSetupContactos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UtilitiesSetupContactos_SolicitudesUtilitiesSetup_SolicitudUtilitiesSetupId",
                        column: x => x.SolicitudUtilitiesSetupId,
                        principalTable: "SolicitudesUtilitiesSetup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorApprovals_IndorProveedorJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "IndorProveedorJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorApprovals_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnreadCount = table.Column<int>(type: "int", nullable: false),
                    LastMessagePreview = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCustomerOnline = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorConversations_IndorProveedorClientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "IndorProveedorClientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorConversations_IndorProveedorJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "IndorProveedorJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorConversations_IndorProveedorLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "IndorProveedorLeads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorConversations_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorEstimates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EstimateType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ServiceCategoryId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EstimatedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimateCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LaborAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MaterialsAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ScopeItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timeline = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Warranty = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HomeownerNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NotifyHomeowner = table.Column<bool>(type: "bit", nullable: false),
                    SaveCopyToLeads = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    EstimatedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedDuration = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LaborWarranty = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PartsWarranty = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ValidDays = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorEstimates_IndorProveedorClientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "IndorProveedorClientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorEstimates_IndorProveedorJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "IndorProveedorJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorEstimates_IndorProveedorLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "IndorProveedorLeads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorEstimates_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    ReportCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhotosCount = table.Column<int>(type: "int", nullable: false),
                    HasChecklist = table.Column<bool>(type: "bit", nullable: false),
                    HasWarranty = table.Column<bool>(type: "bit", nullable: false),
                    HasDocuments = table.Column<bool>(type: "bit", nullable: false),
                    AddedToHouseFacts = table.Column<bool>(type: "bit", nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WorkCompleted = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaterialsUsed = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WarrantyInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SendToHomeowner = table.Column<bool>(type: "bit", nullable: false),
                    RequestApproval = table.Column<bool>(type: "bit", nullable: false),
                    AttachToHouseFacts = table.Column<bool>(type: "bit", nullable: false),
                    PhotoUrlsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DocumentsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FilesCount = table.Column<int>(type: "int", nullable: false),
                    ReportDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreparedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    LocationDetail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Weather = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorReports_IndorProveedorClientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "IndorProveedorClientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorReports_IndorProveedorJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "IndorProveedorJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorReports_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorRealtorSharedQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealtorId = table.Column<int>(type: "int", nullable: false),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    BidId = table.Column<int>(type: "int", nullable: false),
                    ShareToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HomeownerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    HomeownerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    HomeownerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ShareProviderInfo = table.Column<bool>(type: "bit", nullable: false),
                    ShareFullPriceBreakdown = table.Column<bool>(type: "bit", nullable: false),
                    ShareScopeOfWork = table.Column<bool>(type: "bit", nullable: false),
                    ShareWarranty = table.Column<bool>(type: "bit", nullable: false),
                    ShareIncludedRepairs = table.Column<bool>(type: "bit", nullable: false),
                    ShareTimeline = table.Column<bool>(type: "bit", nullable: false),
                    PricingDisplayMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageToHomeowner = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorRealtorSharedQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorRealtorSharedQuotes_IndorRealtorQuoteBids_BidId",
                        column: x => x.BidId,
                        principalTable: "IndorRealtorQuoteBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndorRealtorSharedQuotes_IndorRealtorQuotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "IndorRealtorQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndorRealtorSharedQuotes_IndorRealtors_RealtorId",
                        column: x => x.RealtorId,
                        principalTable: "IndorRealtors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AttachmentLabel = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorMessages_IndorProveedorConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "IndorProveedorConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    EstimateId = table.Column<int>(type: "int", nullable: true),
                    LeadId = table.Column<int>(type: "int", nullable: true),
                    SentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    NotesToCustomer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LineItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastReminderUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReminderChannel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastReminderMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorInvoices_IndorProveedorEstimates_EstimateId",
                        column: x => x.EstimateId,
                        principalTable: "IndorProveedorEstimates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorInvoices_IndorProveedorJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "IndorProveedorJobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorInvoices_IndorProveedorLeads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "IndorProveedorLeads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndorProveedorInvoices_IndorProveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "IndorProveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndorProveedorReportPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndorProveedorReportPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndorProveedorReportPhotos_IndorProveedorReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "IndorProveedorReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosCleaning_SolicitudCleaningId",
                table: "ArchivosCleaning",
                column: "SolicitudCleaningId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmergenciaElectrical_SolicitudEmergenciaElectricalId",
                table: "ArchivosEmergenciaElectrical",
                column: "SolicitudEmergenciaElectricalId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmergenciaFlood_SolicitudEmergenciaFloodId",
                table: "ArchivosEmergenciaFlood",
                column: "SolicitudEmergenciaFloodId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmergenciaHvac_SolicitudEmergenciaHvacId",
                table: "ArchivosEmergenciaHvac",
                column: "SolicitudEmergenciaHvacId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmergenciaPlomeria_SolicitudEmergenciaPlomeriaId",
                table: "ArchivosEmergenciaPlomeria",
                column: "SolicitudEmergenciaPlomeriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmergenciaRoofLeak_SolicitudEmergenciaRoofLeakId",
                table: "ArchivosEmergenciaRoofLeak",
                column: "SolicitudEmergenciaRoofLeakId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmergenciaTreeDamage_SolicitudEmergenciaTreeDamageId",
                table: "ArchivosEmergenciaTreeDamage",
                column: "SolicitudEmergenciaTreeDamageId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosEmergenciaWaterHeater_SolicitudEmergenciaWaterHeaterId",
                table: "ArchivosEmergenciaWaterHeater",
                column: "SolicitudEmergenciaWaterHeaterId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosExteriorPaint_SolicitudExteriorPaintId",
                table: "ArchivosExteriorPaint",
                column: "SolicitudExteriorPaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosFurnitureAssembly_SolicitudFurnitureAssemblyId",
                table: "ArchivosFurnitureAssembly",
                column: "SolicitudFurnitureAssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosGeneralHelp_SolicitudGeneralHelpId",
                table: "ArchivosGeneralHelp",
                column: "SolicitudGeneralHelpId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosGutterCleaning_SolicitudGutterCleaningId",
                table: "ArchivosGutterCleaning",
                column: "SolicitudGutterCleaningId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosHvacMaintenance_SolicitudHvacMaintenanceId",
                table: "ArchivosHvacMaintenance",
                column: "SolicitudHvacMaintenanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionCompleta_SolicitudInspeccionCompletaId",
                table: "ArchivosInspeccionCompleta",
                column: "SolicitudInspeccionCompletaId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionElectrica_SolicitudInspeccionElectricaId",
                table: "ArchivosInspeccionElectrica",
                column: "SolicitudInspeccionElectricaId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionHomeSafety_SolicitudInspeccionHomeSafetyId",
                table: "ArchivosInspeccionHomeSafety",
                column: "SolicitudInspeccionHomeSafetyId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionHvac_SolicitudInspeccionHvacId",
                table: "ArchivosInspeccionHvac",
                column: "SolicitudInspeccionHvacId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionInvestor_SolicitudInspeccionInvestorId",
                table: "ArchivosInspeccionInvestor",
                column: "SolicitudInspeccionInvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionMoldMoisture_SolicitudInspeccionMoldMoistureId",
                table: "ArchivosInspeccionMoldMoisture",
                column: "SolicitudInspeccionMoldMoistureId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionPlomeria_SolicitudInspeccionPlomeriaId",
                table: "ArchivosInspeccionPlomeria",
                column: "SolicitudInspeccionPlomeriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionRoof_SolicitudInspeccionRoofId",
                table: "ArchivosInspeccionRoof",
                column: "SolicitudInspeccionRoofId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionStructural_SolicitudInspeccionStructuralId",
                table: "ArchivosInspeccionStructural",
                column: "SolicitudInspeccionStructuralId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosInspeccionWindowsInsulation_SolicitudInspeccionWindowsInsulationId",
                table: "ArchivosInspeccionWindowsInsulation",
                column: "SolicitudInspeccionWindowsInsulationId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosMoving_SolicitudMovingId",
                table: "ArchivosMoving",
                column: "SolicitudMovingId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosPacking_SolicitudPackingId",
                table: "ArchivosPacking",
                column: "SolicitudPackingId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosPowerWash_SolicitudPowerWashId",
                table: "ArchivosPowerWash",
                column: "SolicitudPowerWashId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosRemodelingServicio_SolicitudRemodelingServicioId",
                table: "ArchivosRemodelingServicio",
                column: "SolicitudRemodelingServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosReporteInspeccion_SolicitudInspeccionId",
                table: "ArchivosReporteInspeccion",
                column: "SolicitudInspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosRoofInspection_SolicitudRoofInspectionId",
                table: "ArchivosRoofInspection",
                column: "SolicitudRoofInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosSafeAir_SolicitudSafeAirId",
                table: "ArchivosSafeAir",
                column: "SolicitudSafeAirId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosTvWallMounting_SolicitudTvWallMountingId",
                table: "ArchivosTvWallMounting",
                column: "SolicitudTvWallMountingId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosWaterHeaterFlush_SolicitudWaterHeaterFlushId",
                table: "ArchivosWaterHeaterFlush",
                column: "SolicitudWaterHeaterFlushId");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningProServicioLanding_MicroservicioId",
                table: "CleaningProServicioLanding",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningServicioLanding_MovingSetupServicioId",
                table: "CleaningServicioLanding",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CrawlspaceCheckServicioLanding_HomeCarePriorityId",
                table: "CrawlspaceCheckServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_ExteriorPaintServicioLanding_HomeCarePriorityId",
                table: "ExteriorPaintServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_FurnitureAssemblyServicioLanding_MovingSetupServicioId",
                table: "FurnitureAssemblyServicioLanding",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_GutterCleaningServicioLanding_HomeCarePriorityId",
                table: "GutterCleaningServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialServicios_UserId",
                table: "HistorialServicios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HvacMaintenanceServicioLanding_HomeCarePriorityId",
                table: "HvacMaintenanceServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorNearbyNetworkItems_OwnerRealtorId",
                table: "IndorNearbyNetworkItems",
                column: "OwnerRealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorNearbyNetworkItems_RelatedClientId",
                table: "IndorNearbyNetworkItems",
                column: "RelatedClientId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorNearbyNetworkSettings_RealtorId",
                table: "IndorNearbyNetworkSettings",
                column: "RealtorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborRequestOffers_RequestId",
                table: "IndorNeighborRequestOffers",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborRequestPhotos_RequestId",
                table: "IndorNeighborRequestPhotos",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborRequests_CategoryId",
                table: "IndorNeighborRequests",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorNeighborRequests_PropiedadId",
                table: "IndorNeighborRequests",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorPropertyAdminHomecarePlans_AdministratorId",
                table: "IndorPropertyAdminHomecarePlans",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorPropertyAdministrators_UserId",
                table: "IndorPropertyAdministrators",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorPropertyAdminPortfolioProperties_AdministratorId",
                table: "IndorPropertyAdminPortfolioProperties",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorPropertyAdminPortfolioProperties_PropiedadId",
                table: "IndorPropertyAdminPortfolioProperties",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorPropertyAdminPreventivePlans_AdministratorId",
                table: "IndorPropertyAdminPreventivePlans",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorPropertyAdminScheduledVisits_AdministratorId",
                table: "IndorPropertyAdminScheduledVisits",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorPropertyAdminServiceRequests_AdministratorId",
                table: "IndorPropertyAdminServiceRequests",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorApprovals_JobId",
                table: "IndorProveedorApprovals",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorApprovals_ProveedorId",
                table: "IndorProveedorApprovals",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorCategoriasSel_CategoriaId",
                table: "IndorProveedorCategoriasSel",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorClientes_ProveedorId",
                table: "IndorProveedorClientes",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorConversations_ClienteId",
                table: "IndorProveedorConversations",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorConversations_JobId",
                table: "IndorProveedorConversations",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorConversations_LeadId",
                table: "IndorProveedorConversations",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorConversations_ProveedorId",
                table: "IndorProveedorConversations",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorDocumentos_ProveedorId",
                table: "IndorProveedorDocumentos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedores_RegistrationToken",
                table: "IndorProveedores",
                column: "RegistrationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedores_UserId",
                table: "IndorProveedores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorEstimates_ClienteId",
                table: "IndorProveedorEstimates",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorEstimates_JobId",
                table: "IndorProveedorEstimates",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorEstimates_LeadId",
                table: "IndorProveedorEstimates",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorEstimates_ProveedorId",
                table: "IndorProveedorEstimates",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorExamRespuestas_ProveedorId",
                table: "IndorProveedorExamRespuestas",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorInvoices_EstimateId",
                table: "IndorProveedorInvoices",
                column: "EstimateId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorInvoices_JobId",
                table: "IndorProveedorInvoices",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorInvoices_LeadId",
                table: "IndorProveedorInvoices",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorInvoices_ProveedorId",
                table: "IndorProveedorInvoices",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorJobs_ClienteId",
                table: "IndorProveedorJobs",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorJobs_LeadId",
                table: "IndorProveedorJobs",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorJobs_ProveedorId",
                table: "IndorProveedorJobs",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorLeads_ProveedorId",
                table: "IndorProveedorLeads",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorMessages_ConversationId",
                table: "IndorProveedorMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorOfertasSel_OfertaId",
                table: "IndorProveedorOfertasSel",
                column: "OfertaId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorReportPhotos_ReportId",
                table: "IndorProveedorReportPhotos",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorReports_ClienteId",
                table: "IndorProveedorReports",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorReports_JobId",
                table: "IndorProveedorReports",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorReports_ProveedorId",
                table: "IndorProveedorReports",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorReportTemplates_ProveedorId",
                table: "IndorProveedorReportTemplates",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorReportTemplateSections_TemplateId",
                table: "IndorProveedorReportTemplateSections",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProveedorVerificaciones_ProveedorId",
                table: "IndorProveedorVerificaciones",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorProviderInsuranceQuotes_ProveedorId",
                table: "IndorProviderInsuranceQuotes",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorActivities_RealtorId",
                table: "IndorRealtorActivities",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorClients_RealtorId",
                table: "IndorRealtorClients",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorDocumentos_RealtorId",
                table: "IndorRealtorDocumentos",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorInspectionDraftProviders_DraftId",
                table: "IndorRealtorInspectionDraftProviders",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorInspectionUploadDrafts_RealtorId",
                table: "IndorRealtorInspectionUploadDrafts",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorInspectionUploadFindings_DraftId",
                table: "IndorRealtorInspectionUploadFindings",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorInvitations_PropertyFileId",
                table: "IndorRealtorInvitations",
                column: "PropertyFileId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorInvitations_RealtorId",
                table: "IndorRealtorInvitations",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorPropertyFileDraftCategories_DraftId",
                table: "IndorRealtorPropertyFileDraftCategories",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorPropertyFileDraftItems_DraftId",
                table: "IndorRealtorPropertyFileDraftItems",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorPropertyFileDrafts_RealtorId",
                table: "IndorRealtorPropertyFileDrafts",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorPropertyFileItems_PropertyFileId",
                table: "IndorRealtorPropertyFileItems",
                column: "PropertyFileId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorPropertyFiles_RealtorId",
                table: "IndorRealtorPropertyFiles",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorQuoteBids_QuoteId",
                table: "IndorRealtorQuoteBids",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorQuoteRequestDraftProviders_DraftId",
                table: "IndorRealtorQuoteRequestDraftProviders",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorQuoteRequestDraftProviders_ProviderId",
                table: "IndorRealtorQuoteRequestDraftProviders",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorQuoteRequestDrafts_RealtorId",
                table: "IndorRealtorQuoteRequestDrafts",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorQuotes_RealtorId",
                table: "IndorRealtorQuotes",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorQuoteSentProviders_QuoteId",
                table: "IndorRealtorQuoteSentProviders",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtors_UserId",
                table: "IndorRealtors",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorSharedPackages_RealtorId",
                table: "IndorRealtorSharedPackages",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorSharedQuotes_BidId",
                table: "IndorRealtorSharedQuotes",
                column: "BidId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorSharedQuotes_QuoteId",
                table: "IndorRealtorSharedQuotes",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorSharedQuotes_RealtorId",
                table: "IndorRealtorSharedQuotes",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorUrgentQuoteDraftPhotos_DraftId",
                table: "IndorRealtorUrgentQuoteDraftPhotos",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_IndorRealtorUrgentQuoteDrafts_RealtorId",
                table: "IndorRealtorUrgentQuoteDrafts",
                column: "RealtorId");

            migrationBuilder.CreateIndex(
                name: "IX_LawnCatalogOptions_MicroservicioId",
                table: "LawnCatalogOptions",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_LawnServicioLanding_MicroservicioId",
                table: "LawnServicioLanding",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasUsuario_PlanMembresiaId",
                table: "MembresiasUsuario",
                column: "PlanMembresiaId");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasUsuario_UserId",
                table: "MembresiasUsuario",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesSoporte_UserId",
                table: "MensajesSoporte",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MetodosPago_UserId",
                table: "MetodosPago",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MovingServicioLanding_MovingSetupServicioId",
                table: "MovingServicioLanding",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingServicioLanding_MovingSetupServicioId",
                table: "PackingServicioLanding",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_MetodoPagoId",
                table: "Pagos",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UserId",
                table: "Pagos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PestControlServicioLanding_HomeCarePriorityId",
                table: "PestControlServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerWashServicioLanding_HomeCarePriorityId",
                table: "PowerWashServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramacionesMicroservicio_MicroservicioId",
                table: "ProgramacionesMicroservicio",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramacionesMicroservicio_PropiedadId",
                table: "ProgramacionesMicroservicio",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramacionesMicroservicio_UserId",
                table: "ProgramacionesMicroservicio",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadDocumentos_PropiedadId",
                table: "PropiedadDocumentos",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadHistorial_PropiedadId",
                table: "PropiedadHistorial",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadHistorial_PropiedadProveedorId",
                table: "PropiedadHistorial",
                column: "PropiedadProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadHvacSistemas_PropiedadId",
                table: "PropiedadHvacSistemas",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadMantenimiento_PropiedadId",
                table: "PropiedadMantenimiento",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadMantenimiento_PropiedadProveedorId",
                table: "PropiedadMantenimiento",
                column: "PropiedadProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadProveedores_PropiedadId",
                table: "PropiedadProveedores",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadWaterHeaterSistemas_PropiedadId",
                table: "PropiedadWaterHeaterSistemas",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_RoofInspectionServicioLanding_HomeCarePriorityId",
                table: "RoofInspectionServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeAirServicioLanding_MicroservicioId",
                table: "SafeAirServicioLanding",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SmokeDetectorServicioLanding_HomeCarePriorityId",
                table: "SmokeDetectorServicioLanding",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCleaning_MovingSetupServicioId",
                table: "SolicitudesCleaning",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCleaning_PropiedadId",
                table: "SolicitudesCleaning",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCleaning_UserId",
                table: "SolicitudesCleaning",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCleaningPro_MicroservicioId",
                table: "SolicitudesCleaningPro",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCleaningPro_PropiedadId",
                table: "SolicitudesCleaningPro",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCleaningPro_UserId",
                table: "SolicitudesCleaningPro",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCrawlspaceCheck_HomeCarePriorityId",
                table: "SolicitudesCrawlspaceCheck",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCrawlspaceCheck_PropiedadId",
                table: "SolicitudesCrawlspaceCheck",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCrawlspaceCheck_UserId",
                table: "SolicitudesCrawlspaceCheck",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaElectrical_PropiedadId",
                table: "SolicitudesEmergenciaElectrical",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaElectrical_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaElectrical",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaElectrical_UserId",
                table: "SolicitudesEmergenciaElectrical",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaFlood_PropiedadId",
                table: "SolicitudesEmergenciaFlood",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaFlood_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaFlood",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaFlood_UserId",
                table: "SolicitudesEmergenciaFlood",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaHvac_PropiedadId",
                table: "SolicitudesEmergenciaHvac",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaHvac_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaHvac",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaHvac_UserId",
                table: "SolicitudesEmergenciaHvac",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaPlomeria_PropiedadId",
                table: "SolicitudesEmergenciaPlomeria",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaPlomeria_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaPlomeria",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaPlomeria_UserId",
                table: "SolicitudesEmergenciaPlomeria",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaRoofLeak_PropiedadId",
                table: "SolicitudesEmergenciaRoofLeak",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaRoofLeak_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaRoofLeak",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaRoofLeak_UserId",
                table: "SolicitudesEmergenciaRoofLeak",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaSmokeDetector_PropiedadId",
                table: "SolicitudesEmergenciaSmokeDetector",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaSmokeDetector_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaSmokeDetector",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaSmokeDetector_UserId",
                table: "SolicitudesEmergenciaSmokeDetector",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaTreeDamage_PropiedadId",
                table: "SolicitudesEmergenciaTreeDamage",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaTreeDamage_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaTreeDamage",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaTreeDamage_UserId",
                table: "SolicitudesEmergenciaTreeDamage",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaWaterHeater_PropiedadId",
                table: "SolicitudesEmergenciaWaterHeater",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaWaterHeater_ServicioEmergenciaId",
                table: "SolicitudesEmergenciaWaterHeater",
                column: "ServicioEmergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesEmergenciaWaterHeater_UserId",
                table: "SolicitudesEmergenciaWaterHeater",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesExteriorPaint_HomeCarePriorityId",
                table: "SolicitudesExteriorPaint",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesExteriorPaint_PropiedadId",
                table: "SolicitudesExteriorPaint",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesExteriorPaint_UserId",
                table: "SolicitudesExteriorPaint",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesFurnitureAssembly_MovingSetupServicioId",
                table: "SolicitudesFurnitureAssembly",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesFurnitureAssembly_PropiedadId",
                table: "SolicitudesFurnitureAssembly",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesFurnitureAssembly_UserId",
                table: "SolicitudesFurnitureAssembly",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesGeneralHelp_MovingSetupServicioId",
                table: "SolicitudesGeneralHelp",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesGeneralHelp_PropiedadId",
                table: "SolicitudesGeneralHelp",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesGeneralHelp_UserId",
                table: "SolicitudesGeneralHelp",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesGutterCleaning_HomeCarePriorityId",
                table: "SolicitudesGutterCleaning",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesGutterCleaning_PropiedadId",
                table: "SolicitudesGutterCleaning",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesGutterCleaning_UserId",
                table: "SolicitudesGutterCleaning",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesHvacMaintenance_HomeCarePriorityId",
                table: "SolicitudesHvacMaintenance",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesHvacMaintenance_PropiedadId",
                table: "SolicitudesHvacMaintenance",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesHvacMaintenance_UserId",
                table: "SolicitudesHvacMaintenance",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccion_InspeccionId",
                table: "SolicitudesInspeccion",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccion_PropiedadId",
                table: "SolicitudesInspeccion",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccion_UserId",
                table: "SolicitudesInspeccion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionCompleta_InspeccionId",
                table: "SolicitudesInspeccionCompleta",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionCompleta_PropiedadId",
                table: "SolicitudesInspeccionCompleta",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionCompleta_UserId",
                table: "SolicitudesInspeccionCompleta",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionElectrica_InspeccionId",
                table: "SolicitudesInspeccionElectrica",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionElectrica_PropiedadId",
                table: "SolicitudesInspeccionElectrica",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionElectrica_UserId",
                table: "SolicitudesInspeccionElectrica",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionHomeSafety_InspeccionId",
                table: "SolicitudesInspeccionHomeSafety",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionHomeSafety_PropiedadId",
                table: "SolicitudesInspeccionHomeSafety",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionHomeSafety_UserId",
                table: "SolicitudesInspeccionHomeSafety",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionHvac_InspeccionId",
                table: "SolicitudesInspeccionHvac",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionHvac_PropiedadId",
                table: "SolicitudesInspeccionHvac",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionHvac_UserId",
                table: "SolicitudesInspeccionHvac",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionInvestor_InspeccionId",
                table: "SolicitudesInspeccionInvestor",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionInvestor_PropiedadId",
                table: "SolicitudesInspeccionInvestor",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionInvestor_UserId",
                table: "SolicitudesInspeccionInvestor",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionMoldMoisture_InspeccionId",
                table: "SolicitudesInspeccionMoldMoisture",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionMoldMoisture_PropiedadId",
                table: "SolicitudesInspeccionMoldMoisture",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionMoldMoisture_UserId",
                table: "SolicitudesInspeccionMoldMoisture",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionPlomeria_InspeccionId",
                table: "SolicitudesInspeccionPlomeria",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionPlomeria_PropiedadId",
                table: "SolicitudesInspeccionPlomeria",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionPlomeria_UserId",
                table: "SolicitudesInspeccionPlomeria",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionRoof_InspeccionId",
                table: "SolicitudesInspeccionRoof",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionRoof_PropiedadId",
                table: "SolicitudesInspeccionRoof",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionRoof_UserId",
                table: "SolicitudesInspeccionRoof",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionStructural_InspeccionId",
                table: "SolicitudesInspeccionStructural",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionStructural_PropiedadId",
                table: "SolicitudesInspeccionStructural",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionStructural_UserId",
                table: "SolicitudesInspeccionStructural",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionWindowsInsulation_InspeccionId",
                table: "SolicitudesInspeccionWindowsInsulation",
                column: "InspeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionWindowsInsulation_PropiedadId",
                table: "SolicitudesInspeccionWindowsInsulation",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesInspeccionWindowsInsulation_UserId",
                table: "SolicitudesInspeccionWindowsInsulation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesLawn_MicroservicioId",
                table: "SolicitudesLawn",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesLawn_PropiedadId",
                table: "SolicitudesLawn",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesLawn_UserId",
                table: "SolicitudesLawn",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMoving_MovingSetupServicioId",
                table: "SolicitudesMoving",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMoving_PropiedadId",
                table: "SolicitudesMoving",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesMoving_UserId",
                table: "SolicitudesMoving",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPacking_MovingSetupServicioId",
                table: "SolicitudesPacking",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPacking_PropiedadId",
                table: "SolicitudesPacking",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPacking_UserId",
                table: "SolicitudesPacking",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPestControl_HomeCarePriorityId",
                table: "SolicitudesPestControl",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPestControl_PropiedadId",
                table: "SolicitudesPestControl",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPestControl_UserId",
                table: "SolicitudesPestControl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPowerWash_HomeCarePriorityId",
                table: "SolicitudesPowerWash",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPowerWash_PropiedadId",
                table: "SolicitudesPowerWash",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPowerWash_UserId",
                table: "SolicitudesPowerWash",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRealtor_PropiedadId",
                table: "SolicitudesRealtor",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRemodelingServicio_PropiedadId",
                table: "SolicitudesRemodelingServicio",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRemodelingServicio_ServicioId",
                table: "SolicitudesRemodelingServicio",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRemodelingServicio_UserId",
                table: "SolicitudesRemodelingServicio",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRoofInspection_HomeCarePriorityId",
                table: "SolicitudesRoofInspection",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRoofInspection_PropiedadId",
                table: "SolicitudesRoofInspection",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesRoofInspection_UserId",
                table: "SolicitudesRoofInspection",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesSafeAir_MicroservicioId",
                table: "SolicitudesSafeAir",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesSafeAir_PropiedadId",
                table: "SolicitudesSafeAir",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesSafeAir_UserId",
                table: "SolicitudesSafeAir",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesSmokeDetector_HomeCarePriorityId",
                table: "SolicitudesSmokeDetector",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesSmokeDetector_PropiedadId",
                table: "SolicitudesSmokeDetector",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesSmokeDetector_UserId",
                table: "SolicitudesSmokeDetector",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesTrash_MicroservicioId",
                table: "SolicitudesTrash",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesTrash_PropiedadId",
                table: "SolicitudesTrash",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesTrash_UserId",
                table: "SolicitudesTrash",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesTvWallMounting_MovingSetupServicioId",
                table: "SolicitudesTvWallMounting",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesTvWallMounting_PropiedadId",
                table: "SolicitudesTvWallMounting",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesTvWallMounting_UserId",
                table: "SolicitudesTvWallMounting",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesUtilitiesSetup_MovingSetupServicioId",
                table: "SolicitudesUtilitiesSetup",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesUtilitiesSetup_PropiedadId",
                table: "SolicitudesUtilitiesSetup",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesUtilitiesSetup_ProveedorInternetId",
                table: "SolicitudesUtilitiesSetup",
                column: "ProveedorInternetId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesUtilitiesSetup_UserId",
                table: "SolicitudesUtilitiesSetup",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesWaterHeaterFlush_HomeCarePriorityId",
                table: "SolicitudesWaterHeaterFlush",
                column: "HomeCarePriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesWaterHeaterFlush_PropiedadId",
                table: "SolicitudesWaterHeaterFlush",
                column: "PropiedadId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesWaterHeaterFlush_UserId",
                table: "SolicitudesWaterHeaterFlush",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrashServicioLanding_MicroservicioId",
                table: "TrashServicioLanding",
                column: "MicroservicioId");

            migrationBuilder.CreateIndex(
                name: "IX_TvWallMountingServicioLanding_MovingSetupServicioId",
                table: "TvWallMountingServicioLanding",
                column: "MovingSetupServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilitiesSetupContactos_SolicitudUtilitiesSetupId",
                table: "UtilitiesSetupContactos",
                column: "SolicitudUtilitiesSetupId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterHeaterFlushServicioLanding_HomeCarePriorityId",
                table: "WaterHeaterFlushServicioLanding",
                column: "HomeCarePriorityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivosCleaning");

            migrationBuilder.DropTable(
                name: "ArchivosEmergenciaElectrical");

            migrationBuilder.DropTable(
                name: "ArchivosEmergenciaFlood");

            migrationBuilder.DropTable(
                name: "ArchivosEmergenciaHvac");

            migrationBuilder.DropTable(
                name: "ArchivosEmergenciaPlomeria");

            migrationBuilder.DropTable(
                name: "ArchivosEmergenciaRoofLeak");

            migrationBuilder.DropTable(
                name: "ArchivosEmergenciaTreeDamage");

            migrationBuilder.DropTable(
                name: "ArchivosEmergenciaWaterHeater");

            migrationBuilder.DropTable(
                name: "ArchivosExteriorPaint");

            migrationBuilder.DropTable(
                name: "ArchivosFurnitureAssembly");

            migrationBuilder.DropTable(
                name: "ArchivosGeneralHelp");

            migrationBuilder.DropTable(
                name: "ArchivosGutterCleaning");

            migrationBuilder.DropTable(
                name: "ArchivosHvacMaintenance");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionCompleta");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionElectrica");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionHomeSafety");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionHvac");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionInvestor");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionMoldMoisture");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionPlomeria");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionRoof");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionStructural");

            migrationBuilder.DropTable(
                name: "ArchivosInspeccionWindowsInsulation");

            migrationBuilder.DropTable(
                name: "ArchivosMoving");

            migrationBuilder.DropTable(
                name: "ArchivosPacking");

            migrationBuilder.DropTable(
                name: "ArchivosPowerWash");

            migrationBuilder.DropTable(
                name: "ArchivosRemodelingServicio");

            migrationBuilder.DropTable(
                name: "ArchivosReporteInspeccion");

            migrationBuilder.DropTable(
                name: "ArchivosRoofInspection");

            migrationBuilder.DropTable(
                name: "ArchivosSafeAir");

            migrationBuilder.DropTable(
                name: "ArchivosTvWallMounting");

            migrationBuilder.DropTable(
                name: "ArchivosWaterHeaterFlush");

            migrationBuilder.DropTable(
                name: "CleaningProServicioLanding");

            migrationBuilder.DropTable(
                name: "CleaningServicioLanding");

            migrationBuilder.DropTable(
                name: "CrawlspaceCheckServicioLanding");

            migrationBuilder.DropTable(
                name: "ExteriorPaintServicioLanding");

            migrationBuilder.DropTable(
                name: "FurnitureAssemblyServicioLanding");

            migrationBuilder.DropTable(
                name: "GutterCleaningServicioLanding");

            migrationBuilder.DropTable(
                name: "HistorialServicios");

            migrationBuilder.DropTable(
                name: "HomeCarePrioritiesConfig");

            migrationBuilder.DropTable(
                name: "HvacMaintenanceServicioLanding");

            migrationBuilder.DropTable(
                name: "IndorNearbyNetworkItems");

            migrationBuilder.DropTable(
                name: "IndorNearbyNetworkSettings");

            migrationBuilder.DropTable(
                name: "IndorNeighborRequestOffers");

            migrationBuilder.DropTable(
                name: "IndorNeighborRequestPhotos");

            migrationBuilder.DropTable(
                name: "IndorPasswordResetCodes");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdminHomecarePlans");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdminPortfolioProperties");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdminPreventivePlans");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdminPreventiveServiceCatalog");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdminScheduledVisits");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdminServiceCatalog");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdminServiceRequests");

            migrationBuilder.DropTable(
                name: "IndorProveedorAlcanceReglas");

            migrationBuilder.DropTable(
                name: "IndorProveedorApprovals");

            migrationBuilder.DropTable(
                name: "IndorProveedorCategoriasSel");

            migrationBuilder.DropTable(
                name: "IndorProveedorDocumentos");

            migrationBuilder.DropTable(
                name: "IndorProveedorExamPreguntas");

            migrationBuilder.DropTable(
                name: "IndorProveedorExamRespuestas");

            migrationBuilder.DropTable(
                name: "IndorProveedorInvoices");

            migrationBuilder.DropTable(
                name: "IndorProveedorMessages");

            migrationBuilder.DropTable(
                name: "IndorProveedorNetworkGuardados");

            migrationBuilder.DropTable(
                name: "IndorProveedorNetworkHires");

            migrationBuilder.DropTable(
                name: "IndorProveedorNetworkInvitaciones");

            migrationBuilder.DropTable(
                name: "IndorProveedorNetworkJobs");

            migrationBuilder.DropTable(
                name: "IndorProveedorNetworkQuotes");

            migrationBuilder.DropTable(
                name: "IndorProveedorNetworkResenas");

            migrationBuilder.DropTable(
                name: "IndorProveedorOfertasSel");

            migrationBuilder.DropTable(
                name: "IndorProveedorReportPhotos");

            migrationBuilder.DropTable(
                name: "IndorProveedorReportTemplateSections");

            migrationBuilder.DropTable(
                name: "IndorProveedorVerificaciones");

            migrationBuilder.DropTable(
                name: "IndorProviderInsuranceQuotes");

            migrationBuilder.DropTable(
                name: "IndorRealtorActivities");

            migrationBuilder.DropTable(
                name: "IndorRealtorDocumentos");

            migrationBuilder.DropTable(
                name: "IndorRealtorInspectionDraftProviders");

            migrationBuilder.DropTable(
                name: "IndorRealtorInspectionUploadFindings");

            migrationBuilder.DropTable(
                name: "IndorRealtorInvitations");

            migrationBuilder.DropTable(
                name: "IndorRealtorPropertyFileDraftCategories");

            migrationBuilder.DropTable(
                name: "IndorRealtorPropertyFileDraftItems");

            migrationBuilder.DropTable(
                name: "IndorRealtorPropertyFileItems");

            migrationBuilder.DropTable(
                name: "IndorRealtorQuoteRequestDraftProviders");

            migrationBuilder.DropTable(
                name: "IndorRealtorQuoteSentProviders");

            migrationBuilder.DropTable(
                name: "IndorRealtorSharedPackages");

            migrationBuilder.DropTable(
                name: "IndorRealtorSharedQuotes");

            migrationBuilder.DropTable(
                name: "IndorRealtorUrgentQuoteDraftPhotos");

            migrationBuilder.DropTable(
                name: "LawnCatalogOptions");

            migrationBuilder.DropTable(
                name: "LawnServicioLanding");

            migrationBuilder.DropTable(
                name: "MembresiasUsuario");

            migrationBuilder.DropTable(
                name: "MensajesSoporte");

            migrationBuilder.DropTable(
                name: "MovingServicioLanding");

            migrationBuilder.DropTable(
                name: "MovingSetupConfig");

            migrationBuilder.DropTable(
                name: "MovingSetupEnlacesRapidos");

            migrationBuilder.DropTable(
                name: "PackingServicioLanding");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "PestControlServicioLanding");

            migrationBuilder.DropTable(
                name: "PlanesInternet");

            migrationBuilder.DropTable(
                name: "PowerWashServicioLanding");

            migrationBuilder.DropTable(
                name: "ProgramacionesMicroservicio");

            migrationBuilder.DropTable(
                name: "PropiedadDocumentos");

            migrationBuilder.DropTable(
                name: "PropiedadHistorial");

            migrationBuilder.DropTable(
                name: "PropiedadHvacSistemas");

            migrationBuilder.DropTable(
                name: "PropiedadMantenimiento");

            migrationBuilder.DropTable(
                name: "PropiedadWaterHeaterSistemas");

            migrationBuilder.DropTable(
                name: "RoofInspectionServicioLanding");

            migrationBuilder.DropTable(
                name: "SafeAirServicioLanding");

            migrationBuilder.DropTable(
                name: "SmokeDetectorServicioLanding");

            migrationBuilder.DropTable(
                name: "SolicitudesCleaningPro");

            migrationBuilder.DropTable(
                name: "SolicitudesCrawlspaceCheck");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaSmokeDetector");

            migrationBuilder.DropTable(
                name: "SolicitudesLawn");

            migrationBuilder.DropTable(
                name: "SolicitudesPestControl");

            migrationBuilder.DropTable(
                name: "SolicitudesRealtor");

            migrationBuilder.DropTable(
                name: "SolicitudesSmokeDetector");

            migrationBuilder.DropTable(
                name: "SolicitudesTrash");

            migrationBuilder.DropTable(
                name: "TrashServicioLanding");

            migrationBuilder.DropTable(
                name: "TvWallMountingServicioLanding");

            migrationBuilder.DropTable(
                name: "UtilitiesSetupContactos");

            migrationBuilder.DropTable(
                name: "WaterHeaterFlushServicioLanding");

            migrationBuilder.DropTable(
                name: "SolicitudesCleaning");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaElectrical");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaFlood");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaHvac");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaPlomeria");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaRoofLeak");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaTreeDamage");

            migrationBuilder.DropTable(
                name: "SolicitudesEmergenciaWaterHeater");

            migrationBuilder.DropTable(
                name: "SolicitudesExteriorPaint");

            migrationBuilder.DropTable(
                name: "SolicitudesFurnitureAssembly");

            migrationBuilder.DropTable(
                name: "SolicitudesGeneralHelp");

            migrationBuilder.DropTable(
                name: "SolicitudesGutterCleaning");

            migrationBuilder.DropTable(
                name: "SolicitudesHvacMaintenance");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionCompleta");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionElectrica");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionHomeSafety");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionHvac");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionInvestor");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionMoldMoisture");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionPlomeria");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionRoof");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionStructural");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccionWindowsInsulation");

            migrationBuilder.DropTable(
                name: "SolicitudesMoving");

            migrationBuilder.DropTable(
                name: "SolicitudesPacking");

            migrationBuilder.DropTable(
                name: "SolicitudesPowerWash");

            migrationBuilder.DropTable(
                name: "SolicitudesRemodelingServicio");

            migrationBuilder.DropTable(
                name: "SolicitudesInspeccion");

            migrationBuilder.DropTable(
                name: "SolicitudesRoofInspection");

            migrationBuilder.DropTable(
                name: "SolicitudesSafeAir");

            migrationBuilder.DropTable(
                name: "SolicitudesTvWallMounting");

            migrationBuilder.DropTable(
                name: "SolicitudesWaterHeaterFlush");

            migrationBuilder.DropTable(
                name: "IndorRealtorClients");

            migrationBuilder.DropTable(
                name: "IndorNeighborRequests");

            migrationBuilder.DropTable(
                name: "IndorPropertyAdministrators");

            migrationBuilder.DropTable(
                name: "IndorProveedorCategoriasCatalogo");

            migrationBuilder.DropTable(
                name: "IndorProveedorEstimates");

            migrationBuilder.DropTable(
                name: "IndorProveedorConversations");

            migrationBuilder.DropTable(
                name: "IndorProveedorOfertasCatalogo");

            migrationBuilder.DropTable(
                name: "IndorProveedorReports");

            migrationBuilder.DropTable(
                name: "IndorProveedorReportTemplates");

            migrationBuilder.DropTable(
                name: "IndorRealtorInspectionUploadDrafts");

            migrationBuilder.DropTable(
                name: "IndorRealtorPropertyFileDrafts");

            migrationBuilder.DropTable(
                name: "IndorRealtorPropertyFiles");

            migrationBuilder.DropTable(
                name: "IndorRealtorQuoteProviders");

            migrationBuilder.DropTable(
                name: "IndorRealtorQuoteRequestDrafts");

            migrationBuilder.DropTable(
                name: "IndorRealtorQuoteBids");

            migrationBuilder.DropTable(
                name: "IndorRealtorUrgentQuoteDrafts");

            migrationBuilder.DropTable(
                name: "PlanesMembresia");

            migrationBuilder.DropTable(
                name: "MetodosPago");

            migrationBuilder.DropTable(
                name: "PropiedadProveedores");

            migrationBuilder.DropTable(
                name: "SolicitudesUtilitiesSetup");

            migrationBuilder.DropTable(
                name: "ServiciosEmergencia");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Inspecciones");

            migrationBuilder.DropTable(
                name: "Microservicios");

            migrationBuilder.DropTable(
                name: "HomeCarePriorities");

            migrationBuilder.DropTable(
                name: "IndorNeighborRequestCategories");

            migrationBuilder.DropTable(
                name: "IndorProveedorJobs");

            migrationBuilder.DropTable(
                name: "IndorRealtorQuotes");

            migrationBuilder.DropTable(
                name: "MovingSetupServicios");

            migrationBuilder.DropTable(
                name: "UtilitiesSetupProveedorInternet");

            migrationBuilder.DropTable(
                name: "IndorProveedorClientes");

            migrationBuilder.DropTable(
                name: "IndorProveedorLeads");

            migrationBuilder.DropTable(
                name: "IndorRealtors");

            migrationBuilder.DropTable(
                name: "IndorProveedores");

            migrationBuilder.DropColumn(
                name: "AttomLastSyncUtc",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "AttomPropertyId",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "AttomRawJson",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "AttomSyncError",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "AttomSyncStatus",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "MantenimientoRecomendadoJson",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "MantenimientoRecomendadoUtc",
                table: "Propiedades");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PreferredUiCulture",
                table: "AspNetUsers");
        }
    }
}
