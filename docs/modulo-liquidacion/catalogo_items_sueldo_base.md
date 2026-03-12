# Catalogo base de items de sueldo (Argentina)

## Nota clave
No existe un listado unico y universal de "todos los items" de liquidacion en Argentina.
El catalogo completo se construye combinando:
1) Normativa legal obligatoria (aportes, contribuciones, retenciones, embargos).
2) Convenios colectivos (CCT) por actividad/empresa.
3) Politicas internas del cliente (beneficios, premios, bonos, adicionales).

## Fuentes oficiales para el nucleo obligatorio
- RG 4003 (retencion Ganancias): https://biblioteca.afip.gob.ar/search/query/norma.aspx?p=t%3ARAG%7Cn%3A4003%7Co%3A3%7Ca%3A2017%7Cf%3A02%2F03%2F2017
- ARCA/AFIP - Ganancias (escalas Art. 94): https://www.afip.gob.ar/gananciasYBienes/ganancias/personas-humanas-sucesiones-indivisas/declaracion-jurada/determinacion.asp
- Manual F.1359 (certificado anual/informativo/final): https://www.afip.gob.ar/572web/documentos/F1359-Version-00200-Manual-001.pdf

## Catalogo base (por categorias)
### Remunerativos
- Sueldo basico.
- Antiguedad.
- Adicionales por convenio (zona, titulo, turnos, etc.).
- Horas extras.
- SAC (aguinaldo).
- Vacaciones pagas.

### No remunerativos
- Sumas no remunerativas por acuerdo/convenio.
- Bonos y gratificaciones no remunerativas (segun CCT).

### Descuentos y retenciones obligatorias
- Jubilacion.
- Obra social.
- Ley 19.032.
- Sindicato (aporte y contribucion, segun convenio).
- Impuesto a las Ganancias (4ta categoria) segun RG 4003.

### Embargos y cuota alimentaria
- Embargos judiciales por porcentaje/monto fijo (base neta o bruta, o por concepto).
- Otras causas (monto total y pendiente).

### Asignaciones familiares
- Hijo, hijo con discapacidad, prenatal, nacimiento, matrimonio, ayuda escolar, etc. (segun SUAF y condiciones vigentes).

### Otras incidencias
- Licencias con/sin goce (maternidad, enfermedad, accidente, examenes, etc.).
- Pluriempleo (aportes y deducciones entre empleadores).

## Catalogo completo heredado (referencia)
Para la base completa de conceptos utilizada por el sistema anterior:
- `C:\proyectos\23.01\23.01\Conceptos\Conceptos.xml`
- `C:\proyectos\23.01\23.01\Conceptos\Conceptos_SICOSS.xml`

Estas fuentes contienen los conceptos y reglas usadas en produccion. Se deben portar y versionar en el core moderno.

## Reglas activas en el core moderno
- `data/reglas/conceptos/reglas.json`
