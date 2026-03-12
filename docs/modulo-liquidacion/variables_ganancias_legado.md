# Variables de Ganancias - legado 23.01 (referencia)

## Objetivo
Documentar las variables observadas en el legado para IG (Ganancias) y su importacion.

## Deducciones por legajo (DEDUC_IG)
Fuente: `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Legajo\DEDUC_IG.form.xml`
- `c_tipo` (Tipo: Anual o Mensual)
- `e_anio`
- `oi_item_ig` (Item de deduccion)
- `e_periodo_des` (Periodo desde YYYYMM)
- `e_periodo_has` (Periodo hasta YYYYMM)
- `oi_entidad_ig` (Entidad)
- `n_importe` (Importe mensual)

## Importacion de items IG (interface)
Fuente: `C:\proyectos\23.01\23.01\Class\NucleusRH\Base\Liquidacion\lib_v11.ImportarItemsGanancia.cs`
- `c_item_ig` (codigo de item IG)
- `e_anio`
- `e_periodo_desde`
- `e_periodo_hasta`
- `n_importe`
- `e_numero_legajo`
- Derivacion: `oi_item_ig` resuelto desde codigo.

## Rangos IG (escala)
Fuente: `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Interface\RangosIG\flow.xml`
- Interface de importacion de rangos IG por `e_anio`.

## Topes de deducciones IG
Fuente: `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Interface\TopesDeducciones\flow.xml`
- Interface de importacion de topes/deducciones (sin detalle en el form).

## Embargos (cuota alimentaria y otras causas)
Fuente: `C:\proyectos\23.01\23.01\Class\NucleusRH\Base\Liquidacion\lib_v11.EntradaEmbargos.ENTRADAEMBARGOS.cs`
Campos observados:
- `e_nro_oficio`, `f_oficio`, `f_recepcion`
- `c_tipo_embargo` (CA cuota alimentaria, OC otras causas)
- `l_liquida` (automatico)
- `o_caratula`, `d_juzgado`, `d_juez`, `d_secretaria`
- `c_nro_expediente`, `c_nro_cuenta`
- `f_inicio`, `f_inactivo`, `f_finalizado`
- `c_estado` (I/A/N/F)
- `n_porc_emb`, `l_sueldo_bruto`, `l_sueldo_neto`
- `n_monto_fijo`, `n_monto_total`, `n_monto_pend`
- `oi_concepto` (opcional)
- `d_actor`, `d_causa`, `oi_sucursal`
Derivados observados:
- `n_monto_pend = n_monto_total - suma` (form validacion en `Legajo.Embargos.form.xml`).

## Reportes y formularios IG
- F.1359 (IG 4ta): `C:\proyectos\23.01\23.01\Form\NucleusRH\Base\Liquidacion\Interface\LiqIG4TA\form.xml`
- Definicion de interfaces: `C:\proyectos\23.01\23.01\InterfacesOut\Definitions\liqig4ta.def.xml`
