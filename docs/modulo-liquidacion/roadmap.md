# Roadmap del módulo de Liquidación

## Fase 0 · Fundaciones (completa)
- [x] Relevar código legado (`Class/NucleusRH/Base/Liquidacion/*`).
- [x] Documentar alcance y arquitectura objetivo.

## Fase 1 · Servicio base (en curso)
- [ ] Construir API .NET 8 (`liquidacion-service`).
- [ ] Implementar repositorio file-store + contratos DDD.
- [ ] Test manual vía HTTP client y UI básica.

## Fase 2 · UI operativa
- [ ] Diseñar vista dedicada de Liquidación (cards + tabla de legajos).
- [ ] Acciones: crear lote, agregar/quitar legajo, procesar, descargar recibos.
- [ ] Mostrar estado por etapas y logs de cálculo.

## Fase 3 · Integraciones
- [ ] Generar archivos CSV/JSON similares a `Empresa_Liquidacion.InterfaceContable.XML`.
- [ ] Preparar hooks/eventos (Service Bus/Kafka) para contabilidad y bancos.
- [ ] Añadir endpoints seguros para download.

## Fase 4 · Conectividad y seguridad
- [ ] Autenticación OIDC.
- [ ] Auditoría completa de operaciones.
- [ ] Deploy container + pipeline CI/CD.

---
*Última actualización: 2026-03-09*
