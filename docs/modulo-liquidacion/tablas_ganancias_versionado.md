# Tablas de Ganancias - esquema versionado

## Objetivo
Establecer el esquema de datos para versionar tablas oficiales (Art. 94 y Art. 30) con vigencias y trazabilidad de fuentes.

## Reglas generales
- Cada tabla tiene `effective_from` y `effective_to`.
- Las fuentes se guardan con URL y fecha de publicacion.
- Se permite coexistencia de versiones por semestre.

## Art. 94 - Escala de retencion (schema)
Archivo recomendado: `data/reglas/ganancias/art94.schema.json`

```json
{
  "version": "YYYY-S1|YYYY-S2",
  "effective_from": "YYYY-MM-DD",
  "effective_to": "YYYY-MM-DD",
  "source_url": "https://www.afip.gob.ar/.../Tabla-Art-94-....pdf",
  "source_published_at": "YYYY-MM-DD",
  "rows": [
    {
      "desde": 0,
      "hasta": 0,
      "cuota_fija": 0,
      "alicuota": 0
    }
  ]
}
```

## Art. 30 - Deducciones personales (schema)
Archivo recomendado: `data/reglas/ganancias/art30.schema.json`

```json
{
  "version": "YYYY-S1|YYYY-S2",
  "effective_from": "YYYY-MM-DD",
  "effective_to": "YYYY-MM-DD",
  "source_url": "https://www.afip.gob.ar/.../Deducciones-personales-art-30-....pdf",
  "source_published_at": "YYYY-MM-DD",
  "rows": [
    {
      "codigo": "GAN_NO_IMPONIBLE",
      "descripcion": "Ganancia no imponible",
      "importe": 0,
      "unidad": "ARS"
    }
  ]
}
```

## Ubicacion recomendada
- `data/reglas/ganancias/art94.schema.json`
- `data/reglas/ganancias/art30.schema.json`

## Notas
- Los valores deben cargarse desde los PDFs oficiales publicados por ARCA/AFIP.
- Cada liquidacion debe registrar la version aplicada.
