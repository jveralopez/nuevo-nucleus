from __future__ import annotations

import json
import os
from dataclasses import dataclass
from datetime import datetime, timezone
from decimal import Decimal, ROUND_HALF_UP
from typing import Any
from uuid import uuid4


ROOT_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DATA_DIR = os.path.join(ROOT_DIR, "data")
ARTIFACTS_DIR = os.path.join(ROOT_DIR, "artifacts")


@dataclass
class Legajo:
    numero: str
    nombre: str
    cuil: str
    convenio: str | None
    categoria: str | None
    basico: Decimal
    antiguedad: Decimal
    adicionales: Decimal
    presentismo: Decimal
    horas_extra: Decimal
    premios: Decimal
    descuentos: Decimal
    no_remunerativo: Decimal
    bonos_no_remunerativos: Decimal
    aplica_ganancias: bool
    omitir_ganancias: bool
    conyuge_a_cargo: bool
    cant_hijos: int
    cant_otros_familiares: int
    deducciones_adicionales: Decimal
    vacaciones_dias: int
    licencias: list[dict[str, Any]]
    embargos: list[dict[str, Any]]


def round_2(value: Decimal) -> Decimal:
    return value.quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)


def load_json(path: str) -> dict[str, Any]:
    if not os.path.exists(path):
        return {}
    with open(path, "r", encoding="utf-8") as fh:
        return json.load(fh)


def normalize_convenio(convenio: str) -> str:
    return convenio.strip().lower().replace(" ", "-").replace("/", "-")


def load_rules(convenio: str | None) -> list[dict[str, Any]]:
    base_path = os.path.join(DATA_DIR, "reglas", "conceptos", "reglas.json")
    base_rules = load_json(base_path).get("rules", [])
    if not convenio:
        return base_rules

    convenio_file = os.path.join(
        DATA_DIR,
        "reglas",
        "conceptos",
        "convenios",
        f"{normalize_convenio(convenio)}.json",
    )
    convenio_rules = load_json(convenio_file).get("rules", [])
    if not convenio_rules:
        return base_rules

    convenio_fields = {
        rule.get("legajoField", "").lower()
        for rule in convenio_rules
        if rule.get("formula") == "LegajoField" and rule.get("legajoField")
    }
    merged = [
        rule
        for rule in base_rules
        if rule.get("formula") != "LegajoField"
        or rule.get("legajoField", "").lower() not in convenio_fields
    ]
    merged.extend(convenio_rules)
    return merged


def get_legajo_field(legajo: Legajo, field: str | None) -> Decimal:
    if not field:
        return Decimal("0")
    key = field.lower()
    mapping = {
        "basico": legajo.basico,
        "antiguedad": legajo.antiguedad,
        "adicionales": legajo.adicionales,
        "presentismo": legajo.presentismo,
        "horasextra": legajo.horas_extra,
        "premios": legajo.premios,
    }
    return mapping.get(key, Decimal("0"))


def evaluate_rule(
    rule: dict[str, Any], legajo: Legajo, remunerativo_actual: Decimal
) -> Decimal:
    formula = rule.get("formula")
    if formula == "LegajoField":
        return get_legajo_field(legajo, rule.get("legajoField"))
    if formula == "PercentOf":
        base_name = str(rule.get("base") or "").lower()
        if base_name == "remunerativo":
            base_value = remunerativo_actual
        elif base_name == "basico":
            base_value = legajo.basico
        else:
            base_value = remunerativo_actual
        rate = Decimal(str(rule.get("rate") or 0))
        return round_2(base_value * rate)
    if formula == "Fixed":
        return Decimal(str(rule.get("amount") or 0))
    return Decimal("0")


def load_art30() -> list[dict[str, Any]]:
    path = os.path.join(DATA_DIR, "reglas", "ganancias", "art30.json")
    return load_json(path).get("rows", [])


def load_art94() -> list[dict[str, Any]]:
    path = os.path.join(DATA_DIR, "reglas", "ganancias", "art94.json")
    return load_json(path).get("rows", [])


