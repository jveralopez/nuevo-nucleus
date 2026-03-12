# Catalogos de conceptos

## Origen
Exportados desde el sistema legado 23.01.

## Archivos
- `conceptos_23.01.json`
- `conceptos_sicoss_23.01.json`

## Generacion
```
python tools/catalogos/export_conceptos.py \
  --xml C:\proyectos\23.01\23.01\Conceptos\Conceptos.xml \
  --source 23.01 \
  --output data/catalogos/conceptos_23.01.json

python tools/catalogos/export_conceptos.py \
  --xml C:\proyectos\23.01\23.01\Conceptos\Conceptos_SICOSS.xml \
  --source 23.01 \
  --output data/catalogos/conceptos_sicoss_23.01.json
```
