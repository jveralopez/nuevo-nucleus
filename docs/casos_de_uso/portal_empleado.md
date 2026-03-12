# Casos de uso · Portal Empleado (MVP)

## CU-PORT-01 Login
- **Actor**: Empleado
- **Objetivo**: Autenticarse en el portal.
- **Precondiciones**: Usuario en Auth Service.
- **Flujo principal**:
  1. El usuario ingresa credenciales.
  2. El portal obtiene JWT.
- **Postcondicion**: Sesion iniciada.

## CU-PORT-02 Ver liquidaciones
- **Actor**: Empleado
- **Objetivo**: Consultar liquidaciones.
- **Precondiciones**: JWT valido.
- **Flujo principal**:
  1. El portal llama a Liquidacion API.
  2. El sistema devuelve lista.
- **Postcondicion**: Liquidaciones visibles.

## CU-PORT-02B Ver detalle de recibos
- **Actor**: Empleado
- **Objetivo**: Ver recibos de una liquidacion.
- **Precondiciones**: JWT valido.
- **Flujo principal**:
  1. Selecciona liquidacion.
  2. El portal llama a `/payrolls/{id}/recibos`.
- **Postcondicion**: Recibos visibles.

## CU-PORT-02C Ver notificaciones
- **Actor**: Empleado
- **Objetivo**: Ver alertas de recibos y tareas.
- **Precondiciones**: JWT valido.
- **Flujo principal**:
  1. El portal agrega notificaciones desde liquidaciones e instancias.
  2. El usuario visualiza la lista.
- **Postcondicion**: Notificaciones visibles.

## CU-PORT-02D Descargar recibos
- **Actor**: Empleado
- **Objetivo**: Exportar recibos en JSON/CSV.
- **Precondiciones**: Detalle de recibos cargado.
- **Flujo principal**:
  1. El usuario descarga JSON o CSV.
  2. El portal genera archivo local.
- **Postcondicion**: Archivo descargado.

## CU-PORT-02M Render de recibo
- **Actor**: Empleado
- **Objetivo**: Ver recibo en formato imprimible.
- **Precondiciones**: Datos del recibo disponibles.
- **Flujo principal**:
  1. Abre `recibo.html`.
  2. Renderiza datos y permite imprimir.
- **Postcondicion**: Recibo visible.

## CU-PORT-02F Descargar exportes con JWT
- **Actor**: Empleado
- **Objetivo**: Descargar exportes generados por liquidacion.
- **Precondiciones**: Exportes disponibles y JWT valido.
- **Flujo principal**:
  1. El portal consulta `/api/portal/v1/liquidacion/{id}/exports`.
  2. Descarga archivos via BFF.
- **Postcondicion**: Exportes descargados.

## CU-PORT-02G Notificaciones persistentes
- **Actor**: Empleado
- **Objetivo**: Mantener notificaciones entre sesiones.
- **Precondiciones**: BFF configurado.
- **Flujo principal**:
  1. El portal publica notificaciones en BFF.
  2. Recupera notificaciones persistidas.
- **Postcondicion**: Notificaciones persistidas.

## CU-PORT-02E Limpiar notificaciones
- **Actor**: Empleado
- **Objetivo**: Limpiar el panel.
- **Precondiciones**: Notificaciones visibles.
- **Flujo principal**:
  1. El usuario presiona "Limpiar".
  2. El portal elimina el cache local.
- **Postcondicion**: Lista vacia.

## CU-PORT-02H Marcar notificacion como leida
- **Actor**: Empleado
- **Objetivo**: Marcar una notificacion.
- **Precondiciones**: BFF configurado y notificacion pendiente.
- **Flujo principal**:
  1. El usuario marca como leida.
  2. El portal llama al BFF.
- **Postcondicion**: Notificacion actualizada.

## CU-PORT-02I Ver resumen de notificaciones
- **Actor**: Empleado
- **Objetivo**: Ver cantidad total y no leidas.
- **Precondiciones**: BFF configurado.
- **Flujo principal**:
  1. El portal consulta `/api/portal/v1/notificaciones/resumen`.
  2. Muestra totales.
