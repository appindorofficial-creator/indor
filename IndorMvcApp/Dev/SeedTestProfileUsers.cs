using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Dev;

public static class SeedTestProfileUsers
{
    public const string DefaultPassword = "IndorTest1";

    private static readonly (string Email, string FullName, string Role)[] Profiles =
    [
        ("test.homeowner@indor.test", "Test Homeowner", "Propietario"),
        ("test.realtor@indor.test", "Test Realtor", "Realtor"),
        ("test.pa@indor.test", "Test PA", "AdministradorPropiedades"),
        ("test.provider@indor.test", "Test Provider", "ProveedorServicios")
    ];

    public static async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("Seeding INDOR test profile users...");
        Console.WriteLine($"Password for all accounts: {DefaultPassword}");
        Console.WriteLine();

        foreach (var (email, fullName, role) in Profiles)
        {
            await EnsureRoleAsync(roleManager, role, cancellationToken);
            var user = await EnsureUserAsync(userManager, email, fullName, role, cancellationToken);
            await EnsureProfileDataAsync(db, user, role, email, fullName, cancellationToken);
            Console.WriteLine($"  OK  {role,-26} {email}");
        }

        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string role,
        CancellationToken cancellationToken)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string role,
        CancellationToken cancellationToken)
    {
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var nombre = parts.Length > 0 ? parts[0] : fullName;
        var apellidos = parts.Length > 1 ? parts[1] : string.Empty;

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nombre = nombre,
                Apellidos = apellidos,
                Telefono = "5551234567",
                PhoneNumber = "5551234567",
                RolUsuario = role,
                FechaRegistro = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, DefaultPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create {email}: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            user.RolUsuario = role;
            user.Nombre = nombre;
            user.Apellidos = apellidos;
            user.EmailConfirmed = true;
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not update {email}: {string.Join("; ", updateResult.Errors.Select(e => e.Description))}");
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, resetToken, DefaultPassword);
            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not reset password for {email}: {string.Join("; ", resetResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    private static async Task EnsureProfileDataAsync(
        AppDbContext db,
        ApplicationUser user,
        string role,
        string email,
        string fullName,
        CancellationToken cancellationToken)
    {
        switch (role)
        {
            case "Propietario":
                await EnsureHomeownerPropertyAsync(db, user, cancellationToken);
                break;
            case "Realtor":
                await EnsureRealtorAsync(db, user, email, fullName, cancellationToken);
                break;
            case "AdministradorPropiedades":
                await EnsurePropertyAdministratorAsync(db, user, email, fullName, cancellationToken);
                break;
            case "ProveedorServicios":
                await EnsureProviderAsync(db, user, email, fullName, cancellationToken);
                break;
        }
    }

    private static async Task EnsureHomeownerPropertyAsync(
        AppDbContext db,
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var hasProperty = await db.Propiedades
            .AnyAsync(p => p.UserId == user.Id && p.Activo, cancellationToken);
        if (hasProperty)
        {
            return;
        }

        db.Propiedades.Add(new Propiedad
        {
            UserId = user.Id,
            Direccion = "789 Maple Drive, Charlotte, NC 28202",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureRealtorAsync(
        AppDbContext db,
        ApplicationUser user,
        string email,
        string fullName,
        CancellationToken cancellationToken)
    {
        var realtor = await db.IndorRealtors.FirstOrDefaultAsync(r => r.UserId == user.Id, cancellationToken);
        if (realtor == null)
        {
            realtor = new IndorRealtor
            {
                UserId = user.Id,
                FechaCreacion = DateTime.UtcNow
            };
            db.IndorRealtors.Add(realtor);
        }

        realtor.DisplayName = fullName;
        realtor.Email = email;
        realtor.Phone = "(704) 555-0100";
        realtor.BrokerageName = "INDOR Test Realty";
        realtor.LicenseNumber = "TEST12345";
        realtor.LicenseState = "NC";
        realtor.ServiceAreas = "Charlotte, NC";
        realtor.ProfessionalTermsAccepted = true;
        realtor.TermsAcceptedUtc = DateTime.UtcNow;
        realtor.RegistrationStatus = RealtorRegistrationStatuses.Basic;
        realtor.ProfileCompletedUtc = DateTime.UtcNow;
        realtor.CurrentStep = 3;
        realtor.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsurePropertyAdministratorAsync(
        AppDbContext db,
        ApplicationUser user,
        string email,
        string fullName,
        CancellationToken cancellationToken)
    {
        var admin = await db.IndorPropertyAdministrators
            .Include(a => a.PortfolioProperties)
            .FirstOrDefaultAsync(a => a.UserId == user.Id, cancellationToken);

        if (admin == null)
        {
            admin = new IndorPropertyAdministrator
            {
                UserId = user.Id,
                FechaCreacion = DateTime.UtcNow
            };
            db.IndorPropertyAdministrators.Add(admin);
        }

        admin.DisplayName = fullName;
        admin.Email = email;
        admin.Phone = "(704) 555-0200";
        admin.PortfolioBusinessName = "INDOR Test Portfolio";
        admin.TermsAccepted = true;
        admin.TermsAcceptedUtc = DateTime.UtcNow;
        admin.PlatformTermsAccepted = true;
        admin.PropertyCountRange = "2-5";
        admin.PortfolioType = "ShortTermRental";
        admin.OwnershipType = "Owner";
        admin.PrimaryMarket = "Charlotte, NC";
        admin.ManagementStyle = "SelfManaged";
        admin.RegistrationStatus = PropertyAdministratorRegistrationStatuses.Completed;
        admin.RegistrationCompletedUtc = DateTime.UtcNow;
        admin.CurrentStep = 5;
        admin.FechaActualizacion = DateTime.UtcNow;

        if (admin.PortfolioProperties.Count == 0)
        {
            admin.PortfolioProperties.Add(new IndorPropertyAdminPortfolioProperty
            {
                PropertyName = "789 Maple Drive",
                Location = "789 Maple Drive, Charlotte, NC 28202",
                PropertyType = "ShortTermRental",
                Status = "Added",
                FechaCreacion = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureProviderAsync(
        AppDbContext db,
        ApplicationUser user,
        string email,
        string fullName,
        CancellationToken cancellationToken)
    {
        var provider = await db.IndorProveedores.FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);
        if (provider == null)
        {
            provider = new IndorProveedor
            {
                UserId = user.Id,
                FechaCreacion = DateTime.UtcNow
            };
            db.IndorProveedores.Add(provider);
        }

        provider.ProviderType = "Independent";
        provider.BusinessName = "INDOR Test Services";
        provider.PrimaryContact = fullName;
        provider.Email = email;
        provider.Phone = "(704) 555-0300";
        provider.BusinessAddress = "100 Trade St, Charlotte, NC 28202";
        provider.PrimaryCity = "Charlotte, NC";
        provider.Latitude = 35.2271m;
        provider.Longitude = -80.8431m;
        provider.TravelRadiusMiles = 25;
        provider.BackgroundCheckConsent = true;
        provider.IsInsured = true;
        provider.IsLicensed = true;
        provider.RegistrationStatus = ProviderRegistrationStatuses.IndorProActive;
        provider.CurrentStep = 6;
        provider.ExamPassed = true;
        provider.ExamScorePercent = 90;
        provider.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
