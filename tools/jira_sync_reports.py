#!/usr/bin/env python3
"""
Script para automatizar la creacion de tareas de testing en Jira.

Uso:
    python tools/jira_sync_reports.py                    # Sync al sprint activo (Sprint 4)
    python tools/jira_sync_reports.py --sprint KAN-41    # Sync a un sprint especifico
    python tools/jira_sync_reports.py --dry-run          # Solo mostrar que crearia

Requiere:
    - JIRA_EMAIL y JIRA_API_TOKEN como env vars o en .env
    - Proyecto configurado (default: KAN)
"""

import os
import sys
import json
import argparse
import pathlib
from datetime import datetime

try:
    import urllib.request
    import urllib.error
    import base64
except ImportError:
    print("Error: urllib no disponible")
    sys.exit(1)

# Configuracion
JIRA_BASE_URL = os.environ.get("JIRA_URL", "https://jonatanmveralopez.atlassian.net")
JIRA_PROJECT = os.environ.get("JIRA_PROJECT", "KAN")
REPORTS_DIR = pathlib.Path(__file__).parent.parent / "docs" / "reportes"

# Credenciales
JIRA_EMAIL = os.environ.get("JIRA_EMAIL")
JIRA_TOKEN = os.environ.get("JIRA_TOKEN")

# Si no hay env vars, intentar leer de .env
if not JIRA_EMAIL or not JIRA_TOKEN:
    env_file = pathlib.Path(__file__).parent.parent / ".env"
    if env_file.exists():
        for line in env_file.read_text(encoding="utf-8").splitlines():
            if line.startswith("JIRA_EMAIL="):
                JIRA_EMAIL = line.split("=", 1)[1].strip().strip('"').strip("'")
            elif line.startswith("JIRA_TOKEN="):
                JIRA_TOKEN = line.split("=", 1)[1].strip().strip('"').strip("'")


def get_auth_headers():
    if not JIRA_EMAIL or not JIRA_TOKEN:
        print("ERROR: Faltan JIRA_EMAIL y JIRA_TOKEN")
        print("Configuralas como env vars o en .env")
        sys.exit(1)
    creds = f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode("utf-8")
    return {
        "Authorization": "Basic " + base64.b64encode(creds).decode("utf-8"),
        "Accept": "application/json",
        "Content-Type": "application/json",
    }


def jql_search(query, max_results=50):
    """Ejecuta una busqueda JQL"""
    headers = get_auth_headers()
    body = json.dumps(
        {"jql": query, "maxResults": max_results, "fields": ["summary"]}
    ).encode("utf-8")
    req = urllib.request.Request(
        f"{JIRA_BASE_URL}/rest/api/3/search/jql",
        data=body,
        headers=headers,
        method="POST",
    )
    with urllib.request.urlopen(req) as resp:
        return json.load(resp).get("issues", [])


def get_issue_key(issue_id):
    """Obtiene el key de un issue por ID"""
    headers = get_auth_headers()
    req = urllib.request.Request(
        f"{JIRA_BASE_URL}/rest/api/3/issue/{issue_id}?fields=summary", headers=headers
    )
    with urllib.request.urlopen(req) as resp:
        return json.load(resp).get("key")


def search_issue_by_summary(summary):
    """Busca un issue por summary"""
    issues = jql_search(
        f'project = {JIRA_PROJECT} AND summary ~ "{summary}"', max_results=1
    )
    return issues[0].get("id") if issues else None


