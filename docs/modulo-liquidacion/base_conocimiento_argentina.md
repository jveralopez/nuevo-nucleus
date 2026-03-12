# Base de conocimiento - Liquidacion de sueldos Argentina

## Objetivo
Documentar la base funcional minima para liquidar sueldos en Argentina y servir como referencia unica para reglas, formularios y actualizaciones oficiales.

## Alcance
- Liquidacion mensual, anual e informativa de Impuesto a las Ganancias (4ta categoria - relacion de dependencia).
- Deducciones personales y escalas oficiales.
- Certificados/formatos oficiales para reportes.
- Reglas de vigencia y actualizacion semestral.

## Marco normativo (fuentes oficiales)
- ARCA/AFIP - Regimen de retencion de Ganancias (RG 4003):
  - https://biblioteca.afip.gob.ar/search/query/norma.aspx?p=t%3ARAG%7Cn%3A4003%7Co%3A3%7Ca%3A2017%7Cf%3A02%2F03%2F2017
- ARCA/AFIP - Ganancias (determinacion, escalas Art. 94):
  - https://www.afip.gob.ar/gananciasYBienes/ganancias/personas-humanas-sucesiones-indivisas/declaracion-jurada/determinacion.asp
- ARCA/AFIP - Portal Ganancias y Bienes:
  - https://www.afip.gob.ar/gananciasYBienes/
- Manual oficial F.1359 (Ganancias 4ta, relacion de dependencia):
  - https://www.afip.gob.ar/572web/documentos/F1359-Version-00200-Manual-001.pdf

## Flujo general de liquidacion
1. Recolectar datos base de legajo, empresa, convenios, conceptos y novedades.
2. Calcular remunerativos y no remunerativos segun conceptos del convenio.
3. Aplicar deducciones obligatorias (aportes jubilatorios, obra social, etc.).
4. Calcular Ganancias 4ta segun RG 4003 (base imponible, deducciones personales, escala Art. 94).
5. Generar recibo, retenciones e informes oficiales.
6. Registrar version de reglas aplicada y vigencia.

## Ganancias 4ta categoria (resumen funcional)
- Base imponible: remuneracion alcanzada por el regimen de retencion.
- Deducciones personales: Art. 30 (ganancia no imponible, cargas de familia, deduccion especial, etc.).
- Escala: Art. 94 (tramos, cuota fija y alicuota).
- Periodicidad: calculo mensual con ajuste anual/final (RG 4003).
- Ajustes retroactivos: tratamiento especifico (RG 4003, Art. 9).
- Multiempleador: designacion de agente de retencion (RG 4003, Art. 3).

## Formularios y reportes oficiales
- F.1359: certificado anual/informativo/final de ingresos y retenciones.
- Reportes IG heredados (referencia legado):
  - `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Interface\LiqIG4TA\form.xml`
  - `C:\proyectos\23.01\23.01\InterfacesOut\Definitions\liqig4ta.def.xml`

## Retenciones judiciales y embargos (legado)
- Existe un circuito de embargos con tipos:
  - **Cuota alimentaria (CA)**: porcentaje sobre sueldo bruto/neto o por concepto.
  - **Otras causas (OC)**: monto total y pendiente.
- Interfaces y formularios legado:
  - Importacion embargos: `C:\proyectos\23.01\23.01\Class\NucleusRH\Base\Liquidacion\lib_v11.EntradaEmbargos.ENTRADAEMBARGOS.cs`
  - ABM embargos: `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Legajo\Legajo.Embargos.form.xml`
  - Reporte listado embargos: `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Reporte\ListadoEmbargos\filter.form.xml`
  - Contestacion de oficio: `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Reporte\ContestaOficio\filter.form.xml`
- Este flujo debe reimplementarse en el core moderno (modelo de embargo + reglas de calculo + eventos + reportes).

## Otros items frecuentes a contemplar (lista inicial)
- Anticipos y cuotas.
- Licencias con goce/sin goce (impacto en remunerativo).
- SAC y prorrateos.
- Asignaciones familiares.
- Topes y bases por obra social/jubilacion/sindicatos.
- Ajustes retroactivos.

## Principios de performance (legado)
- Procesamiento batch y carga masiva de items de Ganancias (interfaces y lotes).
- Importacion de rangos y topes por periodo (actualizacion centralizada).
- Minimizar IO por legajo durante el calculo (pre-carga de datos y cache de reglas).

## Vinculos con implementacion actual
- Regla inicial en `liquidacion-service/Services/ReceiptCalculator.cs` es un placeholder y no cubre Ganancias.
- La version moderna debe incorporar reglas versionadas y vigentes por periodo.
- Herramientas de importacion y versionado en `tools/ganancias/README.md`.
