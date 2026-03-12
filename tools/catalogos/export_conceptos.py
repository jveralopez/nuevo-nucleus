import argparse
import json
from pathlib import Path
import xml.etree.ElementTree as ET


def parse_bool(value):
    if value is None:
        return None
    if value in {"1", "true", "True"}:
        return True
    if value in {"0", "false", "False"}:
        return False
    return value


def extract_concepts(xml_path, source_label):
    tree = ET.parse(xml_path)
    root = tree.getroot()
    concepts = []
    for node in root.iter():
        if node.tag != "CON":
            continue
        attrs = node.attrib
        concepts.append(
            {
                "codigo": attrs.get("c_concepto"),
                "descripcion": attrs.get("d_concepto"),
                "grupo_tliq": attrs.get("c_grupo_tliq"),
                "tipo_concepto": attrs.get("c_tipo_concepto"),
                "acum_ganancia": parse_bool(attrs.get("ca_acum_ganancia")),
                "figura_recibo": parse_bool(attrs.get("ca_fig_recibo")),
                "tipo_asiento": attrs.get("ca_tipo_asiento"),
                "secuencia": attrs.get("e_secuencia"),
                "activo": parse_bool(attrs.get("l_activo")),
                "ejecuta_ajuste": parse_bool(attrs.get("l_ej_ajuste")),
                "retroactivo": parse_bool(attrs.get("l_retroactivo")),
                "source": source_label,
            }
        )
    return concepts


def main():
    parser = argparse.ArgumentParser(description="Exporta conceptos desde XML legado")
    parser.add_argument("--xml", required=True, help="Ruta al XML de conceptos")
    parser.add_argument(
        "--source", required=True, help="Etiqueta de origen (ej. 23.01)"
    )
    parser.add_argument("--output", required=True, help="Ruta de salida JSON")
    args = parser.parse_args()

    xml_path = Path(args.xml)
    concepts = extract_concepts(xml_path, args.source)
    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps({"items": concepts}, indent=2), encoding="utf-8")
    print(f"Exportados {len(concepts)} conceptos a {output_path}")


if __name__ == "__main__":
    main()
