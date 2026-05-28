-- =============================================================
-- Translate existing IndorDB catalog and property JSON data to English.
-- Safe to run multiple times (idempotent updates by Id / REPLACE).
--
-- Usage:
--   USE [IndorDB];
--   GO
--   :r TranslateCatalogDataToEnglish.sql
-- Or execute this file from SSMS / sqlcmd.
-- =============================================================

USE [IndorDB];
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY

    -- =========================================================
    -- 1. Membership plans (PlanesMembresia)
    -- =========================================================
    UPDATE dbo.PlanesMembresia SET
        Nombre = N'Basic Plan',
        Subtitulo = N'Take care of the essentials of your home',
        Descripcion = N'Access to basic microservices and maintenance notifications.',
        Caracteristicas = N'1 express inspection per year|Warranty notifications|Chat support'
    WHERE Id = 0;

    UPDATE dbo.PlanesMembresia SET
        Nombre = N'Monthly Plan',
        Subtitulo = N'Your home always protected',
        Descripcion = N'Monthly coverage with discounts on services and inspections.',
        Caracteristicas = N'2 microservices per month|10% off services|Annual inspection included|Priority support'
    WHERE Id = 1;

    UPDATE dbo.PlanesMembresia SET
        Nombre = N'Premium Plan',
        Subtitulo = N'Everything included for your home',
        Descripcion = N'Full plan with quarterly inspections and preventive maintenance.',
        Caracteristicas = N'4 microservices per month|20% off services|Quarterly inspections|24/7 support'
    WHERE Id = 2;

    -- =========================================================
    -- 2. Microservices (Microservicios)
    -- =========================================================
    UPDATE dbo.Microservicios SET
        Nombre = N'Safe Air 365',
        Subtitulo = N'Breathe easy. We take care of it.',
        Descripcion = N'Professional air conditioner filter replacement to improve air quality and protect your system.',
        DescripcionCompleta = N'Keep your home healthy and your system running at 100%. We replace your AC filter quickly and safely, helping reduce dust, allergies, and costly breakdowns.',
        Incluye = N'Basic system check|Filter replacement|Professional installation',
        Frecuencia = N'Every 3 months',
        PrecioPrefijo = N'From',
        CtaTexto = N'Schedule service'
    WHERE Id = 1;

    UPDATE dbo.Microservicios SET
        Nombre = N'Always Perfect Lawn',
        Subtitulo = N'Your home looks better... effortlessly.',
        Descripcion = N'Lawn mowing and basic maintenance to keep your property looking great.',
        DescripcionCompleta = N'We make sure your yard always looks clean, tidy, and professional. Ideal for maintaining property value without losing time.',
        Incluye = N'Lawn mowing|Basic green area cleanup|Debris collection',
        Frecuencia = N'Weekly or biweekly',
        PrecioPrefijo = N'From',
        CtaTexto = N'Book maintenance'
    WHERE Id = 2;

    UPDATE dbo.Microservicios SET
        Nombre = N'Stress-Free Trash',
        Subtitulo = N'Never forget collection day again.',
        Descripcion = N'We take out your trash and return the bins. Simple, automatic, and worry-free.',
        DescripcionCompleta = N'Forget fines, bad odors, or missed pickups. We make sure your trash is ready on the right day and return bins to their place.',
        Incluye = N'Place bins out on pickup day|Return bins to place|Punctual, reliable service',
        Frecuencia = N'1–2 times per week',
        PrecioPrefijo = N'Monthly',
        CtaTexto = N'Activate service'
    WHERE Id = 3;

    UPDATE dbo.Microservicios SET
        Nombre = N'Cleaning Pro',
        Subtitulo = N'Professional cleaning you can see.',
        Descripcion = N'Cleaning service to keep your home in perfect condition, effortlessly.',
        DescripcionCompleta = N'Enjoy a clean, organized home ready to live in or rent. Our team performs detailed cleanings tailored to your needs. Options: Standard cleaning or Deep cleaning.',
        Incluye = N'Surface cleaning|Bathrooms and kitchen|Vacuuming and mopping',
        Frecuencia = N'Weekly, biweekly, or monthly',
        PrecioPrefijo = N'From',
        CtaTexto = N'Schedule cleaning'
    WHERE Id = 4;

    -- =========================================================
    -- 3. Inspections (Inspecciones)
    -- =========================================================
    UPDATE dbo.Inspecciones SET Nombre=N'Pre-Purchase Home Inspection', Subtitulo=N'Buy with confidence. Avoid costly mistakes.', Descripcion=N'Complete evaluation before buying a property.', DescripcionCompleta=N'We analyze the real condition of the home before you decide. We detect hidden issues that could cost thousands after purchase.', Incluye=N'Basic structural review|Mechanical systems (HVAC, plumbing, electrical)|General condition assessment|Detailed report', Frecuencia=N'Before each purchase', PrecioPrefijo=N'From', CtaTexto=N'Schedule inspection' WHERE Id=0;
    UPDATE dbo.Inspecciones SET Nombre=N'Complete Home Inspection', Subtitulo=N'Everything in one diagnosis.', Descripcion=N'Full review of all home systems.', DescripcionCompleta=N'We inspect every key area of the property to give you a complete picture and help prevent future failures.', Incluye=N'Electrical|Plumbing|HVAC|General structure', Frecuencia=N'Every 1–2 years', PrecioPrefijo=N'From', CtaTexto=N'Request inspection' WHERE Id=1;
    UPDATE dbo.Inspecciones SET Nombre=N'Electrical Inspection', Subtitulo=N'Avoid invisible risks.', Descripcion=N'Electrical system review for safety.', DescripcionCompleta=N'We detect faults, overloads, or defective installations that may pose a risk to your home and family.', Incluye=N'Electrical panel|Wiring|Outlets and connections', Frecuencia=N'Every 2–3 years', PrecioPrefijo=N'From', CtaTexto=N'Review electrical system' WHERE Id=2;
    UPDATE dbo.Inspecciones SET Nombre=N'Plumbing Inspection', Subtitulo=N'Avoid leaks and unnecessary expenses.', Descripcion=N'Water and drainage system evaluation.', DescripcionCompleta=N'We identify leaks, inadequate pressure, and hidden damage that can affect the home structure.', Incluye=N'Pipes|Drains|Water pressure', Frecuencia=N'Every 1–2 years', PrecioPrefijo=N'From', CtaTexto=N'Inspect plumbing' WHERE Id=3;
    UPDATE dbo.Inspecciones SET Nombre=N'HVAC Inspection', Subtitulo=N'Clean air, efficient system.', Descripcion=N'Complete air conditioning system review.', DescripcionCompleta=N'We evaluate HVAC performance to ensure efficiency, energy savings, and comfort.', Incluye=N'AC unit|Filters|General operation', Frecuencia=N'Every 6–12 months', PrecioPrefijo=N'From', CtaTexto=N'Review air conditioning' WHERE Id=4;
    UPDATE dbo.Inspecciones SET Nombre=N'Structural Inspection', Subtitulo=N'The foundation of your investment.', Descripcion=N'Analysis of property stability and safety.', DescripcionCompleta=N'We assess possible structural damage that could compromise safety or lead to higher costs later.', Incluye=N'Foundations|Walls|Roofs', Frecuencia=N'Before purchase or remodeling', PrecioPrefijo=N'From', CtaTexto=N'Evaluate structure' WHERE Id=5;
    UPDATE dbo.Inspecciones SET Nombre=N'Roof Inspection', Subtitulo=N'Protect your home from above.', Descripcion=N'Roof condition and leak review.', DescripcionCompleta=N'We detect wear, damage, or leaks that can affect your home protection.', Incluye=N'Shingles|Sealing|Drainage', Frecuencia=N'Every 1–2 years', PrecioPrefijo=N'From', CtaTexto=N'Inspect roof' WHERE Id=6;
    UPDATE dbo.Inspecciones SET Nombre=N'Foundation Inspection', Subtitulo=N'Avoid serious structural problems.', Descripcion=N'Evaluation of the home base.', DescripcionCompleta=N'We identify cracks, settling, or failures that can compromise building stability.', Incluye=N'Structural base|Cracks|Leveling', Frecuencia=N'Every 2–3 years', PrecioPrefijo=N'From', CtaTexto=N'Inspect foundation' WHERE Id=7;
    UPDATE dbo.Inspecciones SET Nombre=N'Mold and Moisture Inspection', Subtitulo=N'Protect your health and your home.', Descripcion=N'Moisture and mold detection.', DescripcionCompleta=N'We locate moisture issues that can cause mold and affect both structure and health.', Incluye=N'Moisture detection|Wall assessment|Mold identification', Frecuencia=N'When signs appear or every 2 years', PrecioPrefijo=N'From', CtaTexto=N'Detect moisture' WHERE Id=8;
    UPDATE dbo.Inspecciones SET Nombre=N'Windows and Insulation Inspection', Subtitulo=N'Save energy without noticing.', Descripcion=N'Sealing and thermal efficiency evaluation.', DescripcionCompleta=N'We review windows and insulation to prevent energy loss and improve home efficiency.', Incluye=N'Sealing|Insulation|Thermal loss', Frecuencia=N'Every 2–3 years', PrecioPrefijo=N'From', CtaTexto=N'Evaluate efficiency' WHERE Id=9;
    UPDATE dbo.Inspecciones SET Nombre=N'Home Safety Inspection', Subtitulo=N'Your family first.', Descripcion=N'Security and prevention systems review.', DescripcionCompleta=N'We evaluate smoke detectors, potential risks, and conditions that could compromise home safety.', Incluye=N'Detectors|Basic risks|Recommendations', Frecuencia=N'Annual', PrecioPrefijo=N'From', CtaTexto=N'Improve safety' WHERE Id=10;
    UPDATE dbo.Inspecciones SET Nombre=N'Inspection with Professional Report', Subtitulo=N'Decisions based on real data.', Descripcion=N'Detailed home condition report.', DescripcionCompleta=N'Receive a clear, professional report with findings, photos, and recommendations for smart decisions.', Incluye=N'Digital report|Photo evidence|Recommendations', Frecuencia=N'Each inspection', PrecioPrefijo=N'From', CtaTexto=N'Get report' WHERE Id=11;
    UPDATE dbo.Inspecciones SET Nombre=N'Investor Inspection', Subtitulo=N'Invest smart.', Descripcion=N'Strategic evaluation for property purchases.', DescripcionCompleta=N'We analyze properties from an investment perspective to help maximize returns and reduce risk.', Incluye=N'General assessment|Potential risks|Investment recommendations', Frecuencia=N'Per property', PrecioPrefijo=N'From', CtaTexto=N'Evaluate investment' WHERE Id=12;
    UPDATE dbo.Inspecciones SET Nombre=N'Hidden Problems Inspection', Subtitulo=N'What you don''t see... is the most dangerous.', Descripcion=N'Detection of invisible failures.', DescripcionCompleta=N'We identify hidden damage not visible at first glance but that can lead to high costs.', Incluye=N'Deep evaluation|Risk detection|Technical diagnosis', Frecuencia=N'When suspected', PrecioPrefijo=N'From', CtaTexto=N'Detect problems' WHERE Id=13;
    UPDATE dbo.Inspecciones SET Nombre=N'Express Inspection', Subtitulo=N'Fast, clear, and effective.', Descripcion=N'Quick review of key home points.', DescripcionCompleta=N'Ideal for quick decisions—we assess the most important items in little time without losing accuracy.', Incluye=N'Critical points|Quick assessment|Basic recommendations', Frecuencia=N'As needed', PrecioPrefijo=N'From', CtaTexto=N'Quick inspection' WHERE Id=14;

    -- =========================================================
    -- 4. Services (Servicios)
    -- =========================================================
    UPDATE dbo.Servicios SET Nombre=N'Dream Kitchen', Subtitulo=N'Transform the heart of your home.', Descripcion=N'Complete kitchen remodel with modern, functional design.', DescripcionCompleta=N'We renovate your kitchen to improve aesthetics, functionality, and property value.', Incluye=N'Custom design|Cabinet installation|Modern finishes', Frecuencia=N'One-time project', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Design my kitchen' WHERE Id=1;
    UPDATE dbo.Servicios SET Nombre=N'Modern Bath Pro', Subtitulo=N'Comfort, style, and value in one space.', Descripcion=N'Complete bathroom remodel with high-quality finishes.', DescripcionCompleta=N'We transform your bathroom into a modern, functional, attractive space.', Incluye=N'Fixture installation|Modern showers|Premium finishes', Frecuencia=N'One-time project', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Renovate bathroom' WHERE Id=2;
    UPDATE dbo.Servicios SET Nombre=N'Total Interior Renovation', Subtitulo=N'Give your home new life.', Descripcion=N'Interior space remodeling for greater comfort and style.', DescripcionCompleta=N'We update your interior spaces with modern, functional solutions.', Incluye=N'Space redesign|Structural improvements|Interior finishes', Frecuencia=N'One-time project', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Transform interior' WHERE Id=3;
    UPDATE dbo.Servicios SET Nombre=N'Space Expansion', Subtitulo=N'More space, more value.', Descripcion=N'Home extensions to increase property size.', DescripcionCompleta=N'We create new spaces tailored to your needs: bedrooms, offices, or social areas.', Incluye=N'Structural design|Full construction|Home integration', Frecuencia=N'One-time project', PrecioPrefijo=NULL, PrecioTexto=N'Custom', CtaTexto=N'Expand my home' WHERE Id=4;
    UPDATE dbo.Servicios SET Nombre=N'Impactful Exteriors', Subtitulo=N'First impressions matter.', Descripcion=N'Exterior remodeling to improve aesthetics and value.', DescripcionCompleta=N'We improve your property exterior appearance to increase appeal and market value.', Incluye=N'Facades|Exterior paint|Decorative details', Frecuencia=N'One-time project', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Improve exterior' WHERE Id=5;
    UPDATE dbo.Servicios SET Nombre=N'Perfect Patio', Subtitulo=N'Enjoy your home outdoors.', Descripcion=N'Design and construction of functional, modern patios.', DescripcionCompleta=N'We create outdoor spaces ideal for relaxing or sharing with family and friends.', Incluye=N'Custom design|Full construction|Durable finishes', Frecuencia=N'One-time project', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Build patio' WHERE Id=6;
    UPDATE dbo.Servicios SET Nombre=N'Air Conditioning Installation', Subtitulo=N'Comfort all year round.', Descripcion=N'Professional HVAC system installation.', DescripcionCompleta=N'We install efficient AC systems to improve your comfort and energy savings.', Incluye=N'Full installation|Configuration|Operation testing', Frecuencia=N'One-time service', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Install system' WHERE Id=7;
    UPDATE dbo.Servicios SET Nombre=N'Water Heater Pro', Subtitulo=N'Hot water without failures.', Descripcion=N'Water heater installation and replacement.', DescripcionCompleta=N'Ensure reliable hot water with professional installation.', Incluye=N'Safe installation|Operation testing|Technical guidance', Frecuencia=N'One-time service', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Install water heater' WHERE Id=8;
    UPDATE dbo.Servicios SET Nombre=N'Perfect Floors', Subtitulo=N'Every step counts.', Descripcion=N'Floor installation and remodeling.', DescripcionCompleta=N'We renew your floors with modern, durable materials.', Incluye=N'Professional installation|Leveling|Quality finishes', Frecuencia=N'One-time project', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Renew floors' WHERE Id=9;
    UPDATE dbo.Servicios SET Nombre=N'Professional Interior Painting', Subtitulo=N'Refresh without remodeling.', Descripcion=N'Interior painting to quickly transform spaces.', DescripcionCompleta=N'Give your home a new look with clean, professional finishes.', Incluye=NULL, Frecuencia=N'One-time service', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Paint interior' WHERE Id=10;
    UPDATE dbo.Servicios SET Nombre=N'Premium Exterior Painting', Subtitulo=N'Protection and style in one service.', Descripcion=N'Durable, weather-resistant exterior paint.', DescripcionCompleta=N'Protect your property from the elements while improving its appearance.', Incluye=NULL, Frecuencia=N'One-time service', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Paint exterior' WHERE Id=11;
    UPDATE dbo.Servicios SET Nombre=N'Home Security', Subtitulo=N'Protect what matters most.', Descripcion=N'Smoke detector and basic security system installation.', DescripcionCompleta=N'Increase home security with reliable, certified systems.', Incluye=NULL, Frecuencia=N'One-time service', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Install security' WHERE Id=12;
    UPDATE dbo.Servicios SET Nombre=N'Pro Concrete Driveway', Subtitulo=N'Durability you can see.', Descripcion=N'Concrete driveway construction.', DescripcionCompleta=N'We design and install durable driveways that improve functionality and aesthetics.', Incluye=NULL, Frecuencia=N'One-time project', PrecioPrefijo=N'From', PrecioTexto=NULL, CtaTexto=N'Build driveway' WHERE Id=13;

    -- =========================================================
    -- 5. Internet plans (PlanesInternet)
    -- =========================================================
    UPDATE dbo.PlanesInternet SET Caracteristicas = N'Basic Wi-Fi|No contract|Data capped at 1.2 TB' WHERE Id = 0;
    UPDATE dbo.PlanesInternet SET Caracteristicas = N'Symmetric fiber|Unlimited data|Wi-Fi 6 included' WHERE Id = 1;
    UPDATE dbo.PlanesInternet SET Caracteristicas = N'Symmetric fiber|Unlimited data|Router included' WHERE Id = 2;
    UPDATE dbo.PlanesInternet SET Caracteristicas = N'5G wireless|No contract|Unlimited data' WHERE Id = 3;
    UPDATE dbo.PlanesInternet SET Caracteristicas = N'Free Wi-Fi for 12 months|Unlimited data|No contract' WHERE Id = 4;

    -- =========================================================
    -- 6. Generic catalog text (any remaining rows)
    -- =========================================================
    UPDATE dbo.Microservicios SET PrecioPrefijo = N'From' WHERE PrecioPrefijo = N'Desde';
    UPDATE dbo.Microservicios SET PrecioPrefijo = N'Monthly' WHERE PrecioPrefijo = N'Mensual';
    UPDATE dbo.Inspecciones SET PrecioPrefijo = N'From' WHERE PrecioPrefijo = N'Desde';
    UPDATE dbo.Servicios SET PrecioPrefijo = N'From' WHERE PrecioPrefijo = N'Desde';
    UPDATE dbo.Servicios SET PrecioTexto = N'Custom' WHERE PrecioTexto = N'Personalizado';

    -- =========================================================
    -- 7. Payments, history, support (if tables have data)
    -- =========================================================
    IF OBJECT_ID(N'dbo.Pagos', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.Pagos SET Estado = N'Pending'   WHERE Estado = N'Pendiente';
        UPDATE dbo.Pagos SET Estado = N'Completed' WHERE Estado = N'Completado';
        UPDATE dbo.Pagos SET Estado = N'Financed'  WHERE Estado = N'Financiado';
        UPDATE dbo.Pagos SET Estado = N'Overdue'   WHERE Estado = N'Vencido';
    END

    IF OBJECT_ID(N'dbo.HistorialServicios', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.HistorialServicios SET Estado = N'Completed' WHERE Estado = N'Completado';
        UPDATE dbo.HistorialServicios SET Estado = N'Pending'   WHERE Estado = N'Pendiente';
        UPDATE dbo.HistorialServicios SET Estado = N'Overdue'   WHERE Estado = N'Vencido';
        UPDATE dbo.HistorialServicios SET Estado = N'In progress' WHERE Estado = N'En curso';
        UPDATE dbo.HistorialServicios SET Estado = N'Cancelled' WHERE Estado = N'Cancelado';
    END

    IF OBJECT_ID(N'dbo.MetodosPago', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.MetodosPago SET Tipo = N'Card' WHERE Tipo = N'Tarjeta';
        UPDATE dbo.MetodosPago SET Tipo = N'Bank' WHERE Tipo = N'Banco';
    END

    IF OBJECT_ID(N'dbo.MensajesSoporte', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.MensajesSoporte SET Remitente = N'User'    WHERE Remitente = N'Usuario';
        UPDATE dbo.MensajesSoporte SET Remitente = N'Support' WHERE Remitente = N'Soporte';
        UPDATE dbo.MensajesSoporte SET Contenido = N'Hello! We received your message. An agent will reply soon.'
        WHERE Contenido LIKE N'%Hemos recibido tu mensaje%';
    END

    -- =========================================================
    -- 8. Property JSON blobs (Propiedades.DatosJson)
    --    Keeps addresses/coordinates; translates embedded Spanish.
    -- =========================================================
    IF OBJECT_ID(N'dbo.Propiedades', N'U') IS NOT NULL
    BEGIN
        DECLARE @json NVARCHAR(MAX);
        DECLARE @id INT;
        DECLARE prop_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT Id, DatosJson FROM dbo.Propiedades WHERE DatosJson IS NOT NULL;

        OPEN prop_cursor;
        FETCH NEXT FROM prop_cursor INTO @id, @json;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @json = REPLACE(@json, N'Casa unifamiliar', N'Single-family home');
            SET @json = REPLACE(@json, N'Residencial clásico', N'Classic residential');
            SET @json = REPLACE(@json, N'Residencial cl\u00E1sico', N'Classic residential');
            SET @json = REPLACE(@json, N'Residencial R-1', N'Residential R-1');
            SET @json = REPLACE(@json, N'Garaje', N'Garage');
            SET @json = REPLACE(@json, N'Patio trasero', N'Backyard');
            SET @json = REPLACE(@json, N'Sistema de climatización central', N'Central HVAC system');
            SET @json = REPLACE(@json, N'Sistema de climatizaci\u00F3n central', N'Central HVAC system');
            SET @json = REPLACE(@json, N'Cocina equipada', N'Equipped kitchen');
            SET @json = REPLACE(@json, N'Área de lavandería', N'Laundry area');
            SET @json = REPLACE(@json, N'\u00C1rea de lavander\u00EDa', N'Laundry area');

            SET @json = REPLACE(@json, N'"ServiceType":"Electricidad"', N'"ServiceType":"Electricity"');
            SET @json = REPLACE(@json, N'"ServiceType":"Agua Potable"', N'"ServiceType":"Drinking water"');
            SET @json = REPLACE(@json, N'"ServiceType":"Gas Natural"', N'"ServiceType":"Natural gas"');
            SET @json = REPLACE(@json, N'"ServiceType":"Alcantarillado"', N'"ServiceType":"Sewer"');
            SET @json = REPLACE(@json, N'"ServiceType":"Internet por Cable"', N'"ServiceType":"Cable internet"');
            SET @json = REPLACE(@json, N'"Coverage":"Proveedor activo en esta dirección"', N'"Coverage":"Active provider at this address"');
            SET @json = REPLACE(@json, N'"Coverage":"Proveedor activo en esta direcci\u00F3n"', N'"Coverage":"Active provider at this address"');
            SET @json = REPLACE(@json, N'"Coverage":"Proveedor municipal asignado"', N'"Coverage":"Assigned municipal provider"');
            SET @json = REPLACE(@json, N'"Coverage":"Servicio municipal asignado"', N'"Coverage":"Assigned municipal service"');
            SET @json = REPLACE(@json, N'"Coverage":"Proveedor asignado en esta zona"', N'"Coverage":"Assigned provider in this area"');
            SET @json = REPLACE(@json, N'"Coverage":"Proveedor asignado para esta dirección"', N'"Coverage":"Assigned provider for this address"');
            SET @json = REPLACE(@json, N'"Coverage":"Proveedor asignado para esta direcci\u00F3n"', N'"Coverage":"Assigned provider for this address"');
            SET @json = REPLACE(@json, N'"Name":"Verificar proveedor local"', N'"Name":"Verify local provider"');
            SET @json = REPLACE(@json, N'"Coverage":"Contactar para confirmar proveedor asignado"', N'"Coverage":"Contact to confirm assigned provider"');

            SET @json = REPLACE(@json, N'Sistema HVAC (Aire Acondicionado y Calefacción)', N'HVAC System (Air Conditioning and Heating)');
            SET @json = REPLACE(@json, N'Sistema HVAC (Aire Acondicionado y Calefacci\u00F3n)', N'HVAC System (Air Conditioning and Heating)');
            SET @json = REPLACE(@json, N'Calentador de Agua', N'Water Heater');
            SET @json = REPLACE(@json, N'Techo (Shingles de Asfalto)', N'Roof (Asphalt Shingles)');
            SET @json = REPLACE(@json, N'Electrodomésticos Principales', N'Major Appliances');
            SET @json = REPLACE(@json, N'Electrodom\u00E9sticos Principales', N'Major Appliances');
            SET @json = REPLACE(@json, N'Sistema de Plomería', N'Plumbing System');
            SET @json = REPLACE(@json, N'Sistema de Plomer\u00EDa', N'Plumbing System');
            SET @json = REPLACE(@json, N'Sistema Eléctrico', N'Electrical System');
            SET @json = REPLACE(@json, N'Sistema El\u00E9ctrico', N'Electrical System');
            SET @json = REPLACE(@json, N'Póliza Integral de Garantía del Hogar', N'Comprehensive Home Warranty Policy');
            SET @json = REPLACE(@json, N'P\u00F3liza Integral de Garant\u00EDa del Hogar', N'Comprehensive Home Warranty Policy');
            SET @json = REPLACE(@json, N'Garantía del Fabricante', N'Manufacturer warranty');
            SET @json = REPLACE(@json, N'Garant\u00EDa del Fabricante', N'Manufacturer warranty');
            SET @json = REPLACE(@json, N'Sin garantía activa', N'No active warranty');
            SET @json = REPLACE(@json, N'"Status":"Activa"', N'"Status":"Active"');
            SET @json = REPLACE(@json, N'"Status":"Expirada"', N'"Status":"Expired"');

            SET @json = REPLACE(@json, N'Carrier - 10 años compresor, 5 años partes', N'Carrier - 10-year compressor, 5-year parts');
            SET @json = REPLACE(@json, N'Carrier - 10 a\u00F1os compresor, 5 a\u00F1os partes', N'Carrier - 10-year compressor, 5-year parts');
            SET @json = REPLACE(@json, N'Rheem - 6 años tanque, 1 año partes', N'Rheem - 6-year tank, 1-year parts');
            SET @json = REPLACE(@json, N'Rheem - 6 a\u00F1os tanque, 1 a\u00F1o partes', N'Rheem - 6-year tank, 1-year parts');
            SET @json = REPLACE(@json, N' años restantes de 25 años', N' years remaining of 25 years');
            SET @json = REPLACE(@json, N' a\u00F1os restantes de 25 a\u00F1os', N' years remaining of 25 years');

            SET @json = REPLACE(@json, N'Cubre reparaciones y reemplazo de compresor, evaporador, condensador, motor del ventilador', N'Covers repairs and replacement of compressor, evaporator, condenser, and fan motor');
            SET @json = REPLACE(@json, N'Cubre tanque, termostato, válvula de alivio, elementos de calentamiento', N'Covers tank, thermostat, relief valve, and heating elements');
            SET @json = REPLACE(@json, N'Cubre tanque, termostato, v\u00E1lvula de alivio, elementos de calentamiento', N'Covers tank, thermostat, relief valve, and heating elements');
            SET @json = REPLACE(@json, N'Garantía limitada contra defectos de fabricación. No cubre daños por clima extremo.', N'Limited warranty against manufacturing defects. Does not cover extreme weather damage.');
            SET @json = REPLACE(@json, N'Garant\u00EDa limitada contra defectos de fabricaci\u00F3n. No cubre da\u00F1os por clima extremo.', N'Limited warranty against manufacturing defects. Does not cover extreme weather damage.');
            SET @json = REPLACE(@json, N'Cubre refrigerador, estufa, lavavajillas, microondas, lavadora y secadora', N'Covers refrigerator, stove, dishwasher, microwave, washer, and dryer');
            SET @json = REPLACE(@json, N'Cubre fugas, obstrucciones de drenaje, válvulas, grifos interiores', N'Covers leaks, drain blockages, valves, and interior faucets');
            SET @json = REPLACE(@json, N'Cubre fugas, obstrucciones de drenaje, v\u00E1lvulas, grifos interiores', N'Covers leaks, drain blockages, valves, and interior faucets');
            SET @json = REPLACE(@json, N'Cubre panel eléctrico, cableado, interruptores, tomacorrientes (hasta $1,500)', N'Covers electrical panel, wiring, switches, and outlets (up to $1,500)');
            SET @json = REPLACE(@json, N'Cubre panel el\u00E9ctrico, cableado, interruptores, tomacorrientes (hasta $1,500)', N'Covers electrical panel, wiring, switches, and outlets (up to $1,500)');
            SET @json = REPLACE(@json, N'Plan Premium: $', N'Premium plan: $');
            SET @json = REPLACE(@json, N'/año. Deducible de servicio: $125. Cobertura de sistemas principales y electrodomésticos. Llamadas de servicio ilimitadas.', N'/year. Service deductible: $125. Coverage for major systems and appliances. Unlimited service calls.');
            SET @json = REPLACE(@json, N'/a\u00F1o. Deducible de servicio: $125. Cobertura de sistemas principales y electrodom\u00E9sticos. Llamadas de servicio ilimitadas.', N'/year. Service deductible: $125. Coverage for major systems and appliances. Unlimited service calls.');

            SET @json = REPLACE(@json, N'"Type":"aire"', N'"Type":"Air conditioner"');

            UPDATE dbo.Propiedades SET DatosJson = @json WHERE Id = @id;
            FETCH NEXT FROM prop_cursor INTO @id, @json;
        END

        CLOSE prop_cursor;
        DEALLOCATE prop_cursor;
    END

    COMMIT TRANSACTION;
    PRINT 'TranslateCatalogDataToEnglish completed successfully.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
