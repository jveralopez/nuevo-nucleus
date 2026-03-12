# Casos de prueba · Liquidacion -> Integration Hub

## CP-LIQ-INT-01 Exportar sin IntegrationHub configurado
- **Dado** IntegrationHub.TemplateId vacio
- **Cuando** procesa liquidacion con exportar
- **Entonces** se exporta liquidacion y se omite trigger

## CP-LIQ-INT-02 Exportar con IntegrationHub configurado
- **Dado** BaseUrl y TemplateId configurados
- **Cuando** procesa liquidacion con exportar
- **Entonces** dispara job y no falla el procesamiento