def create_issue(summary, issue_type, description, parent_key=None):
    """Crea un issue en Jira"""
    headers = get_auth_headers()
    fields = {
        "project": {"key": JIRA_PROJECT},
        "summary": summary,
        "issuetype": {"name": issue_type},
        "description": {
            "type": "doc",
            "version": 1,
            "content": [
                {
                    "type": "paragraph",
                    "content": [{"type": "text", "text": description}],
                }
            ],
        },
    }
    if parent_key:
        fields["parent"] = {"key": parent_key}

    payload = json.dumps({"fields": fields}).encode("utf-8")
    req = urllib.request.Request(
        f"{JIRA_BASE_URL}/rest/api/3/issue",
        data=payload,
        headers=headers,
        method="POST",
    )
    try:
        with urllib.request.urlopen(req) as resp:
            data = json.load(resp)
            return data.get("key"), data.get("id")
    except urllib.error.HTTPError as e:
        error_body = e.read().decode("utf-8")
        print(f"Error creando {summary}: {e.code}")
        print(error_body)
        return None, None


def set_parent(issue_key, parent_key):
    """Asigna un parent a un issue"""
    headers = get_auth_headers()
    payload = json.dumps({"fields": {"parent": {"key": parent_key}}}).encode("utf-8")
    req = urllib.request.Request(
        f"{JIRA_BASE_URL}/rest/api/3/issue/{issue_key}",
        data=payload,
        headers=headers,
        method="PUT",
    )
    with urllib.request.urlopen(req) as resp:
        return resp.status


def get_sprint_tasks(sprint_key):
    """Obtiene las tareas de un sprint"""
    issues = jql_search(f"project = {JIRA_PROJECT} AND parent = {sprint_key}")
    return {i.get("fields", {}).get("summary"): i.get("key") for i in issues}


def sync_reports(sprint_key=None, dry_run=False):
    """Sincroniza los reportes de docs/reportes a Jira"""

    # Si no hay sprint, buscar el activo
    if not sprint_key:
        # Buscar Sprint 4 como activo por defecto
        sprint_key = "KAN-42"  # Sprint 4
        print(f"Usando sprint activo por defecto: {sprint_key}")

    print(f"\n=== Sincronizando reportes a {sprint_key} ===")
    print(f"Directorio: {REPORTS_DIR}")
    print(f"Dry run: {dry_run}\n")

    # Obtener tareas existentes del sprint
    existing_tasks = get_sprint_tasks(sprint_key)
    print(f"Tareas existentes en sprint: {len(existing_tasks)}")

    # Buscar reportes
    if not REPORTS_DIR.exists():
        print(f"ERROR: No existe {REPORTS_DIR}")
        return

    report_files = sorted(REPORTS_DIR.glob("*.md"))
    print(f"Reportes encontrados: {len(report_files)}\n")

    created = []
    skipped = []

    for report_path in report_files:
        summary = f"Test: {report_path.stem.replace('_', ' ')}"

        # Verificar si ya existe
        if summary in existing_tasks:
            skipped.append((summary, existing_tasks[summary]))
            continue

        # Leer contenido
        content = report_path.read_text(encoding="utf-8").strip()

        # Crear descripcion
        description = f"Reporte de testing\nFuente: docs/reportes/{report_path.name}\n\n{content[:3000]}"

        if dry_run:
            print(f"[DRY-RUN] Crearia: {summary}")
            continue

        # Crear tarea
        key, issue_id = create_issue(summary, "Tarea", description, sprint_key)

        if key:
            print(f"Creado: {key} - {summary}")
            created.append((summary, key))
        else:
            print(f"ERROR: No se pudo crear {summary}")

    print(f"\n=== Resumen ===")
    print(f"Creados: {len(created)}")
    print(f"Skippeados (ya existen): {len(skipped)}")

    if created:
        print("\nLinks creados:")
        for _, key in created:
            print(f"  {JIRA_BASE_URL}/browse/{key}")

    return created, skipped


def main():
    parser = argparse.ArgumentParser(description="Sincroniza reportes a Jira")
    parser.add_argument("--sprint", help="Key del sprint (ej: KAN-41)")
    parser.add_argument(
        "--dry-run", action="store_true", help="Solo mostrar lo que crearia"
    )
    args = parser.parse_args()

    sync_reports(sprint_key=args.sprint, dry_run=args.dry_run)


if __name__ == "__main__":
    main()