- **Postcondicion**: Resumen visible.

## CU-PORT-02J Marcar todas las notificaciones
- **Actor**: Empleado
- **Objetivo**: Marcar todas como leidas.
- **Precondiciones**: BFF configurado.
- **Flujo principal**:
  1. El portal llama `/api/portal/v1/notificaciones/read-all`.
  2. El sistema marca todas como leidas.
- **Postcondicion**: Todas las notificaciones en estado leido.

## CU-PORT-02K Filtrar notificaciones no leidas
- **Actor**: Empleado
- **Objetivo**: Ver solo pendientes.
- **Precondiciones**: BFF configurado.
- **Flujo principal**:
  1. Activa el filtro.
  2. El portal consulta `?unreadOnly=true`.
- **Postcondicion**: Lista filtrada.

## CU-PORT-02L Ver resumen en dashboard
- **Actor**: Empleado
- **Objetivo**: Ver resumen total/no leidas.
- **Precondiciones**: BFF configurado.
- **Flujo principal**:
  1. El portal consulta `/api/portal/v1/notificaciones/resumen`.
  2. Muestra ratio.
- **Postcondicion**: Resumen visible.

## CU-PORT-03 Home (BFF)
- **Actor**: Empleado
- **Objetivo**: Obtener widgets de inicio.
- **Precondiciones**: JWT valido.
- **Flujo principal**:
  1. El portal llama al BFF `/api/portal/v1/home`.
  2. El BFF devuelve widgets.
- **Postcondicion**: Home renderizado.

## CU-PORT-04 Transicionar workflow
- **Actor**: Empleado
- **Objetivo**: Mover una instancia a otro estado.
- **Precondiciones**: Instancia existente, evento valido y JWT con rol Admin (MVP).
- **Flujo principal**:
  1. El usuario selecciona instancia y evento.
  2. El portal llama a `/api/portal/v1/wf/instances/{id}/transitions`.
  3. El sistema actualiza el estado.
- **Postcondicion**: Instancia transicionada.

## CU-PORT-05 Solicitud de vacaciones
- **Actor**: Empleado
- **Objetivo**: Crear solicitud de vacaciones.
- **Precondiciones**: Workflow "vacaciones" disponible y JWT con rol Admin (MVP).
- **Flujo principal**:
  1. Completa días y motivo.
  2. El portal inicia instancia via `/api/portal/v1/wf/instances`.
- **Postcondicion**: Solicitud creada.

## CU-PORT-06 Acciones rápidas en tareas
- **Actor**: Empleado
- **Objetivo**: Aprobar o rechazar instancia.
- **Precondiciones**: Instancia visible y JWT con rol Admin (MVP).
- **Flujo principal**:
  1. Selecciona instancia.
  2. Ejecuta aprobar/rechazar via `/api/portal/v1/wf/instances/{id}/transitions`.
- **Postcondicion**: Estado actualizado.

## CU-PORT-07 Cargar workflows demo
- **Actor**: Empleado Admin
- **Objetivo**: Crear definiciones base.
- **Precondiciones**: JWT con rol Admin.
- **Flujo principal**:
  1. Ejecuta "Cargar workflows".
  2. El portal crea definiciones via `/api/portal/v1/wf/definitions`.
- **Postcondicion**: Definiciones disponibles.

## CU-PORT-08 Solicitud de datos personales
- **Actor**: Empleado
- **Objetivo**: Enviar actualización de datos.
- **Precondiciones**: Workflow "datos-personales" disponible y JWT con rol Admin (MVP).
- **Flujo principal**:
  1. Completa motivo y detalle.
  2. El portal inicia instancia via `/api/portal/v1/wf/instances`.
- **Postcondicion**: Solicitud creada.

## CU-PORT-09 Reclamo
- **Actor**: Empleado
- **Objetivo**: Enviar reclamo.
- **Precondiciones**: Workflow "reclamos" disponible y JWT con rol Admin (MVP).
- **Flujo principal**:
  1. Completa asunto y detalle.
  2. El portal inicia instancia via `/api/portal/v1/wf/instances`.
- **Postcondicion**: Reclamo creado.
