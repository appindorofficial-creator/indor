-- =============================================================
-- Re-seed IndorDB catalog tables with English content.
-- Deletes existing catalog rows, then inserts fresh English data.
-- Safe to run multiple times.
-- =============================================================

USE [IndorDB];
GO

SET NOCOUNT ON;

-- MembresiasUsuario references PlanesMembresia (no CASCADE)
IF OBJECT_ID(N'dbo.MembresiasUsuario', N'U') IS NOT NULL
    DELETE FROM dbo.MembresiasUsuario;
GO

DELETE FROM dbo.PlanesMembresia;
DBCC CHECKIDENT ('dbo.PlanesMembresia', RESEED, -1);
GO

DELETE FROM dbo.Inspecciones;
DBCC CHECKIDENT ('dbo.Inspecciones', RESEED, -1);
GO

DELETE FROM dbo.Microservicios;
DBCC CHECKIDENT ('dbo.Microservicios', RESEED, 0);
GO

DELETE FROM dbo.PlanesInternet;
DBCC CHECKIDENT ('dbo.PlanesInternet', RESEED, -1);
GO

DELETE FROM dbo.Servicios;
DBCC CHECKIDENT ('dbo.Servicios', RESEED, 0);
GO

-- =============================================================
-- INSERT English catalog data
-- =============================================================

SET IDENTITY_INSERT [dbo].[PlanesMembresia] ON;
GO

INSERT [dbo].[PlanesMembresia] ([Id], [Nombre], [Subtitulo], [Descripcion], [PrecioMensual], [Moneda], [Caracteristicas], [Orden], [Activo], [Recomendado]) VALUES
(0, N'Basic Plan', N'Take care of the essentials of your home', N'Access to basic microservices and maintenance notifications.', CAST(9.99 AS Decimal(12, 2)), N'USD', N'1 express inspection per year|Warranty notifications|Chat support', 1, 1, 0),
(1, N'Monthly Plan', N'Your home always protected', N'Monthly coverage with discounts on services and inspections.', CAST(19.99 AS Decimal(12, 2)), N'USD', N'2 microservices per month|10% off services|Annual inspection included|Priority support', 2, 1, 1),
(2, N'Premium Plan', N'Everything included for your home', N'Full plan with quarterly inspections and preventive maintenance.', CAST(39.99 AS Decimal(12, 2)), N'USD', N'4 microservices per month|20% off services|Quarterly inspections|24/7 support', 3, 1, 0);
GO

SET IDENTITY_INSERT [dbo].[PlanesMembresia] OFF;
GO

SET IDENTITY_INSERT [dbo].[Inspecciones] ON;
GO

