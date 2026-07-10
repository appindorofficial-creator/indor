import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent

spanish = {}
for f in (ROOT / "Localization").glob("UiTranslations*.cs"):
    if f.name == "UiTranslations.cs":
        continue
    text = f.read_text(encoding="utf-8", errors="replace")
    for m in re.finditer(r'\["([^"]+)"\]\s*=\s*"([^"]*)"', text):
        spanish[m.group(1)] = m.group(2)

l_keys = {}
for f in (ROOT / "Views").rglob("*.cshtml"):
    text = f.read_text(encoding="utf-8", errors="replace")
    for m in re.finditer(r'@L\["([^"]+)"\]', text):
        k = m.group(1)
        l_keys.setdefault(k, []).append(str(f.relative_to(ROOT)).replace("\\", "/"))

missing = sorted(k for k in l_keys if k not in spanish)
same_as_en = sorted(k for k in l_keys if k in spanish and spanish[k] == k)
hybrid_re = re.compile(
    r"\b(the|and|or|for|with|your|this|that|file|report|quote|view|add|new|select|upload|pending|active|shared|client|property|inspection|provider|service|home|need|today|week|emergency|search|filter|create|open|copy|request)\b",
    re.I,
)
broken_hybrid = sorted(
    k for k in l_keys
    if k in spanish and spanish[k] != k and hybrid_re.search(spanish[k])
)

print("=== STATS ===")
print(f"Spanish dict entries: {len(spanish)}")
print(f"Unique L[] keys in views: {len(l_keys)}")
print(f"Missing translations: {len(missing)}")
print(f"Same as English (untranslated): {len(same_as_en)}")
print(f"Possible hybrid/broken: {len(broken_hybrid)}")
print()
print("=== TOP 50 MISSING ===")
for k in missing[:50]:
    print(k)
print()
print("=== TOP 50 UNTRANSLATED (value==key) ===")
for k in same_as_en[:50]:
    print(k)
print()
print("=== TOP 50 HYBRID ===")
for k in broken_hybrid[:50]:
    print(f"{k} => {spanish[k]}")
