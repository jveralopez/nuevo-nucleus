# API de Liquidación

Base URL por defecto: `http://localhost:5188` (configurable vía `ASPNETCORE_URLS`). Todas las respuestas son JSON.

## Modelos
- `PayrollRun`: `{ id, periodo, tipo, estado, createdAt, updatedAt, legajos: [], recibos: [] }`
- `Legajo`: `{ id, numero, nombre, cuil, basico, antiguedad, adicionales, descuentos }`
- `Recibo`: `{ id, payrollRunId, legajoId, remunerativo, deducciones, neto, detalle[] }`

## Endpoints

### GET /health
Devuelve `{ status: "ok", service: "liquidacion-service", version }`.

### GET /payrolls
Lista todas las liquidaciones registradas.

### POST /payrolls
Crea una liquidación.
```json
{
  "periodo": "2026-02",
  "tipo": "Mensual",
  "descripcion": "Mensualizados Casa Central"
}
```

### GET /payrolls/{id}
Detalle de una liquidación (incluye legajos y recibos si existen).

### POST /payrolls/{id}/legajos
Agrega un legajo al lote.
```json
{
  "numero": "00045",
  "nombre": "Ana Nieto",
  "cuil": "27-12345678-9",
  "basico": 520000,
  "antiguedad": 24000,
  "adicionales": 38000,
  "descuentos": 16000
}
```

### DELETE /payrolls/{id}/legajos/{legajoId}
Elimina un legajo antes de procesar.

### POST /payrolls/{id}/procesar
Calcula recibos para todos los legajos del lote y cambia estado a `Procesado`.
Opcionalmente acepta `{ "exportar": true }` para generar archivos en `liquidacion-service/storage/exports/`.

### GET /payrolls/{id}/recibos
Lista los recibos generados para la liquidación.

### GET /exports/{fileName}
Sirve los archivos generados (CSV/JSON) para integraciones.

## Estados de la liquidación
`Draft -> Calculando -> Procesado -> Exportado`. Cada endpoint valida la transición.

## Reglas de negocio iniciales
- Remunerativo = `basico + antiguedad + adicionales`.
- Deducciones = `descuentos + (remunerativo * 0.11) + (remunerativo * 0.03) + (remunerativo * 0.02)`.
- Neto = `remunerativo - deducciones`.
- Cualquier valor negativo se normaliza a 0.

---
*La API reemplaza métodos heredados como `LiquidacionDDO.LIQUIDACION_DDO.RemoteStartLiq.Method.cs` y `lib_v11.Empresa_Liquidacion.EMPRESA.cs`.*
