# Propuesta de avances por interacción

## Objetivo
Dejar una narrativa clara de cómo evolucionó el producto en cada interacción para la demo con el cliente.

## Interacciones y avances
1. **Separación de portales**
   - Portal RH y Portal Empleado como UIs independientes.
   - Navegación y módulos iniciales.

2. **Servicios base**
   - Auth, Organización, Personal, Liquidación, Nucleus WF, Integration Hub.
   - Persistencia y JWT.

3. **Portal RH operativo**
   - CRUD de organización/personal.
   - Liquidación: crear, editar, legajos, procesar.

4. **Portal Empleado MVP**
   - Login JWT y dashboard.
   - Workflows: iniciar, transicionar y acciones rápidas.

5. **Exportes y seguridad**
   - Endpoint de exportes y descarga con JWT.
   - Hardening de exportes.

6. **Workflows ampliados**
   - Definiciones demo: vacaciones, datos personales, reclamos.
   - Solicitudes recientes en portal.

7. **RH Vacaciones/Tiempos**
   - RRHH aprueba/rechaza vacaciones en WF.
   - RRHH dispara jobs de exporte de horas.

8. **Recibos y notificaciones**
   - Detalle de recibos con desglose.
   - Notificaciones en dashboard.

9. **Export local y limpieza**
   - Descarga local JSON/CSV.
   - Limpieza de notificaciones.

10. **Persistencia real + exportes empleado**
   - Notificaciones persistentes en Portal BFF (SQLite).
   - Descarga de exportes con JWT desde Liquidación.

11. **Notificaciones leídas**
   - Marcar leída desde Portal Empleado.
   - Filtro de no leídas disponible en BFF.

12. **Resumen de notificaciones**
   - Conteo total/no leidas para dashboard.

13. **Notificaciones a escala**
   - Filtro/paginado en API.
   - Marcar todas como leidas.

14. **Validacion BFF**
   - Pruebas automatizadas para notificaciones y paginado.

15. **UI de notificaciones**
   - Resumen, filtro de no leidas y acciones masivas.

16. **Exportes centralizados**
   - Proxy de exportes por BFF.

17. **Workflows centralizados**
   - Proxy de definiciones e instancias por BFF.

18. **Integraciones RH**
   - Detalle de job y reintentos en integraciones.

19. **Liquidación centralizada**
   - Recibos y exportes via BFF.

20. **RH centralizado**
   - Proxy de organización, personal, liquidación e integraciones.

21. **Tests core RH**
   - Validación de endpoints RH vía BFF.

22. **Eventos de integración**
   - Registro y consulta de eventos de jobs.

23. **Core integraciones**
   - Eventos y auditoría listos para demo.

24. **Triggers de integraciones**
   - Alta y ejecución manual de triggers.

25. **Render de recibo**
   - HTML imprimible para demo.

26. **Triggers en RH**
   - Creación y ejecución desde UI.

27. **Edición de triggers**
   - Actualización de configuración en UI.

28. **Orden UI**
   - Hints de BFF y consistencia visual.

29. **Preparación producción**
   - Dockerfiles, compose y checklist.

## Cierre core
- UX validaciones finales en RH y Empleado.
- Demo script final actualizado.

## Uso en demo
Esta propuesta puede usarse como guion de demo, mostrando cada interacción como un paso de valor incremental.
