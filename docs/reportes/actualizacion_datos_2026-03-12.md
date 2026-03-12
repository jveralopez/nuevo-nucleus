# Actualizacion de datos 2026-03-12

## Validacion de CSV
```
python tools/ganancias/validate_tables.py \
  --art94 data/ganancias/art94.csv \
  --art30 data/ganancias/art30.csv
```
Resultado: `Tablas OK`.

## Regeneracion de JSON
```
python tools/ganancias/import_art94_from_csv.py \
  --csv data/ganancias/art94.csv \
  --version 2026-S1 \
  --effective-from 2026-01-01 \
  --effective-to 2026-06-30 \
  --source-url "https://www.afip.gob.ar/gananciasYBienes/ganancias/personas-humanas-sucesiones-indivisas/declaracion-jurada/documentos/Tabla-Art-94-LIG-per-ene-a-jun-2026.pdf" \
  --source-published-at 2026-01-10

python tools/ganancias/import_art30_from_csv.py \
  --csv data/ganancias/art30.csv \
  --version 2026-S1 \
  --effective-from 2026-01-01 \
  --effective-to 2026-06-30 \
  --source-url "https://www.afip.gob.ar/gananciasYBienes/ganancias/personas-humanas-sucesiones-indivisas/deducciones/documentos/Deducciones-personales-art-30-ene-a-jun-2026.pdf" \
  --source-published-at 2026-01-10
```

Salidas:
- `data/reglas/ganancias/art94.json`
- `data/reglas/ganancias/art30.json`
