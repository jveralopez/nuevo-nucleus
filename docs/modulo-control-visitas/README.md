# Módulo Control de Visitas · Blueprint

## Objetivo
Modernizar el módulo “Control de Visitas” (referenciado en `Application.xml`) para gestionar visitas corporativas: ingreso, acreditación, autorización, historial y reportes de seguridad.

## Visión actual (23.01)
- Formularios Nomad y definiciones en `Class/NucleusRH/Base/Control_de_Visitas/*` (ABM de visitas, agenda, registros).
- Integraciones con Seguridad/Recepción via interfaces.
- Funcionalidades: registrar visitas, generar badges, asociar empleados anfitriones, reportes de ingreso/egreso.

## Propuesta moderna
1. **Visitas API**: microservicio para preregistro, autorizaciones, check-in/out y credenciales.
2. **Portal / App Recepción**: UI para recepción/seguridad y autoservicio (enviar invitaciones, QR, notificaciones).
3. **Integraciones**: con Legajos (anfitriones), Seguridad física (control de acceso), calendario corporativo (invitaciones), Integrations Hub (reportes, exportes legales).

---
*Basado en conocimiento de módulos similares y referencias en Application.xml.*
