import argparse
import json
from pathlib import Path


def build_rule(item, default_tipo, inactive):
    return {
        "codigo": item.get("codigo"),
        "descripcion": item.get("descripcion"),
        "tipo": default_tipo,
        "formula": "Fixed",
        "amount": 0,
        "activo": not inactive,
        "afectaGanancias": False,
        "source": item.get("source") or "legacy",
    }


def main():
    parser = argparse.ArgumentParser(description="Genera reglas base desde un catálogo")
    parser.add_argument("--catalogo", required=True, help="JSON de catálogo exportado")
    parser.add_argument("--output", required=True, help="Ruta de salida JSON")
    parser.add_argument(
        "--version", required=True, help="Version label (e.g., 2026-S1)"
    )
    parser.add_argument("--effective-from", required=True, help="YYYY-MM-DD")
    parser.add_argument("--effective-to", required=True, help="YYYY-MM-DD")
    parser.add_argument(
        "--default-tipo", default="Remunerativo", help="Tipo por defecto"
    )
    parser.add_argument(
        "--inactive", action="store_true", help="Generar reglas inactivas"
    )
    args = parser.parse_args()

    catalog_path = Path(args.catalogo)
    data = json.loads(catalog_path.read_text(encoding="utf-8"))
    items = data.get("items", [])

    rules = [build_rule(item, args.default_tipo, args.inactive) for item in items]
    payload = {
        "version": args.version,
        "effective_from": args.effective_from,
        "effective_to": args.effective_to,
        "rules": rules,
    }

    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    print(f"Reglas generadas: {len(rules)}")


if __name__ == "__main__":
    main()