def get_art30_importe(rows: list[dict[str, Any]], codigo: str) -> Decimal:
    for row in rows:
        if str(row.get("codigo", "")).upper() == codigo.upper():
            return Decimal(str(row.get("importe") or 0))
    return Decimal("0")


def calcular_ganancias(
    remunerativo: Decimal, deducciones_afectan: Decimal, legajo: Legajo
) -> Decimal:
    if not legajo.aplica_ganancias or legajo.omitir_ganancias:
        return Decimal("0")

    art30 = load_art30()
    if not art30:
        return Decimal("0")

    total_deducciones = Decimal("0")
    total_deducciones += get_art30_importe(art30, "GAN_NO_IMPONIBLE")
    total_deducciones += get_art30_importe(art30, "DED_ESPECIAL")
    if legajo.conyuge_a_cargo:
        total_deducciones += get_art30_importe(art30, "CONYUGE")
    if legajo.cant_hijos > 0:
        total_deducciones += get_art30_importe(art30, "HIJO") * legajo.cant_hijos
    if legajo.cant_otros_familiares > 0:
        total_deducciones += (
            get_art30_importe(art30, "OTRO_FAMILIAR") * legajo.cant_otros_familiares
        )
    total_deducciones += legajo.deducciones_adicionales
    total_deducciones = round_2(total_deducciones)

    base_imponible = remunerativo - deducciones_afectan - total_deducciones
    if base_imponible <= 0:
        return Decimal("0")

    art94 = load_art94()
    if not art94:
        return Decimal("0")

    base = base_imponible
    tramo = None
    for row in art94:
        desde = Decimal(str(row.get("desde") or 0))
        hasta = Decimal(str(row.get("hasta") or 0))
        if base >= desde and base <= hasta:
            tramo = row
            break
    if tramo is None:
        tramo = max(art94, key=lambda r: Decimal(str(r.get("desde") or 0)))

    desde = Decimal(str(tramo.get("desde") or 0))
    cuota = Decimal(str(tramo.get("cuotaFija") or 0))
    alicuota = Decimal(str(tramo.get("alicuota") or 0))
    impuesto = cuota + (base - desde) * alicuota
    return round_2(impuesto) if impuesto > 0 else Decimal("0")


def calcular_embargos(
    embargos: list[dict[str, Any]], remunerativo: Decimal, neto: Decimal
) -> Decimal:
    total = Decimal("0")
    for embargo in embargos:
        if not embargo.get("activo", True):
            continue
        base = neto
        if str(embargo.get("baseCalculo", "Neto")).lower() == "bruto":
            base = remunerativo
        monto = Decimal("0")
        porcentaje = embargo.get("porcentaje")
        if porcentaje is not None and Decimal(str(porcentaje)) > 0:
            monto = base * Decimal(str(porcentaje))
        elif embargo.get("montoFijo") is not None:
            monto = Decimal(str(embargo.get("montoFijo")))

        if embargo.get("montoPendiente") is not None:
            monto = min(monto, Decimal(str(embargo.get("montoPendiente"))))
        elif embargo.get("montoTotal") is not None:
            monto = min(monto, Decimal(str(embargo.get("montoTotal"))))

        total += max(Decimal("0"), round_2(monto))
    return total


