import argparse
import json
import re
from pathlib import Path


PATTERNS = [
    {
        "pattern": r"\bb[áa]sico\b",
        "tipo": "Remunerativo",
        "formula": "LegajoField",
        "legajoField": "basico",
        "afectaGanancias": True,
    },
    {
        "pattern": r"\bsueldo\b",
        "tipo": "Remunerativo",
        "formula": "LegajoField",
        "legajoField": "basico",
        "afectaGanancias": True,
    },
    {
        "pattern": r"\bantig[üu]edad\b",
        "tipo": "Remunerativo",
        "formula": "LegajoField",
        "legajoField": "antiguedad",
        "afectaGanancias": True,
    },
    {
        "pattern": r"\badicional(es)?\b",
        "tipo": "Remunerativo",
        "formula": "LegajoField",
        "legajoField": "adicionales",
        "afectaGanancias": True,
    },
]


def apply_patterns(rules):
    updated = 0
    for rule in rules:
        desc = (rule.get("descripcion") or "").lower()
        for entry in PATTERNS:
            if re.search(entry["pattern"], desc):
                rule["tipo"] = entry["tipo"]
                rule["formula"] = entry["formula"]
                rule["legajoField"] = entry["legajoField"]
                rule["afectaGanancias"] = entry["afectaGanancias"]
                rule["activo"] = True
                updated += 1
                break
    return updated


def main():
    parser = argparse.ArgumentParser(description="Activa reglas por descripción")
    parser.add_argument("--input", required=True, help="JSON de reglas")
    parser.add_argument("--output", required=True, help="Salida JSON")
    args = parser.parse_args()

    input_path = Path(args.input)
    data = json.loads(input_path.read_text(encoding="utf-8"))
    rules = data.get("rules", [])
    updated = apply_patterns(rules)
    data["rules"] = rules

    output_path = Path(args.output)
    output_path.write_text(json.dumps(data, indent=2), encoding="utf-8")
    print(f"Reglas activadas: {updated}")


if __name__ == "__main__":
    main()
