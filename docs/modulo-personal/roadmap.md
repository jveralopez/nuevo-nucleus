# Roadmap · Módulo Personal

## Fase 0 · Descubrimiento (completa)
- [x] Relevar clases `lib_v11.Legajo.PERSONAL` y workflow `Solicitud.WF.xml`.
- [x] Definir alcance y arquitectura objetivo.

## Fase 1 · Servicio de legajos
- [ ] Diseñar modelo de datos detallado (ER + catálogos).
- [ ] Implementar API CRUD con almacenamiento en archivo (PoC) + contract tests.
- [ ] Exponer endpoints para Liquidación (`/liquidacion-profile`).

## Fase 2 · Workflows y autoservicio
- [ ] Re-crear flujo “Cambio de Datos Personales” con Temporal/Durable Functions.
- [ ] UI de autoservicio (empleado) + UI aprobador (RRHH).
- [ ] Auditoría por atributo, replicando lista de cambios del workflow legado.

## Fase 3 · Integraciones externas
- [ ] Exportes equivalentes a `InterfacePersonal`, `InterfaceLicencias`, `InterfaceRemuneraciones`.
- [ ] Publicación de eventos `LegajoUpdated` hacia Liquidación, Payroll, Analytics.
- [ ] Importación masiva (fotos/documentos) modernizada.

## Fase 4 · Endurecimiento
- [ ] Autenticación OIDC y autorización por rol.
- [ ] Gestión de adjuntos y cifrado de datos sensibles.
- [ ] Migración a base SQL y pipelines CI/CD.

---
*Última actualización: 2026-03-09*