def build_recibo(legajo: Legajo) -> dict[str, Any]:
    rules = [rule for rule in load_rules(legajo.convenio) if rule.get("activo")]
    detalle: list[dict[str, Any]] = []

    remunerativo = Decimal("0")
    no_remunerativo = legajo.no_remunerativo + legajo.bonos_no_remunerativos
    deducciones = Decimal("0")
    deducciones_afectan = Decimal("0")

    for rule in [r for r in rules if r.get("tipo") != "Deduccion"]:
        importe = evaluate_rule(rule, legajo, remunerativo)
        if importe == 0:
            continue
        if rule.get("tipo") == "Remunerativo":
            remunerativo += importe
            detalle.append(
                {"concepto": rule.get("descripcion", ""), "importe": float(importe)}
            )
        elif rule.get("tipo") == "NoRemunerativo":
            no_remunerativo += importe
            detalle.append(
                {"concepto": rule.get("descripcion", ""), "importe": float(importe)}
            )

    for rule in [r for r in rules if r.get("tipo") == "Deduccion"]:
        importe = evaluate_rule(rule, legajo, remunerativo)
        if importe == 0:
            continue
        deducciones += importe
        if rule.get("afectaGanancias"):
            deducciones_afectan += importe
        detalle.append(
            {"concepto": rule.get("descripcion", ""), "importe": float(-importe)}
        )

    if no_remunerativo > 0:
        detalle.append(
            {"concepto": "No remunerativo", "importe": float(no_remunerativo)}
        )

    if legajo.vacaciones_dias > 0:
        vacaciones = round_2(
            (legajo.basico / Decimal("30")) * Decimal(legajo.vacaciones_dias)
        )
        remunerativo += vacaciones
        detalle.append({"concepto": "Vacaciones", "importe": float(vacaciones)})

    dias_sin_goce = sum(
        l.get("dias", 0) for l in legajo.licencias if not l.get("conGoce", True)
    )
    if dias_sin_goce > 0:
        descuento = round_2((legajo.basico / Decimal("30")) * Decimal(dias_sin_goce))
        deducciones += descuento
        detalle.append({"concepto": "Licencia sin goce", "importe": float(-descuento)})

    if legajo.descuentos > 0:
        deducciones += legajo.descuentos
        detalle.append(
            {"concepto": "Descuentos manuales", "importe": float(-legajo.descuentos)}
        )

    impuesto = calcular_ganancias(remunerativo, deducciones_afectan, legajo)
    if impuesto > 0:
        deducciones += impuesto
        detalle.append(
            {"concepto": "Impuesto a las ganancias", "importe": float(-impuesto)}
        )

    neto_pre_embargos = max(Decimal("0"), remunerativo - deducciones + no_remunerativo)
    embargos_total = calcular_embargos(legajo.embargos, remunerativo, neto_pre_embargos)
    if embargos_total > 0:
        deducciones += embargos_total
        detalle.append({"concepto": "Embargos", "importe": float(-embargos_total)})

    neto = max(Decimal("0"), remunerativo - deducciones + no_remunerativo)

    return {
        "id": str(uuid4()),
        "payrollRunId": str(uuid4()),
        "legajoId": str(uuid4()),
        "legajoNumero": legajo.numero,
        "legajoNombre": legajo.nombre,
        "remunerativo": float(round_2(remunerativo)),
        "deducciones": float(round_2(deducciones)),
        "neto": float(round_2(neto)),
        "generadoEn": datetime.now(timezone.utc).isoformat(),
        "detalle": detalle,
    }


def main() -> None:
    legajo = Legajo(
        numero="900",
        nombre="Lucia Fernandez",
        cuil="27-12345678-9",
        convenio="CCT 130/75",
        categoria="Administrativo",
        basico=Decimal("50000000"),
        antiguedad=Decimal("2000000"),
        adicionales=Decimal("1000000"),
        presentismo=Decimal("300000"),
        horas_extra=Decimal("400000"),
        premios=Decimal("300000"),
        descuentos=Decimal("50000"),
        no_remunerativo=Decimal("200000"),
        bonos_no_remunerativos=Decimal("120000"),
        aplica_ganancias=True,
        omitir_ganancias=False,
        conyuge_a_cargo=True,
        cant_hijos=2,
        cant_otros_familiares=0,
        deducciones_adicionales=Decimal("100000"),
        vacaciones_dias=5,
        licencias=[{"tipo": "Sin goce", "dias": 2, "conGoce": False}],
        embargos=[
            {
                "tipo": "Alimentos",
                "porcentaje": 0.05,
                "baseCalculo": "Neto",
                "activo": True,
            }
        ],
    )

    recibo = build_recibo(legajo)
    os.makedirs(ARTIFACTS_DIR, exist_ok=True)
    output_path = os.path.join(ARTIFACTS_DIR, "recibo-demo.json")
    with open(output_path, "w", encoding="utf-8") as fh:
        json.dump(recibo, fh, ensure_ascii=False, indent=2)
    print(f"Recibo demo generado en {output_path}")


if __name__ == "__main__":
    main()
