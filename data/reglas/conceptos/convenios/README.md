# Reglas por convenio

Colocá un archivo por convenio con el nombre normalizado.
Ejemplo: `cct-130-75.json` para el convenio CCT-130-75.

## Formato
```
{
  "version": "2026-S1",
  "effective_from": "2026-01-01",
  "effective_to": "2026-06-30",
  "rules": [
    {
      "codigo": "ADIC_CONV",
      "descripcion": "Adicional convenio",
      "tipo": "Remunerativo",
      "formula": "PercentOf",
      "base": "basico",
      "rate": 0.0,
      "activo": false,
      "afectaGanancias": true,
      "source": "convenio"
    }
  ]
}
```

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
