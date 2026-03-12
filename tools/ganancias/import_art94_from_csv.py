import argparse
import csv
import json
from pathlib import Path


def parse_decimal(value, field):
    try:
        return float(value)
    except ValueError as exc:
        raise ValueError(f"Invalid {field}: {value}") from exc


def main():
    parser = argparse.ArgumentParser(description="Import Art.94 CSV to JSON")
    parser.add_argument("--csv", required=True, help="Path to CSV file")
    parser.add_argument(
        "--version", required=True, help="Version label (e.g., 2026-S1)"
    )
    parser.add_argument("--effective-from", required=True, help="YYYY-MM-DD")
    parser.add_argument("--effective-to", required=True, help="YYYY-MM-DD")
    parser.add_argument("--source-url", required=True, help="Source PDF URL")
    parser.add_argument("--source-published-at", required=True, help="YYYY-MM-DD")
    parser.add_argument(
        "--output",
        default="data/reglas/ganancias/art94.json",
        help="Output JSON path",
    )

    args = parser.parse_args()

    rows = []
    csv_path = Path(args.csv)
    with csv_path.open("r", newline="", encoding="utf-8") as handle:
        reader = csv.DictReader(handle)
        for idx, row in enumerate(reader, start=2):
            desde = parse_decimal(row.get("desde", ""), f"desde (line {idx})")
            hasta = parse_decimal(row.get("hasta", ""), f"hasta (line {idx})")
            cuota = parse_decimal(row.get("cuota_fija", ""), f"cuota_fija (line {idx})")
            alicuota = parse_decimal(row.get("alicuota", ""), f"alicuota (line {idx})")
            rows.append(
                {
                    "desde": desde,
                    "hasta": hasta,
                    "cuota_fija": cuota,
                    "alicuota": alicuota,
                }
            )

    payload = {
        "version": args.version,
        "effective_from": args.effective_from,
        "effective_to": args.effective_to,
        "source_url": args.source_url,
        "source_published_at": args.source_published_at,
        "rows": rows,
    }

    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(payload, indent=2), encoding="utf-8")


if __name__ == "__main__":
    main()
