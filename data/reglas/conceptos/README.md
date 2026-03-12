# Reglas de conceptos

## Base
- `reglas.json` contiene reglas generales aplicables a todos los convenios.

## Por convenio
- `convenios/<convenio>.json` agrega reglas específicas.

## Generación desde catálogo legado
```
python tools/catalogos/generar_reglas_desde_catalogo.py \
  --catalogo data/catalogos/conceptos_23.01.json \
  --output data/reglas/conceptos/convenios/cct-130-75.json \
  --version 2026-S1 \
  --effective-from 2026-01-01 \
  --effective-to 2026-06-30 \
  --inactive
```

Las reglas generadas vienen inactivas y deben completarse con fórmulas reales.
