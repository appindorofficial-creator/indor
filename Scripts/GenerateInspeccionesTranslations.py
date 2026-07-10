#!/usr/bin/env python3
"""Generate UiTranslationsInspeccionesEmergency.cs from broken-inspection-keys.txt"""
import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
KEYS_FILE = os.path.join(os.path.dirname(ROOT), "broken-inspection-keys.txt")
OUT_FILE = os.path.join(ROOT, "IndorMvcApp", "Localization", "UiTranslationsInspeccionesEmergency.cs")
MAP_FILE = os.path.join(ROOT, "Scripts", "inspecciones-es-map.json")

def escape_cs(s: str) -> str:
    return s.replace("\\", "\\\\").replace('"', '\\"')

def main():
    with open(KEYS_FILE, encoding="utf-8") as f:
        keys = [line.strip() for line in f if line.strip()]

    if os.path.exists(MAP_FILE):
        with open(MAP_FILE, encoding="utf-8") as f:
            translations = json.load(f)
    else:
        translations = {}

    missing = [k for k in keys if k not in translations]
    if missing:
        print(f"WARNING: {len(missing)} keys missing from map (will use English as fallback)")

    lines = [
        "namespace IndorMvcApp.Localization;",
        "",
        "/// <summary>Overrides broken/missing UiTranslationsFlows entries for inspections and emergency flows.</summary>",
        "public static class UiTranslationsInspeccionesEmergency",
        "{",
        "    public static IEnumerable<KeyValuePair<string, string>> Entries =>",
        "        new Dictionary<string, string>(StringComparer.Ordinal)",
        "        {",
    ]

    for key in keys:
        es = translations.get(key, key)
        lines.append(f'            ["{escape_cs(key)}"] = "{escape_cs(es)}",')

    lines.extend([
        "        };",
        "}",
    ])

    with open(OUT_FILE, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")

    print(f"Wrote {len(keys)} entries to {OUT_FILE}")

if __name__ == "__main__":
    main()
