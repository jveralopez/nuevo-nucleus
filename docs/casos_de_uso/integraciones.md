# Casos de uso · Integraciones (Integration Hub)

## CU-INT-01 Crear template de integracion
- **Actor**: Analista de integraciones
- **Objetivo**: Definir un template con origen, transformacion y destino.
- **Precondiciones**: Acceso a Integration Hub.
- **Flujo principal**:
  1. El actor carga nombre, version y schedule.
  2. Define source (type, connection, query).
  3. Define transform (type, template).
  4. Define destination (type, connection, path).
  5. El sistema guarda el template en estado Draft.
- **Postcondicion**: Template disponible para publicacion.

## CU-INT-02 Publicar template
- **Actor**: Analista de integraciones
- **Objetivo**: Habilitar un template para ejecucion.
- **Precondiciones**: Template existente en Draft.
- **Flujo principal**:
  1. El actor selecciona template.
  2. El sistema cambia estado a Publicado.
- **Postcondicion**: Template disponible para jobs.

## CU-INT-03 Crear conexion
- **Actor**: Admin de integraciones
- **Objetivo**: Registrar credenciales para conectores.
- **Precondiciones**: Acceso admin.
- **Flujo principal**:
  1. El actor carga name, type, host, username, secretId.
  2. El sistema guarda la conexion.
- **Postcondicion**: Conexion disponible en templates.

## CU-INT-04 Ejecutar job manual
- **Actor**: Operador de integraciones
- **Objetivo**: Ejecutar un template para un periodo.
- **Precondiciones**: Template publicado.
- **Flujo principal**:
  1. El actor selecciona template y periodo.
  2. El sistema crea job en EnProceso.
  3. El sistema genera archivo de salida.
  4. El sistema marca job como Completado.
- **Postcondicion**: Archivo generado y job auditado.

## CU-INT-05 Reintentar job fallido
- **Actor**: Operador de integraciones
- **Objetivo**: Reintentar una ejecucion fallida.
- **Precondiciones**: Job con estado Fallido.
- **Flujo principal**:
  1. El actor ejecuta retry.
  2. El sistema reinicia el job.
  3. El sistema actualiza estado y resultado.
- **Postcondicion**: Job actualizado.

## CU-INT-06 Acceso autenticado
- **Actor**: Usuario autenticado
- **Objetivo**: Acceder a templates y jobs con JWT valido.
- **Precondiciones**: Token emitido por Auth Service.
- **Flujo principal**:
  1. El actor incluye token en Authorization.
  2. El sistema valida token.
  3. El sistema permite la consulta.
- **Postcondicion**: Respuesta con datos solicitados.

## CU-INT-07 Exportar banco Galicia
- **Actor**: Operador de integraciones
- **Objetivo**: Ejecutar template Banco Galicia.
- **Precondiciones**: Template publicado y conexión SFTP creada.
- **Flujo principal**:
  1. El actor ejecuta job con periodo.
  2. El sistema genera archivo batch.
  3. El sistema deja registro del job.
- **Postcondicion**: Archivo disponible.
