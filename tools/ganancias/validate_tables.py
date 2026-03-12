import argparse
import csv


def validate_art94(path):
    with open(path, newline="", encoding="utf-8") as handle:
        rows = list(csv.DictReader(handle))
    if len(rows) != 8:
        raise SystemExit(f"Art.94 debe tener 8 tramos, tiene {len(rows)}")
    required_rates = {"0.05", "0.09", "0.12", "0.15", "0.19", "0.23", "0.27", "0.31"}
    rates = {r["alicuota"] for r in rows}
    if rates != required_rates:
        raise SystemExit(f"Art.94 alícuotas inválidas: {sorted(rates)}")


def validate_art30(path):
    with open(path, newline="", encoding="utf-8") as handle:
        rows = list(csv.DictReader(handle))
    codes = {r["codigo"] for r in rows}
    required = {
        "GAN_NO_IMPONIBLE",
        "CONYUGE",
        "HIJO",
        "HIJO_DISCAPACIDAD",
        "DED_ESPECIAL",
        "DED_ESPECIAL_AP2",
        "DED_ESPECIAL_NUEVOS",
    }
    missing = required - codes
    if missing:
        raise SystemExit(f"Art.30 faltan códigos: {sorted(missing)}")


def main():
    parser = argparse.ArgumentParser(description="Valida tablas de Ganancias")
    parser.add_argument("--art94", required=True)
    parser.add_argument("--art30", required=True)
    args = parser.parse_args()

    validate_art94(args.art94)
    validate_art30(args.art30)
    print("Tablas OK")


if __name__ == "__main__":
    main()
