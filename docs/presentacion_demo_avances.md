# Presentacion de avances · Nucleus RH Next

## Objetivo
Mostrar la evolucion del producto por iteraciones, con foco en valor funcional y demo al cliente.

## Iteraciones realizadas
1. **Fundacion del producto**
   - Separacion de portales: Portal RH vs Portal Empleado.
   - Servicios base .NET 8: auth, organizacion, personal, liquidacion, WF, integration hub.
   - Scripts de arranque para demo.

2. **Portal RH operativo**
   - CRUD organizacion/personal.
   - Liquidacion: alta, edicion, legajos, procesamiento, exportes, detalle.
   - Integraciones: jobs visibles.

3. **Portal Empleado MVP**
   - Login JWT y dashboard.
   - Workflows: iniciar instancia, transicionar y acciones rapidas.
   - Solicitud de vacaciones.

4. **Exportes y seguridad**
   - Endpoint `GET /payrolls/{id}/exports`.
   - Descarga con JWT en UI RH.
   - Hardening de exportes (evitar path traversal).
   - Tests de exportes en liquidacion-service.

5. **Workflows ampliados**
   - Definiciones demo para vacaciones, datos personales y reclamos.
   - Solicitudes y lista de solicitudes recientes.

6. **Vacaciones y tiempos en RH**
   - RH aprueba/rechaza solicitudes de vacaciones via WF.
   - RH dispara jobs de exporte de horas via Integration Hub.

7. **Recibos detalle y notificaciones**
   - Portal Empleado muestra detalle de recibos con remunerativo/deducciones/neto.
   - Notificaciones en dashboard derivadas de liquidaciones e instancias.

8. **Descarga de recibos**
   - Export local en JSON/CSV desde el detalle.
   - Limpieza de notificaciones en UI.

9. **Notificaciones persistidas**
   - Cache en localStorage para mantener alertas entre recargas.

10. **Notificaciones persistentes + exportes con JWT**
    - Portal BFF guarda notificaciones en SQLite.
    - Portal Empleado descarga exportes desde `/payrolls/{id}/exports/empleado`.

11. **Notificaciones leidas**
    - Portal Empleado marca notificaciones como leidas via BFF.
    - Filtro opcional de no leidas.

12. **Resumen de notificaciones**
    - BFF expone conteo total y no leidas.

13. **Operaciones masivas de notificaciones**
    - Paginado y filtro en API.
    - Marcar todas como leidas.

14. **Tests BFF**
    - Cobertura de CRUD y paginado de notificaciones.

15. **Dashboard de notificaciones**
    - Resumen, filtro de no leidas y marcar todas.

16. **Exportes via BFF**
    - Portal Empleado consume exportes desde BFF.

17. **Workflows via BFF**
    - Portal BFF proxy de definiciones e instancias.

18. **Integraciones RH avanzadas**
    - Detalle de job y reintentos desde Portal RH.

19. **Liquidación via BFF**
    - Recibos via BFF para Portal Empleado.

20. **RH via BFF**
    - Organizacion, personal, liquidacion e integraciones centralizados.

21. **Tests core RH**
    - Cobertura BFF de endpoints RH clave.

22. **Eventos de integración**
    - Auditoría de jobs con eventos persistidos.

23. **Core de integraciones**
    - Endpoints y eventos consolidados.

24. **Triggers de integraciones**
    - Triggers configurables y ejecución manual.

25. **Recibo imprimible**
    - HTML dedicado para render de recibo.

26. **Triggers en RH UI**
    - Alta y ejecución de triggers desde portal.

27. **Edición de triggers**
    - Ajuste de eventName/template en UI.

28. **Orden UI**
    - Hints y uso de BFF clarificados.

29. **Preparación producción**
    - Dockerfiles, compose y checklist.

## Guion demo final
1. Login demo en Portal RH.
2. Crear empresa, unidad y posición.
3. Crear legajo y asignar posición.
4. Crear liquidación, agregar legajo y procesar.
5. Exportar y descargar recibos.
6. Ver eventos de integración y reintentar job.
7. Crear trigger y ejecutar.
8. Login en Portal Empleado.
9. Ver recibos y abrir `recibo.html`.
10. Crear solicitudes y ver notificaciones.

Nota: los endpoints RH vía BFF requieren rol Admin.

## Demo sugerida (flow)
1. Login demo en Portal RH.
2. Crear empresa/unidad/legajo y asignar posicion.
3. Crear liquidacion, agregar legajo y procesar/exportar.
4. Ver detalle y descargar exportes.
5. Iniciar job de exporte de horas.
6. Login en Portal Empleado.
7. Cargar workflows demo y crear solicitudes.
8. Revisar tareas y notificaciones.
9. Volver a RH y aprobar/rechazar vacaciones.

## Estado actual
- Portal RH: organizado, con liquidacion y flujos RH basicos.
- Portal Empleado: autoservicio con solicitudes y recibos con detalle.
- Servicios: APIs principales funcionando con JWT.

## Proximos pasos
- Recibos con descarga y firmas.
- Notificaciones persistentes y canal tiempo real.
- Modulos RH restantes (tiempos/vacaciones con servicios propios).
