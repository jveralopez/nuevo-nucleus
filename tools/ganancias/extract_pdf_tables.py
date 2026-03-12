import argparse
import csv
import re
import urllib.request
from pathlib import Path

import pdfplumber


def download(url, output_path):
    output_path.parent.mkdir(parents=True, exist_ok=True)
    urllib.request.urlretrieve(url, output_path)


def normalize_number(value):
    if value is None:
        return None
    cleaned = value.strip()
    if re.search(r"[A-Za-z]", cleaned):
        return None
    cleaned = cleaned.replace(".", "").replace(",", ".")
    cleaned = re.sub(r"[^0-9.-]", "", cleaned)
    if cleaned == "":
        return None
    return float(cleaned)


def extract_art94(pdf_path, output_csv):
    rows = []
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            table = page.extract_table()
            if table:
                for raw in table:
                    nums = [
                        normalize_number(cell)
                        for cell in raw
                        if cell and re.search(r"\d", cell)
                    ]
                    if len(nums) >= 4:
                        desde, hasta, cuota, alicuota = (
                            nums[0],
                            nums[1],
                            nums[2],
                            nums[3],
                        )
                        if (
                            desde is not None
                            and hasta is not None
                            and alicuota is not None
                        ):
                            rows.append(
                                [
                                    desde,
                                    hasta,
                                    cuota or 0,
                                    percent_to_decimal(alicuota),
                                ]
                            )
                continue

            text = page.extract_text() or ""
            for line in text.splitlines():
                nums = [
                    normalize_number(n)
                    for n in re.findall(r"[0-9\.]+,[0-9]+|[0-9\.]+", line)
                ]
                if len(nums) >= 4:
                    desde, hasta, cuota, alicuota = nums[0], nums[1], nums[2], nums[3]
                    if desde is not None and hasta is not None and alicuota is not None:
                        rows.append(
                            [
                                desde,
                                hasta,
                                cuota or 0,
                                percent_to_decimal(alicuota),
                            ]
                        )

    rows = [r for r in rows if r[0] is not None and r[1] is not None]
    rows = [r for r in rows if 0 <= r[3] <= 1 and r[1] > r[0]]
    rows = sorted({(r[0], r[1], r[2], r[3]) for r in rows})
    valid_rates = {0.05, 0.09, 0.12, 0.15, 0.19, 0.23, 0.27, 0.31}
    by_rate = {}
    for row in rows:
        rate = round(row[3], 2)
        if rate not in valid_rates:
            continue
        existing = by_rate.get(rate)
        if existing is None or row[1] > existing[1]:
            by_rate[rate] = row
    rows = sorted(by_rate.values(), key=lambda r: r[0])
    with open(output_csv, "w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle)
        writer.writerow(["desde", "hasta", "cuota_fija", "alicuota"])
        writer.writerows(rows)


def map_art30_code(description):
    desc = description.lower()
    if "ganancias no imponibles" in desc or "ganancia no imponible" in desc:
        return "GAN_NO_IMPONIBLE"
    if "conyuge" in desc or "cónyuge" in desc:
        return "CONYUGE"
    if "incapacitado" in desc and "hijo" in desc:
        return "HIJO_DISCAPACIDAD"
    if "hijo" in desc:
        return "HIJO"
    if "deduccion especial" in desc or "deducción especial" in desc:
        if "apartado 2" in desc:
            return "DED_ESPECIAL_AP2"
        if "nuevos" in desc:
            return "DED_ESPECIAL_NUEVOS"
        return "DED_ESPECIAL"
    return None


def percent_to_decimal(value):
    if value is None:
        return 0.0
    return (value / 100.0) if value > 1 else value


def extract_art30(pdf_path, output_csv):
    rows = []
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            table = page.extract_table()
            if table:
                for raw in table:
                    if not raw or len(raw) < 2:
                        continue
                    desc = (raw[0] or "").strip()
                    amount = normalize_number(raw[-1] or "")
                    code = map_art30_code(desc)
                    if code and amount is not None:
                        rows.append([code, desc, amount, "ARS"])
                continue

            text = page.extract_text() or ""
            for line in text.splitlines():
                parts = re.split(r"\s{2,}", line.strip())
                if len(parts) < 2:
                    continue
                desc = parts[0]
                amount = normalize_number(parts[-1])
                code = map_art30_code(desc)
                if code and amount is not None:
                    rows.append([code, desc, amount, "ARS"])

    deduped = {}
    for row in rows:
        key = row[0]
        if key not in deduped or row[2] > deduped[key][2]:
            deduped[key] = row
    rows = list(deduped.values())
    with open(output_csv, "w", newline="", encoding="utf-8") as handle:
        writer = csv.writer(handle)
        writer.writerow(["codigo", "descripcion", "importe", "unidad"])
        writer.writerows(rows)


def main():
    parser = argparse.ArgumentParser(description="Extrae tablas de Ganancias desde PDF")
    parser.add_argument("--tipo", required=True, choices=["art94", "art30"])
    parser.add_argument("--url", help="URL del PDF oficial")
    parser.add_argument("--pdf", help="Ruta local del PDF")
    parser.add_argument("--output", required=True, help="CSV de salida")
    args = parser.parse_args()

    pdf_path = args.pdf
    if args.url:
        pdf_path = str(Path("data/ganancias/sources") / Path(args.url).name)
        download(args.url, Path(pdf_path))

    if not pdf_path:
        raise SystemExit("Debe indicar --url o --pdf")

    if args.tipo == "art94":
        extract_art94(pdf_path, args.output)
    else:
        extract_art30(pdf_path, args.output)


if __name__ == "__main__":
    main()
