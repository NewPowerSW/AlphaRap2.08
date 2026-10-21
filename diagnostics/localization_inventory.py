"""Inventory UI captions without treating recipe values or user input as translations."""
import json
import pathlib
import re
import xml.etree.ElementTree as ET

ROOT = pathlib.Path(__file__).resolve().parents[1]
OUT = ROOT / '.vp-validation' / 'localization'
OUT.mkdir(parents=True, exist_ok=True)


def read_xml(path):
    data = path.read_bytes()
    text = data.decode('utf-16' if data.startswith((b'\xff\xfe', b'\xfe\xff')) else 'utf-8-sig')
    return ET.fromstring(re.sub(r'<\?xml.*?\?>', '', text))


catalog = {}
for folder in ['LanguageData', 'AlphaRap/bin/x64/Debug/LanguageData']:
    for language in ['Chinese', 'English', 'Español']:
        path = ROOT / folder / (language + '.xml')
        if not path.exists():
            continue
        root = read_xml(path)
        for form in root:
            for node in form:
                key = form.tag + '/' + node.tag
                catalog.setdefault(key, {}).setdefault(language, node.get('ComponentText', ''))

designer = []
for path in (ROOT / 'AlphaRap').rglob('*.cs'):
    if any(p in ('bin', 'obj', 'Connected Services') for p in path.parts):
        continue
    text = path.read_text(encoding='utf-8-sig')
    if path.name.lower().endswith('.designer.cs'):
        group = path.name.split('.')[0]
        for match in re.finditer(r'this\.(?:(\w+)\.)?(Text|HeaderText|ToolTipText)\s*=\s*("(?:\\.|[^"\\])*")', text):
            designer.append([group, match[1] or group, match[2], json.loads(match[3])])

(OUT / 'catalog.json').write_text(json.dumps(catalog, ensure_ascii=False, indent=2), encoding='utf-8')
(OUT / 'designer.json').write_text(json.dumps(designer, ensure_ascii=False, indent=2), encoding='utf-8')
values = sorted(set(v for *_, v in designer if v.strip()))
(OUT / 'captions.txt').write_text('\n'.join(values), encoding='utf-8')
print('Existing catalog keys:', len(catalog), 'Designer captions:', len(designer), 'Distinct captions:', len(values))
for language in ['Chinese', 'English', 'Español']:
    print(language, 'missing:', sum(not v.get(language) for v in catalog.values()),
          'CJK:', sum(bool(re.search('[一-鿿]', v.get(language, ''))) for v in catalog.values()))
