# Ganancias - herramientas de importacion

## Objetivo
Convertir tablas oficiales (Art. 94 y Art. 30) a JSON versionado para el motor de reglas.

## Formato CSV - Art. 94 (escala)
Archivo con cabecera:

```
desde,hasta,cuota_fija,alicuota
0,1635136.56,0,0.05
1635136.56,3270273.12,81756.83,0.09
```

Notas:
- `alicuota` en formato decimal (0.05 = 5%).
- `desde` y `hasta` en moneda local sin separadores.

## Formato CSV - Art. 30 (deducciones personales)
Archivo con cabecera:

```
codigo,descripcion,importe,unidad
GAN_NO_IMPONIBLE,Ganancia no imponible,0,ARS
```

## Uso

Art. 94 (CSV en `data/ganancias/art94.csv`):
```
python tools/ganancias/import_art94_from_csv.py \
  --csv data/ganancias/art94.csv \
  --version 2026-S1 \
  --effective-from 2026-01-01 \
  --effective-to 2026-06-30 \
  --source-url "https://www.afip.gob.ar/.../Tabla-Art-94-...pdf" \
  --source-published-at 2026-01-10
```

Art. 30 (CSV en `data/ganancias/art30.csv`):
```
python tools/ganancias/import_art30_from_csv.py \
  --csv data/ganancias/art30.csv \
  --version 2026-S1 \
  --effective-from 2026-01-01 \
  --effective-to 2026-06-30 \
  --source-url "https://www.afip.gob.ar/.../Deducciones-Art-30-...pdf" \
  --source-published-at 2026-01-10
```

## Salidas
- `data/reglas/ganancias/art94.json`
- `data/reglas/ganancias/art30.json`

## Extracción automática desde PDF
```

## Validación
```
python tools/ganancias/validate_tables.py \
  --art94 data/ganancias/art94.csv \
  --art30 data/ganancias/art30.csv
```
python tools/ganancias/extract_pdf_tables.py \
  --tipo art94 \
  --url "https://www.afip.gob.ar/gananciasYBienes/ganancias/personas-humanas-sucesiones-indivisas/declaracion-jurada/documentos/Tabla-Art-94-LIG-per-ene-a-jun-2026.pdf" \
  --output data/ganancias/art94.csv

python tools/ganancias/extract_pdf_tables.py \
  --tipo art30 \
  --url "https://www.afip.gob.ar/gananciasYBienes/ganancias/personas-humanas-sucesiones-indivisas/deducciones/documentos/Deducciones-personales-art-30-ene-a-jun-2026.pdf" \
  --output data/ganancias/art30.csv
```