INSERT [dbo].[Inspecciones] ([Id], [Nombre], [Subtitulo], [Descripcion], [DescripcionCompleta], [Incluye], [Frecuencia], [Valor], [Moneda], [PrecioPrefijo], [PrecioTexto], [CtaTexto], [ImagenUrl], [Activo], [Orden], [FechaCreacion]) VALUES
(0, N'Pre-Purchase Home Inspection', N'Buy with confidence. Avoid costly mistakes.', N'Complete evaluation before buying a property.', N'We analyze the real condition of the home before you decide. We detect hidden issues that could cost thousands after purchase.', N'Basic structural review|Mechanical systems (HVAC, plumbing, electrical)|General condition assessment|Detailed report', N'Before each purchase', CAST(149.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Schedule inspection', N'/inspeccion1.jpeg', 1, 1, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(1, N'Complete Home Inspection', N'Everything in one diagnosis.', N'Full review of all home systems.', N'We inspect every key area of the property to give you a complete picture and help prevent future failures.', N'Electrical|Plumbing|HVAC|General structure', N'Every 1–2 years', CAST(129.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Request inspection', N'/inspeccion2.jpeg', 1, 2, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(2, N'Electrical Inspection', N'Avoid invisible risks.', N'Electrical system review for safety.', N'We detect faults, overloads, or defective installations that may pose a risk to your home and family.', N'Electrical panel|Wiring|Outlets and connections', N'Every 2–3 years', CAST(99.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Review electrical system', N'/inspeccion3.jpeg', 1, 3, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(3, N'Plumbing Inspection', N'Avoid leaks and unnecessary expenses.', N'Water and drainage system evaluation.', N'We identify leaks, inadequate pressure, and hidden damage that can affect the home structure.', N'Pipes|Drains|Water pressure', N'Every 1–2 years', CAST(99.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Inspect plumbing', N'/inspeccion4.jpeg', 1, 4, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(4, N'HVAC Inspection', N'Clean air, efficient system.', N'Complete air conditioning system review.', N'We evaluate HVAC performance to ensure efficiency, energy savings, and comfort.', N'AC unit|Filters|General operation', N'Every 6–12 months', CAST(89.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Review air conditioning', N'/inspeccion5.jpeg', 1, 5, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(5, N'Structural Inspection', N'The foundation of your investment.', N'Analysis of property stability and safety.', N'We assess possible structural damage that could compromise safety or lead to higher costs later.', N'Foundations|Walls|Roofs', N'Before purchase or remodeling', CAST(149.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Evaluate structure', N'/inspeccion6.jpeg', 1, 6, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(6, N'Roof Inspection', N'Protect your home from above.', N'Roof condition and leak review.', N'We detect wear, damage, or leaks that can affect your home protection.', N'Shingles|Sealing|Drainage', N'Every 1–2 years', CAST(99.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Inspect roof', N'/inspeccion7.jpeg', 1, 7, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(7, N'Foundation Inspection', N'Avoid serious structural problems.', N'Evaluation of the home base.', N'We identify cracks, settling, or failures that can compromise building stability.', N'Structural base|Cracks|Leveling', N'Every 2–3 years', CAST(129.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Inspect foundation', N'/inspeccion8.jpeg', 1, 8, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(8, N'Mold and Moisture Inspection', N'Protect your health and your home.', N'Moisture and mold detection.', N'We locate moisture issues that can cause mold and affect both structure and health.', N'Moisture detection|Wall assessment|Mold identification', N'When signs appear or every 2 years', CAST(119.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Detect moisture', N'/inspeccion9.jpeg', 1, 9, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(9, N'Windows and Insulation Inspection', N'Save energy without noticing.', N'Sealing and thermal efficiency evaluation.', N'We review windows and insulation to prevent energy loss and improve home efficiency.', N'Sealing|Insulation|Thermal loss', N'Every 2–3 years', CAST(89.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Evaluate efficiency', N'/inspeccion10.jpeg', 1, 10, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(10, N'Home Safety Inspection', N'Your family first.', N'Security and prevention systems review.', N'We evaluate smoke detectors, potential risks, and conditions that could compromise home safety.', N'Detectors|Basic risks|Recommendations', N'Annual', CAST(79.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Improve safety', N'/inspeccion11.jpeg', 1, 11, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(11, N'Inspection with Professional Report', N'Decisions based on real data.', N'Detailed home condition report.', N'Receive a clear, professional report with findings, photos, and recommendations for smart decisions.', N'Digital report|Photo evidence|Recommendations', N'Each inspection', CAST(49.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Get report', N'/inspeccion12.jpeg', 1, 12, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(12, N'Investor Inspection', N'Invest smart.', N'Strategic evaluation for property purchases.', N'We analyze properties from an investment perspective to help maximize returns and reduce risk.', N'General assessment|Potential risks|Investment recommendations', N'Per property', CAST(149.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Evaluate investment', N'/inspeccion13.jpeg', 1, 13, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(13, N'Hidden Problems Inspection', N'What you don''t see… is the most dangerous.', N'Detection of invisible failures.', N'We identify hidden damage not visible at first glance but that can lead to high costs.', N'Deep evaluation|Risk detection|Technical diagnosis', N'When suspected', CAST(129.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Detect problems', N'/inspeccion14.jpeg', 1, 14, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2)),
(14, N'Express Inspection', N'Fast, clear, and effective.', N'Quick review of key home points.', N'Ideal for quick decisions—we assess the most important items in little time without losing accuracy.', N'Critical points|Quick assessment|Basic recommendations', N'As needed', CAST(79.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Quick inspection', N'/inspeccion15.jpeg', 1, 15, CAST(N'2026-05-01T08:45:25.5153211' AS DateTime2));
GO

SET IDENTITY_INSERT [dbo].[Inspecciones] OFF;
GO

SET IDENTITY_INSERT [dbo].[Microservicios] ON;
GO

INSERT [dbo].[Microservicios] ([Id], [Nombre], [Descripcion], [Frecuencia], [Valor], [Moneda], [ImagenBase64], [Activo], [FechaCreacion], [Subtitulo], [DescripcionCompleta], [Incluye], [PrecioPrefijo], [CtaTexto], [ImagenUrl]) VALUES
(1, N'Safe Air 365', N'Professional air conditioner filter replacement to improve air quality and protect your system.', N'Every 3 months', CAST(49.00 AS Decimal(10, 2)), N'USD', N'', 1, CAST(N'2026-05-01T08:23:08.5599716' AS DateTime2), N'Breathe easy. We take care of it.', N'Keep your home healthy and your system running at 100%. We replace your AC filter quickly and safely, helping reduce dust, allergies, and costly breakdowns.', N'Basic system check|Filter replacement|Professional installation', N'From', N'Schedule service', N'/aire.jpeg'),
(2, N'Always Perfect Lawn', N'Lawn mowing and basic maintenance to keep your property looking great.', N'Weekly or biweekly', CAST(45.00 AS Decimal(10, 2)), N'USD', N'', 1, CAST(N'2026-05-01T08:23:08.5599716' AS DateTime2), N'Your home looks better... effortlessly.', N'We make sure your yard always looks clean, tidy, and professional. Ideal for maintaining property value without losing time.', N'Lawn mowing|Basic green area cleanup|Debris collection', N'From', N'Book maintenance', N'/cesped.jpeg'),
(3, N'Stress-Free Trash', N'We take out your trash and return the bins. Simple, automatic, and worry-free.', N'1–2 times per week', CAST(29.00 AS Decimal(10, 2)), N'USD', N'', 1, CAST(N'2026-05-01T08:23:08.5599716' AS DateTime2), N'Never forget collection day again.', N'Forget fines, bad odors, or missed pickups. We make sure your trash is ready on the right day and return bins to their place.', N'Place bins out on pickup day|Return bins to place|Punctual, reliable service', N'Monthly', N'Activate service', N'/basura.jpeg'),
(4, N'Cleaning Pro', N'Cleaning service to keep your home in perfect condition, effortlessly.', N'Weekly, biweekly, or monthly', CAST(99.00 AS Decimal(10, 2)), N'USD', N'', 1, CAST(N'2026-05-01T08:23:08.5599716' AS DateTime2), N'Professional cleaning you can see.', N'Enjoy a clean, organized home ready to live in or rent. Our team performs detailed cleanings tailored to your needs. Options: Standard cleaning or Deep cleaning.', N'Surface cleaning|Bathrooms and kitchen|Vacuuming and mopping', N'From', N'Schedule cleaning', N'/limpieza.jpeg');
GO

SET IDENTITY_INSERT [dbo].[Microservicios] OFF;
GO

SET IDENTITY_INSERT [dbo].[PlanesInternet] ON;
GO

INSERT [dbo].[PlanesInternet] ([Id], [Proveedor], [Nombre], [VelocidadDescargaMbps], [VelocidadSubidaMbps], [PrecioMensual], [Moneda], [Caracteristicas], [EsPlanActual], [Activo], [Orden]) VALUES
(0, N'Comcast Xfinity', N'Internet 300 Mbps', 300, 20, CAST(65.00 AS Decimal(12, 2)), N'USD', N'Basic Wi-Fi|No contract|Data capped at 1.2 TB', 1, 1, 1),
(1, N'AT&T Fiber', N'Fiber 500', 500, 500, CAST(70.00 AS Decimal(12, 2)), N'USD', N'Symmetric fiber|Unlimited data|Wi-Fi 6 included', 0, 1, 2),
(2, N'Verizon Fios', N'Fios 1 Gig', 1000, 1000, CAST(89.99 AS Decimal(12, 2)), N'USD', N'Symmetric fiber|Unlimited data|Router included', 0, 1, 3),
(3, N'T-Mobile Home', N'Home Internet 5G', 245, 31, CAST(50.00 AS Decimal(12, 2)), N'USD', N'5G wireless|No contract|Unlimited data', 0, 1, 4),
(4, N'Spectrum', N'Internet Ultra', 500, 20, CAST(69.99 AS Decimal(12, 2)), N'USD', N'Free Wi-Fi for 12 months|Unlimited data|No contract', 0, 1, 5);
GO

SET IDENTITY_INSERT [dbo].[PlanesInternet] OFF;
GO

SET IDENTITY_INSERT [dbo].[Servicios] ON;
GO

INSERT [dbo].[Servicios] ([Id], [Nombre], [Subtitulo], [Descripcion], [DescripcionCompleta], [Incluye], [Frecuencia], [Valor], [Moneda], [PrecioPrefijo], [PrecioTexto], [CtaTexto], [ImagenUrl], [Activo], [Orden], [FechaCreacion]) VALUES
(1, N'Dream Kitchen', N'Transform the heart of your home.', N'Complete kitchen remodel with modern, functional design.', N'We renovate your kitchen to improve aesthetics, functionality, and property value.', N'Custom design|Cabinet installation|Modern finishes', N'One-time project', CAST(5000.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Design my kitchen', N'/servicio1.jpeg', 1, 1, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(2, N'Modern Bath Pro', N'Comfort, style, and value in one space.', N'Complete bathroom remodel with high-quality finishes.', N'We transform your bathroom into a modern, functional, attractive space.', N'Fixture installation|Modern showers|Premium finishes', N'One-time project', CAST(3500.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Renovate bathroom', N'/servicio2.jpeg', 1, 2, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(3, N'Total Interior Renovation', N'Give your home new life.', N'Interior space remodeling for greater comfort and style.', N'We update your interior spaces with modern, functional solutions.', N'Space redesign|Structural improvements|Interior finishes', N'One-time project', CAST(2500.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Transform interior', N'/servicio3.jpeg', 1, 3, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(4, N'Space Expansion', N'More space, more value.', N'Home extensions to increase property size.', N'We create new spaces tailored to your needs: bedrooms, offices, or social areas.', N'Structural design|Full construction|Home integration', N'One-time project', NULL, N'USD', NULL, N'Custom', N'Expand my home', N'/servicio4.jpeg', 1, 4, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(5, N'Impactful Exteriors', N'First impressions matter.', N'Exterior remodeling to improve aesthetics and value.', N'We improve your property exterior appearance to increase appeal and market value.', N'Facades|Exterior paint|Decorative details', N'One-time project', CAST(2000.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Improve exterior', N'/servicio5.jpeg', 1, 5, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(6, N'Perfect Patio', N'Enjoy your home outdoors.', N'Design and construction of functional, modern patios.', N'We create outdoor spaces ideal for relaxing or sharing with family and friends.', N'Custom design|Full construction|Durable finishes', N'One-time project', CAST(3000.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Build patio', N'/servicio6.jpeg', 1, 6, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(7, N'Air Conditioning Installation', N'Comfort all year round.', N'Professional HVAC system installation.', N'We install efficient AC systems to improve your comfort and energy savings.', N'Full installation|Configuration|Operation testing', N'One-time service', CAST(1500.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Install system', N'/servicio7.jpeg', 1, 7, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(8, N'Water Heater Pro', N'Hot water without failures.', N'Water heater installation and replacement.', N'Ensure reliable hot water with professional installation.', N'Safe installation|Operation testing|Technical guidance', N'One-time service', CAST(900.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Install water heater', N'/servicio8.jpeg', 1, 8, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(9, N'Perfect Floors', N'Every step counts.', N'Floor installation and remodeling.', N'We renew your floors with modern, durable materials.', N'Professional installation|Leveling|Quality finishes', N'One-time project', CAST(1800.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Renew floors', N'/servicio9.jpeg', 1, 9, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(10, N'Professional Interior Painting', N'Refresh without remodeling.', N'Interior painting to quickly transform spaces.', N'Give your home a new look with clean, professional finishes.', NULL, N'One-time service', CAST(800.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Paint interior', N'/servicio10.jpeg', 1, 10, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(11, N'Premium Exterior Painting', N'Protection and style in one service.', N'Durable, weather-resistant exterior paint.', N'Protect your property from the elements while improving its appearance.', NULL, N'One-time service', CAST(1200.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Paint exterior', N'/servicio11.jpeg', 1, 11, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(12, N'Home Security', N'Protect what matters most.', N'Smoke detector and basic security system installation.', N'Increase home security with reliable, certified systems.', NULL, N'One-time service', CAST(300.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Install security', N'/servicio12.jpeg', 1, 12, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2)),
(13, N'Pro Concrete Driveway', N'Durability you can see.', N'Concrete driveway construction.', N'We design and install durable driveways that improve functionality and aesthetics.', NULL, N'One-time project', CAST(2500.00 AS Decimal(12, 2)), N'USD', N'From', NULL, N'Build driveway', N'/servicio13.jpeg', 1, 13, CAST(N'2026-05-01T08:37:11.7819750' AS DateTime2));
GO

SET IDENTITY_INSERT [dbo].[Servicios] OFF;
GO

PRINT 'Catalog data re-seeded in English successfully.';
GO
