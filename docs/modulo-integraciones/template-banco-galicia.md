# Template real · Banco Galicia

## Proposito
Exportar archivos de liquidacion para Banco Galicia en formato batch.

## Contenido
- **Origen**: SQL (query `galicia.sql`).
- **Transformacion**: Liquid template `galicia.liquid`.
- **Destino**: SFTP `galicia-sftp`.

## Archivo JSON
Ver `docs/modulo-integraciones/template-banco-galicia.json`.

## Ejecucion manual (MVP)
1. Crear conexion `galicia-sftp`.
2. Crear template con el JSON.
3. Publicar template.
4. Ejecutar job con periodo.
