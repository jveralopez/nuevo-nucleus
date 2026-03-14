# API implementada · Tiempos Trabajados

## Ingesta de fichadas
- `POST /fichadas`
```json
{
  "legajoId": "00000000-0000-0000-0000-000000000011",
  "fechaHora": "2026-03-09T08:01:12Z",
  "tipo": "Entrada",
  "origen": "terminal",
  "observaciones": "Ingreso normal"
}
```
- `GET /fichadas?legajoId=&desde=&hasta=`
- `GET /fichadas/{id}`
- `PATCH /fichadas/{id}` para correcciones.

## Horarios / turnos
- `GET /turnos`, `POST /turnos`, `PUT /turnos/{id}`, `DELETE /turnos/{id}`.
- `GET /horarios`, `POST /horarios`, `PUT /horarios/{id}`, `DELETE /horarios/{id}`.

## Planillas
- `GET /planillas?periodo=&empresaId=`
- `POST /planillas`
```json
{
  "periodo": "2026-02",
  "empresaId": "00000000-0000-0000-0000-000000000010",
  "detalles": [
    {
      "legajoId": "00000000-0000-0000-0000-000000000011",
      "horasNormales": 160,
      "horasExtra": 10,
      "horasAusencia": 2,
      "observaciones": "Planilla demo"
    }
  ]
}
```
- `GET /planillas/{id}`
- `POST /planillas/{id}/cerrar`

## Seguridad
- JWT emitido por `auth-service`.
- Lecturas: usuario autenticado.
- Escrituras: rol `Admin`.

---
*Los endpoints avanzados (procesamientos, novedades, exportes) quedan para una fase siguiente.*
