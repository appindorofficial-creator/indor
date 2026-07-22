-- =============================================================
-- IndorDB — Seed Spanish catalog translations (*Es columns)
-- Prerequisite: AlterCatalogTablesAddSpanishColumns.sql
-- Safe to run multiple times (updates by Id / Code / Nombre).
--
-- Usage:
--   USE [IndorDB];
--   GO
--   :r SeedCatalogSpanishTranslations.sql
-- =============================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY

    /* ---------- Microservicios ---------- */
    IF OBJECT_ID(N'dbo.Microservicios', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.Microservicios SET
            NombreEs = N'Aire Seguro 365',
            SubtituloEs = N'Respira tranquilo. Nosotros nos encargamos.',
            DescripcionEs = N'Reemplazo profesional del filtro del aire acondicionado para mejorar la calidad del aire y proteger tu sistema.',
            DescripcionCompletaEs = N'Mantén tu hogar saludable y tu sistema funcionando al 100%. Reemplazamos tu filtro de AC de forma rápida y segura, ayudando a reducir polvo, alergias y averías costosas.',
            IncluyeEs = N'Revisión básica del sistema|Reemplazo de filtro|Instalación profesional',
            FrecuenciaEs = N'Cada 3 meses',
            PrecioPrefijoEs = N'Desde',
            CtaTextoEs = N'Agendar servicio'
        WHERE Id = 1;

        UPDATE dbo.Microservicios SET
            NombreEs = N'Césped Siempre Perfecto',
            SubtituloEs = N'Tu hogar se ve mejor... sin esfuerzo.',
            DescripcionEs = N'Corte de césped y mantenimiento básico para mantener tu propiedad impecable.',
            DescripcionCompletaEs = N'Nos aseguramos de que tu jardín siempre se vea limpio, ordenado y profesional. Ideal para mantener el valor de la propiedad sin perder tiempo.',
            IncluyeEs = N'Corte de césped|Limpieza básica de áreas verdes|Recolección de residuos',
            FrecuenciaEs = N'Semanal o quincenal',
            PrecioPrefijoEs = N'Desde',
            CtaTextoEs = N'Reservar mantenimiento'
        WHERE Id = 2;

        UPDATE dbo.Microservicios SET
            NombreEs = N'Basura Sin Estrés',
            SubtituloEs = N'Nunca olvides el día de recolección.',
            DescripcionEs = N'Sacamos tu basura y devolvemos los contenedores. Simple, automático y sin preocupaciones.',
            DescripcionCompletaEs = N'Olvídate de multas, malos olores o recolecciones perdidas. Nos aseguramos de que tu basura esté lista el día correcto y devolvemos los contenedores a su lugar.',
            IncluyeEs = N'Sacar contenedores el día de recolección|Devolver contenedores|Puntualidad y confianza',
            FrecuenciaEs = N'1–2 veces por semana',
            PrecioPrefijoEs = N'Mensual',
            CtaTextoEs = N'Activar servicio'
        WHERE Id = 3;

        UPDATE dbo.Microservicios SET
            NombreEs = N'Limpieza Pro',
            SubtituloEs = N'Limpieza profesional que se nota.',
            DescripcionEs = N'Servicio de limpieza para mantener tu hogar en perfectas condiciones, sin esfuerzo.',
            DescripcionCompletaEs = N'Disfruta de un hogar limpio, organizado y listo para vivir o rentar. Opciones: limpieza estándar o limpieza profunda.',
            IncluyeEs = N'Limpieza de superficies|Baños y cocina|Aspirado y trapeado',
            FrecuenciaEs = N'Semanal, quincenal o mensual',
            PrecioPrefijoEs = N'Desde',
            CtaTextoEs = N'Agendar limpieza'
        WHERE Id = 4;
    END

    /* ---------- Inspecciones ---------- */
    IF OBJECT_ID(N'dbo.Inspecciones', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección pre-compra', SubtituloEs=N'Compra con confianza. Evita errores costosos.', DescripcionEs=N'Evaluación completa antes de comprar una propiedad.', DescripcionCompletaEs=N'Analizamos la condición real del hogar antes de decidir. Detectamos problemas ocultos que podrían costar miles después de la compra.', IncluyeEs=N'Revisión estructural básica|Sistemas mecánicos (HVAC, plomería, eléctrico)|Evaluación general|Reporte detallado', FrecuenciaEs=N'Antes de cada compra', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Agendar inspección' WHERE Id=0;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección completa del hogar', SubtituloEs=N'Todo en un solo diagnóstico.', DescripcionEs=N'Revisión completa de todos los sistemas del hogar.', DescripcionCompletaEs=N'Inspeccionamos cada área clave para darte una imagen completa y prevenir fallas futuras.', IncluyeEs=N'Eléctrico|Plomería|HVAC|Estructura general', FrecuenciaEs=N'Cada 1–2 años', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Solicitar inspección' WHERE Id=1;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección eléctrica', SubtituloEs=N'Evita riesgos invisibles.', DescripcionEs=N'Revisión del sistema eléctrico por seguridad.', DescripcionCompletaEs=N'Detectamos fallas, sobrecargas o instalaciones defectuosas que pueden poner en riesgo tu hogar y familia.', IncluyeEs=N'Panel eléctrico|Cableado|Tomacorrientes y conexiones', FrecuenciaEs=N'Cada 2–3 años', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Revisar sistema eléctrico' WHERE Id=2;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección de plomería', SubtituloEs=N'Evita fugas y gastos innecesarios.', DescripcionEs=N'Evaluación del sistema de agua y drenaje.', DescripcionCompletaEs=N'Identificamos fugas, presión inadecuada y daños ocultos que pueden afectar la estructura.', IncluyeEs=N'Tuberías|Drenajes|Presión de agua', FrecuenciaEs=N'Cada 1–2 años', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Inspeccionar plomería' WHERE Id=3;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección HVAC', SubtituloEs=N'Aire limpio, sistema eficiente.', DescripcionEs=N'Revisión completa del sistema de aire acondicionado.', DescripcionCompletaEs=N'Evaluamos el rendimiento del HVAC para asegurar eficiencia, ahorro energético y confort.', IncluyeEs=N'Unidad de AC|Filtros|Funcionamiento general', FrecuenciaEs=N'Cada 6–12 meses', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Revisar aire acondicionado' WHERE Id=4;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección estructural', SubtituloEs=N'La base de tu inversión.', DescripcionEs=N'Análisis de estabilidad y seguridad de la propiedad.', DescripcionCompletaEs=N'Evaluamos posibles daños estructurales que podrían comprometer la seguridad o generar costos mayores.', IncluyeEs=N'Cimientos|Muros|Techos', FrecuenciaEs=N'Antes de comprar o remodelar', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Evaluar estructura' WHERE Id=5;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección de techo', SubtituloEs=N'Protege tu hogar desde arriba.', DescripcionEs=N'Revisión de condición del techo y fugas.', DescripcionCompletaEs=N'Detectamos desgaste, daños o fugas que pueden afectar la protección de tu hogar.', IncluyeEs=N'Tejas|Sellado|Drenaje', FrecuenciaEs=N'Cada 1–2 años', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Inspeccionar techo' WHERE Id=6;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección de cimentación', SubtituloEs=N'Evita problemas estructurales graves.', DescripcionEs=N'Evaluación de la base del hogar.', DescripcionCompletaEs=N'Identificamos grietas, asentamientos o fallas que pueden comprometer la estabilidad.', IncluyeEs=N'Base estructural|Grietas|Nivelación', FrecuenciaEs=N'Cada 2–3 años', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Inspeccionar cimentación' WHERE Id=7;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección de moho y humedad', SubtituloEs=N'Protege tu salud y tu hogar.', DescripcionEs=N'Detección de humedad y moho.', DescripcionCompletaEs=N'Localizamos problemas de humedad que pueden causar moho y afectar estructura y salud.', IncluyeEs=N'Detección de humedad|Evaluación de muros|Identificación de moho', FrecuenciaEs=N'Cuando aparezcan signos o cada 2 años', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Detectar humedad' WHERE Id=8;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección de ventanas y aislamiento', SubtituloEs=N'Ahorra energía sin notarlo.', DescripcionEs=N'Evaluación de sellado y eficiencia térmica.', DescripcionCompletaEs=N'Revisamos ventanas y aislamiento para evitar pérdida de energía y mejorar eficiencia.', IncluyeEs=N'Sellado|Aislamiento|Pérdida térmica', FrecuenciaEs=N'Cada 2–3 años', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Evaluar eficiencia' WHERE Id=9;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección de seguridad del hogar', SubtituloEs=N'Tu familia primero.', DescripcionEs=N'Revisión de sistemas de seguridad y prevención.', DescripcionCompletaEs=N'Evaluamos detectores de humo, riesgos potenciales y condiciones que comprometan la seguridad.', IncluyeEs=N'Detectores|Riesgos básicos|Recomendaciones', FrecuenciaEs=N'Anual', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Mejorar seguridad' WHERE Id=10;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección con reporte profesional', SubtituloEs=N'Decisiones basadas en datos reales.', DescripcionEs=N'Reporte detallado de la condición del hogar.', DescripcionCompletaEs=N'Recibe un reporte claro y profesional con hallazgos, fotos y recomendaciones.', IncluyeEs=N'Reporte digital|Evidencia fotográfica|Recomendaciones', FrecuenciaEs=N'Cada inspección', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Obtener reporte' WHERE Id=11;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección para inversionistas', SubtituloEs=N'Invierte inteligentemente.', DescripcionEs=N'Evaluación estratégica para compra de propiedades.', DescripcionCompletaEs=N'Analizamos propiedades desde una perspectiva de inversión para maximizar retorno y reducir riesgo.', IncluyeEs=N'Evaluación general|Riesgos potenciales|Recomendaciones de inversión', FrecuenciaEs=N'Por propiedad', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Evaluar inversión' WHERE Id=12;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección de problemas ocultos', SubtituloEs=N'Lo que no ves... es lo más peligroso.', DescripcionEs=N'Detección de fallas invisibles.', DescripcionCompletaEs=N'Identificamos daños ocultos no visibles a simple vista pero que pueden generar altos costos.', IncluyeEs=N'Evaluación profunda|Detección de riesgos|Diagnóstico técnico', FrecuenciaEs=N'Cuando se sospeche', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Detectar problemas' WHERE Id=13;
        UPDATE dbo.Inspecciones SET NombreEs=N'Inspección express', SubtituloEs=N'Rápida, clara y efectiva.', DescripcionEs=N'Revisión rápida de puntos clave del hogar.', DescripcionCompletaEs=N'Ideal para decisiones rápidas: evaluamos lo más importante en poco tiempo sin perder precisión.', IncluyeEs=N'Puntos críticos|Evaluación rápida|Recomendaciones básicas', FrecuenciaEs=N'Según necesidad', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Inspección rápida' WHERE Id=14;
    END

    /* ---------- Servicios ---------- */
    IF OBJECT_ID(N'dbo.Servicios', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.Servicios SET NombreEs=N'Cocina Soñada', SubtituloEs=N'Transforma el corazón de tu hogar.', DescripcionEs=N'Remodelación completa de cocina con diseño moderno y funcional.', DescripcionCompletaEs=N'Renovamos tu cocina para mejorar estética, funcionalidad y valor de la propiedad.', IncluyeEs=N'Diseño personalizado|Instalación de gabinetes|Acabados modernos', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=N'Desde', PrecioTextoEs=NULL, CtaTextoEs=N'Diseñar mi cocina' WHERE Id=1;
        UPDATE dbo.Servicios SET NombreEs=N'Baño Moderno Pro', SubtituloEs=N'Comodidad, estilo y valor en un solo espacio.', DescripcionEs=N'Remodelación completa de baño con acabados de alta calidad.', DescripcionCompletaEs=N'Transformamos tu baño en un espacio moderno, funcional y atractivo.', IncluyeEs=N'Instalación de accesorios|Duchas modernas|Acabados premium', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Renovar baño' WHERE Id=2;
        UPDATE dbo.Servicios SET NombreEs=N'Renovación interior total', SubtituloEs=N'Dale nueva vida a tu hogar.', DescripcionEs=N'Remodelación de espacios interiores para mayor confort y estilo.', DescripcionCompletaEs=N'Actualizamos tus espacios interiores con soluciones modernas y funcionales.', IncluyeEs=N'Rediseño de espacios|Mejoras estructurales|Acabados interiores', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Transformar interior' WHERE Id=3;
        UPDATE dbo.Servicios SET NombreEs=N'Ampliación de espacio', SubtituloEs=N'Más espacio, más valor.', DescripcionEs=N'Extensiones del hogar para aumentar el tamaño de la propiedad.', DescripcionCompletaEs=N'Creamos nuevos espacios según tus necesidades: habitaciones, oficinas o áreas sociales.', IncluyeEs=N'Diseño estructural|Construcción completa|Integración al hogar', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=NULL, PrecioTextoEs=N'Personalizado', CtaTextoEs=N'Ampliar mi hogar' WHERE Id=4;
        UPDATE dbo.Servicios SET NombreEs=N'Exteriores impactantes', SubtituloEs=N'La primera impresión importa.', DescripcionEs=N'Remodelación exterior para mejorar estética y valor.', DescripcionCompletaEs=N'Mejora la apariencia exterior de tu propiedad para aumentar atractivo y valor de mercado.', IncluyeEs=N'Fachadas|Pintura exterior|Detalles decorativos', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Mejorar exterior' WHERE Id=5;
        UPDATE dbo.Servicios SET NombreEs=N'Patio perfecto', SubtituloEs=N'Disfruta tu hogar al aire libre.', DescripcionEs=N'Diseño y construcción de patios funcionales y modernos.', DescripcionCompletaEs=N'Creamos espacios exteriores ideales para relajarte o compartir en familia.', IncluyeEs=N'Diseño personalizado|Construcción completa|Acabados duraderos', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Construir patio' WHERE Id=6;
        UPDATE dbo.Servicios SET NombreEs=N'Instalación de aire acondicionado', SubtituloEs=N'Confort todo el año.', DescripcionEs=N'Instalación profesional de sistemas HVAC.', DescripcionCompletaEs=N'Instalamos sistemas de AC eficientes para mejorar confort y ahorro energético.', IncluyeEs=N'Instalación completa|Configuración|Prueba de funcionamiento', FrecuenciaEs=N'Servicio único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Instalar sistema' WHERE Id=7;
        UPDATE dbo.Servicios SET NombreEs=N'Calentador de agua Pro', SubtituloEs=N'Agua caliente sin fallas.', DescripcionEs=N'Instalación y reemplazo de calentadores de agua.', DescripcionCompletaEs=N'Asegura agua caliente confiable con instalación profesional.', IncluyeEs=N'Instalación segura|Prueba de funcionamiento|Orientación técnica', FrecuenciaEs=N'Servicio único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Instalar calentador' WHERE Id=8;
        UPDATE dbo.Servicios SET NombreEs=N'Pisos perfectos', SubtituloEs=N'Cada paso cuenta.', DescripcionEs=N'Instalación y remodelación de pisos.', DescripcionCompletaEs=N'Renovamos tus pisos con materiales modernos y duraderos.', IncluyeEs=N'Instalación profesional|Nivelación|Acabados de calidad', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Renovar pisos' WHERE Id=9;
        UPDATE dbo.Servicios SET NombreEs=N'Pintura interior profesional', SubtituloEs=N'Renueva sin remodelar.', DescripcionEs=N'Pintura interior para transformar espacios rápidamente.', DescripcionCompletaEs=N'Dale un nuevo look a tu hogar con acabados limpios y profesionales.', FrecuenciaEs=N'Servicio único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Pintar interior' WHERE Id=10;
        UPDATE dbo.Servicios SET NombreEs=N'Pintura exterior premium', SubtituloEs=N'Protección y estilo en un solo servicio.', DescripcionEs=N'Pintura exterior resistente a la intemperie.', DescripcionCompletaEs=N'Protege tu propiedad de los elementos mientras mejoras su apariencia.', FrecuenciaEs=N'Servicio único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Pintar exterior' WHERE Id=11;
        UPDATE dbo.Servicios SET NombreEs=N'Seguridad del hogar', SubtituloEs=N'Protege lo que más importa.', DescripcionEs=N'Instalación de detectores de humo y sistemas básicos de seguridad.', DescripcionCompletaEs=N'Aumenta la seguridad de tu hogar con sistemas confiables y certificados.', FrecuenciaEs=N'Servicio único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Instalar seguridad' WHERE Id=12;
        UPDATE dbo.Servicios SET NombreEs=N'Entrada de concreto Pro', SubtituloEs=N'Durabilidad que se nota.', DescripcionEs=N'Construcción de entradas de concreto.', DescripcionCompletaEs=N'Diseñamos e instalamos entradas duraderas que mejoran funcionalidad y estética.', FrecuenciaEs=N'Proyecto único', PrecioPrefijoEs=N'Desde', CtaTextoEs=N'Construir entrada' WHERE Id=13;
    END

    /* ---------- Planes de membresía ---------- */
    IF OBJECT_ID(N'dbo.PlanesMembresia', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.PlanesMembresia SET
            NombreEs = N'Plan Básico',
            SubtituloEs = N'Cuida lo esencial de tu hogar',
            DescripcionEs = N'Acceso a microservicios básicos y notificaciones de mantenimiento.',
            CaracteristicasEs = N'1 inspección express al año|Notificaciones de garantía|Soporte por chat'
        WHERE Id = 0;

        UPDATE dbo.PlanesMembresia SET
            NombreEs = N'Plan Mensual',
            SubtituloEs = N'Tu hogar siempre protegido',
            DescripcionEs = N'Cobertura mensual con descuentos en servicios e inspecciones.',
            CaracteristicasEs = N'2 microservicios al mes|10% de descuento en servicios|Inspección anual incluida|Soporte prioritario'
        WHERE Id = 1;

        UPDATE dbo.PlanesMembresia SET
            NombreEs = N'Plan Premium',
            SubtituloEs = N'Todo incluido para tu hogar',
            DescripcionEs = N'Plan completo con inspecciones trimestrales y mantenimiento preventivo.',
            CaracteristicasEs = N'4 microservicios al mes|20% de descuento en servicios|Inspecciones trimestrales|Soporte 24/7'
        WHERE Id = 2;
    END

    /* ---------- Servicios de emergencia ---------- */
    IF OBJECT_ID(N'dbo.ServiciosEmergencia', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Emergencia HVAC',
            DescripcionEs = N'Sin calefacción, sin frío o falla del sistema.',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'HVAC';

        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Emergencia de calentador de agua',
            DescripcionEs = N'Sin agua caliente, fugas o problemas del piloto.',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'Water Heater';

        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Emergencia de plomería',
            DescripcionEs = N'Fugas, obstrucciones, rotura de tuberías y más.',
            BadgeTextoEs = N'Más solicitado',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'Plumbing';

        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Emergencia por inundación',
            DescripcionEs = N'Agua estancada, inundación de sótano y remoción urgente de agua.',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'Flood';

        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Emergencia eléctrica',
            DescripcionEs = N'Corte de energía, chispas, breakers disparados y más.',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'Electrical';

        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Emergencia de fuga en techo',
            DescripcionEs = N'Fugas activas, daño por tormenta y parches urgentes.',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'Roof Leak';

        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Emergencia por daño de árbol',
            DescripcionEs = N'Árboles caídos, ramas sobre estructuras y riesgos por tormenta.',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'Tree Damage';

        UPDATE dbo.ServiciosEmergencia SET
            TituloEmergenciaEs = N'Detectores de humo y alerta CO',
            DescripcionEs = N'Alarmas sonando, baterías agotadas, detectores faltantes y ayuda urgente.',
            CaracteristicasEs = N'Llegada rápida|Profesionales de confianza|Precios claros',
            CtaTextoEs = N'Solicitar ayuda'
        WHERE Nombre = N'Smoke Detector';
    END

    /* ---------- Categorías de proveedor ---------- */
    IF OBJECT_ID(N'dbo.IndorProveedorCategoriasCatalogo', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Eléctrico', DescriptionEs = N'Cableado, tomacorrientes, paneles e iluminación' WHERE Id = N'electrical';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Plomería', DescriptionEs = N'Tuberías, fugas, drenajes y accesorios' WHERE Id = N'plumbing';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'HVAC', DescriptionEs = N'Calefacción, ventilación y aire acondicionado' WHERE Id = N'hvac';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Mantenimiento general', DescriptionEs = N'Reparaciones y mantenimiento general' WHERE Id = N'handyman';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Construcción', DescriptionEs = N'Empresa de construcción' WHERE Id = N'construction';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Remodelación de baño', DescriptionEs = N'Remodelación de baños' WHERE Id = N'bathroom';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Remodelación de cocina', DescriptionEs = N'Remodelación de cocinas' WHERE Id = N'kitchen';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Techos', DescriptionEs = N'Reparación, reemplazo e inspección de techos' WHERE Id = N'roofing';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Pintura', DescriptionEs = N'Servicios de pintura' WHERE Id = N'painting';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Pisos', DescriptionEs = N'Instalación y reparación de pisos' WHERE Id = N'flooring';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Limpieza', DescriptionEs = N'Servicios de limpieza' WHERE Id = N'cleaning';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Paisajismo', DescriptionEs = N'Jardinería y paisajismo' WHERE Id = N'landscaping';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Control de plagas', DescriptionEs = N'Control y prevención de plagas' WHERE Id = N'pest';
        UPDATE dbo.IndorProveedorCategoriasCatalogo SET LabelEs = N'Reparación de electrodomésticos', DescriptionEs = N'Reparación de electrodomésticos' WHERE Id = N'appliance';
    END

    IF OBJECT_ID(N'dbo.IndorProveedorOfertasCatalogo', N'U') IS NOT NULL
    BEGIN
        -- Generic offerings
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalaciones' WHERE Id = N'installations';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparaciones' WHERE Id = N'repairs';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mantenimiento' WHERE Id = N'maintenance';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mejoras' WHERE Id = N'upgrades';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Inspecciones' WHERE Id = N'inspections';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Servicios de emergencia' WHERE Id = N'emergency';

        -- Handyman (Profile > Servicios ofrecidos)
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Parche y reparación de tablaroca' WHERE Id = N'drywall_patch';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Ajustes de puertas' WHERE Id = N'door_adjustments';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Montaje de TV / cuadros' WHERE Id = N'tv_mounting';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de estanterías' WHERE Id = N'shelving_install';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Ensamblaje de muebles' WHERE Id = N'furniture_assembly';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reemplazo de herrajes' WHERE Id = N'hardware_replacement';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Calafateo y sellado' WHERE Id = N'caulking_sealing';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparaciones menores de lista de pendientes' WHERE Id = N'punch_list';

        -- Construction
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Ampliaciones de vivienda' WHERE Id = N'home_additions';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Remodelaciones completas de vivienda' WHERE Id = N'full_remodels';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Armado estructural' WHERE Id = N'structural_framing';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Tablaroca' WHERE Id = N'drywall';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Trabajo de concreto' WHERE Id = N'concrete_work';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Carpintería de acabados' WHERE Id = N'finish_carpentry';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Decks y porches' WHERE Id = N'decks_porches';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Renovaciones exteriores' WHERE Id = N'exterior_renovations';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Demolición' WHERE Id = N'demolition';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Gestión de proyectos' WHERE Id = N'project_management';

        -- Bathroom remodeling
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Renovación completa de baño' WHERE Id = N'full_bathroom_renovation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de ducha / bañera' WHERE Id = N'shower_tub_install';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de tocador' WHERE Id = N'vanity_install';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Losa y pisos' WHERE Id = N'tile_flooring';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de inodoro' WHERE Id = N'toilet_install';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reemplazo de accesorios' WHERE Id = N'fixture_replacement';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Impermeabilización' WHERE Id = N'waterproofing';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Tablaroca y pintura' WHERE Id = N'drywall_paint';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mejoras de accesibilidad' WHERE Id = N'accessibility_upgrades';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de puerta de vidrio' WHERE Id = N'glass_door_install';

        -- Plumbing
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparación de fugas' WHERE Id = N'leak_repair';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza de drenajes' WHERE Id = N'drain_cleaning';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Servicio de calentador de agua' WHERE Id = N'water_heater';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de accesorios' WHERE Id = N'fixture_install';

        -- HVAC
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparación de A/C' WHERE Id = N'ac_repair';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de A/C' WHERE Id = N'ac_install';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparación de calefacción' WHERE Id = N'heating_repair';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Servicio de bomba de calor' WHERE Id = N'heat_pump';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Ductos' WHERE Id = N'ductwork';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Termostato' WHERE Id = N'thermostat';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Calidad del aire interior' WHERE Id = N'indoor_air_quality';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mini-split' WHERE Id = N'mini_split';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'HVAC comercial' WHERE Id = N'commercial_hvac';

        -- Kitchen
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Remodelación completa de cocina' WHERE Id = N'full_kitchen_remodel';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de gabinetes' WHERE Id = N'cabinet_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reemplazo de gabinetes' WHERE Id = N'cabinet_replacement';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de mesadas' WHERE Id = N'countertop_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de salpicadero' WHERE Id = N'backsplash_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Pisos' WHERE Id = N'kitchen_flooring';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Pintura y acabados' WHERE Id = N'painting_finish_work';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de electrodomésticos' WHERE Id = N'appliance_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reemplazo de fregadero y grifo' WHERE Id = N'sink_faucet_replacement';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Coordinación de iluminación y accesorios' WHERE Id = N'lighting_fixture_coordination';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Demolición' WHERE Id = N'kitchen_demolition';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Carpintería de molduras y acabados' WHERE Id = N'trim_finish_carpentry';

        -- Roofing
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reemplazo de techo de tejas' WHERE Id = N'shingle_roof_replacement';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparaciones de techo' WHERE Id = N'roof_repairs';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Techo metálico' WHERE Id = N'metal_roofing';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Techo plano' WHERE Id = N'flat_roofing';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Detección de fugas' WHERE Id = N'leak_detection';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Tapajuntas y ventilación' WHERE Id = N'flashing_ventilation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de canaletas' WHERE Id = N'gutter_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Servicio de lona de emergencia' WHERE Id = N'emergency_tarp_service';

        -- Painting
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Pintura interior' WHERE Id = N'interior_painting';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Pintura exterior' WHERE Id = N'exterior_painting';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Pintura de gabinetes' WHERE Id = N'cabinet_painting';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Preparación y parches de tablaroca' WHERE Id = N'drywall_prep_patching';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Molduras y puertas' WHERE Id = N'trim_doors';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Tinción de decks y cercas' WHERE Id = N'deck_fence_staining';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Remoción de papel tapiz' WHERE Id = N'wallpaper_removal';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Preparación con hidrolavado' WHERE Id = N'pressure_washing_prep';

        -- Flooring
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de madera' WHERE Id = N'hardwood_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Laminado' WHERE Id = N'laminate';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Vinilo / LVP' WHERE Id = N'vinyl_lvp';
        -- tile_flooring id is shared with bathroom ("Tile & Flooring"); LabelEs set in bathroom block
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de alfombra' WHERE Id = N'carpet_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparación de pisos' WHERE Id = N'floor_repair';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Reparación de subpiso' WHERE Id = N'subfloor_repair';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Acabado de pisos' WHERE Id = N'refinishing';

        -- Cleaning
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza estándar' WHERE Id = N'standard_cleaning';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza profunda' WHERE Id = N'deep_cleaning';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mudanza de entrada / salida' WHERE Id = N'move_in_move_out';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza postconstrucción' WHERE Id = N'post_construction_cleaning';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza de oficinas' WHERE Id = N'office_cleaning';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Rotación Airbnb' WHERE Id = N'airbnb_turnover';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza recurrente' WHERE Id = N'recurring_cleaning';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza de ventanas' WHERE Id = N'window_cleaning';

        -- Landscaping
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Corte de césped' WHERE Id = N'lawn_mowing';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mantenimiento de césped' WHERE Id = N'lawn_maintenance';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Acolchado' WHERE Id = N'mulching';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Plantación' WHERE Id = N'planting';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Poda de árboles' WHERE Id = N'tree_trimming';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Poda de setos' WHERE Id = N'hedge_trimming';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Instalación de césped en rollo' WHERE Id = N'sod_installation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Riego' WHERE Id = N'irrigation';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Limpieza de temporada' WHERE Id = N'seasonal_cleanup';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mantenimiento de superficies duras' WHERE Id = N'hardscape_maintenance';

        -- Pest control
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Control general de plagas' WHERE Id = N'general_pest_control';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Control de hormigas' WHERE Id = N'ant_control';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Control de cucarachas' WHERE Id = N'roach_control';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Control de roedores' WHERE Id = N'rodent_control';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Inspección de termitas' WHERE Id = N'termite_inspection';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Tratamiento de termitas' WHERE Id = N'termite_treatment';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Tratamiento de mosquitos' WHERE Id = N'mosquito_treatment';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Servicio de chinches' WHERE Id = N'bed_bug_service';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Remoción de avispas / abejas' WHERE Id = N'wasp_bee_removal';

        -- Appliance repair
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Refrigerador' WHERE Id = N'refrigerator';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Congelador' WHERE Id = N'freezer';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Lavavajillas' WHERE Id = N'dishwasher';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Horno / estufa' WHERE Id = N'oven_range';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Parrilla / cubierta' WHERE Id = N'cooktop';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Microondas' WHERE Id = N'microwave';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Lavadora' WHERE Id = N'washer';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Secadora' WHERE Id = N'dryer';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Triturador de basura' WHERE Id = N'garbage_disposal';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Máquina de hielo' WHERE Id = N'ice_maker';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Compactador de basura' WHERE Id = N'trash_compactor';
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Otros electrodomésticos pequeños' WHERE Id = N'other_small_appliances';

        -- Shared preventive maintenance id (pest / hvac)
        UPDATE dbo.IndorProveedorOfertasCatalogo SET LabelEs = N'Mantenimiento preventivo' WHERE Id = N'preventive_maintenance';
    END

    /* ---------- Categorías de solicitudes vecinales ---------- */
    IF OBJECT_ID(N'dbo.IndorNeighborRequestCategories', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.IndorNeighborRequestCategories SET LabelEs = N'Mejoras del hogar', DescriptionEs = N'Reparaciones, mantenimiento, mejoras' WHERE Code = N'home-improvements';
        UPDATE dbo.IndorNeighborRequestCategories SET LabelEs = N'Jardín y patio', DescriptionEs = N'Corte de césped, paisajismo, limpieza' WHERE Code = N'yard-patio';
        UPDATE dbo.IndorNeighborRequestCategories SET LabelEs = N'Limpieza', DescriptionEs = N'Hogar, ventanas, canaletas' WHERE Code = N'cleaning';
        UPDATE dbo.IndorNeighborRequestCategories SET LabelEs = N'Mudanza y acarreo', DescriptionEs = N'Ayuda con mudanza, remoción de basura' WHERE Code = N'moving-hauling';
        UPDATE dbo.IndorNeighborRequestCategories SET LabelEs = N'Tecnología e internet', DescriptionEs = N'Wi-Fi, dispositivos, hogar inteligente' WHERE Code = N'tech-internet';
        UPDATE dbo.IndorNeighborRequestCategories SET LabelEs = N'Otro', DescriptionEs = N'Algo más' WHERE Code = N'other';
    END

    COMMIT TRANSACTION;
    PRINT 'SeedCatalogSpanishTranslations completed successfully.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
