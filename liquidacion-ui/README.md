# Liquidacion UI

Interfaz ligera (HTML + JS) que consume `liquidacion-service`.

## Uso
1. Ejecutá `liquidacion-service` en `http://localhost:5188` (o la URL que definas).
2. Abrí `index.html` en tu navegador.
3. Configurá una URL distinta guardándola en `localStorage.liquidacion_api` desde la consola del navegador si fuera necesario.

## Funcionalidades
- Crear liquidaciones con periodo/tipo/descripcion.
- Agregar o quitar legajos antes de procesar.
- Procesar y exportar recibos.
- Visualizar recibos generados (remunerativo y neto por legajo).
