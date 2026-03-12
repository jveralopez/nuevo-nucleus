# Proceso de actualizacion - Ganancias

## Objetivo
Mantener tablas oficiales de Ganancias actualizadas (Art. 94 y Art. 30) con vigencias y trazabilidad.

## Flujo
1. Descargar PDFs oficiales de ARCA/AFIP (tabla Art. 94 y deducciones Art. 30).
2. Extraer valores a CSV con el formato definido en `tools/ganancias/README.md`.
   - CSV base: `data/ganancias/art94.csv` y `data/ganancias/art30.csv`.
   - Script automático: `tools/ganancias/extract_pdf_tables.py`.
3. Ejecutar importadores y generar JSON versionado.
4. Registrar version y vigencia en la liquidacion aplicada.
5. Publicar release notes internas para todos los clientes.

## Fuentes oficiales
- RG 4003 (regimen de retencion):
  - https://biblioteca.afip.gob.ar/search/query/norma.aspx?p=t%3ARAG%7Cn%3A4003%7Co%3A3%7Ca%3A2017%7Cf%3A02%2F03%2F2017
- Ganancias - determinacion (ARCA):
  - https://www.afip.gob.ar/gananciasYBienes/ganancias/personas-humanas-sucesiones-indivisas/declaracion-jurada/determinacion.asp
- Manual F.1359:
  - https://www.afip.gob.ar/572web/documentos/F1359-Version-00200-Manual-001.pdf

## Artefactos generados
- `data/reglas/ganancias/art94.json`
- `data/reglas/ganancias/art30.json`

## Control de calidad
- Validar que el JSON tenga rangos completos y no solapados.
- Mantener las versiones anteriores para recalculos retroactivos.
