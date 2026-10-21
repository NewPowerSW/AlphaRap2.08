"""Validate UI language packs and report unresolved source alarm definitions."""
import collections
import pathlib
import re
import sys
import xml.etree.ElementTree as ET

ROOT = pathlib.Path(__file__).resolve().parents[1]
LANGUAGES = ('Chinese', 'English', 'Español')


def read_xml(path):
    data = path.read_bytes()
    text = data.decode('utf-16' if data.startswith((b'\xff\xfe', b'\xfe\xff')) else 'utf-8-sig')
    return ET.fromstring(re.sub(r'<\?xml.*?\?>', '', text))


def check_ui():
    catalogs = {}
    errors = []
    for language in LANGUAGES:
        root = read_xml(ROOT / 'LanguageData' / (language + '.xml'))
        if root.tag != language:
            errors.append(f'{language}: wrong XML root {root.tag}')
        entries = [(form.tag + '/' + node.tag, node.get('ComponentText', '')) for form in root for node in form]
        if len(entries) != len(dict(entries)):
            errors.append(f'{language}: duplicate caption keys')
        catalogs[language] = dict(entries)
        for key, value in entries:
            if not value.strip():
                errors.append(f'{language}/{key}: empty translation')
            if language != 'Chinese' and re.search('[一-鿿]', value):
                errors.append(f'{language}/{key}: untranslated Chinese text')
            if r'\r\n' in value:
                errors.append(f'{language}/{key}: literal escaped newline')
    keys = set(catalogs['Chinese'])
    for language, catalog in catalogs.items():
        if set(catalog) != keys:
            errors.append(f'{language}: language key sets differ')
    for key in keys:
        placeholders = [collections.Counter(re.findall(r'(?<!\{)\{(\d+)(?:[^{}]*)\}', catalogs[l].get(key, ''))) for l in LANGUAGES]
        if placeholders[0] != placeholders[1] or placeholders[0] != placeholders[2]:
            errors.append(f'{key}: format placeholders differ between languages')
    print(f'UI: {len(keys)} keys × 3 languages; {len(errors)} errors')
    for error in errors:
        print('ERROR', error)
    return errors


def inspect_alarms():
    roots = {language: read_xml(ROOT / 'AlarmTable' / (language + '.xml')) for language in ('Chinese', 'English', 'Spanish')}
    primary = roots['Chinese']
    counts = collections.Counter(node.tag for node in primary)
    duplicates = sorted(key for key, count in counts.items() if count > 1)
    references = set()
    for path in (ROOT / 'AlphaRap').rglob('*.cs'):
        if any(part in ('bin', 'obj') for part in path.parts):
            continue
        references.update('A' + code for code in re.findall(r'Alarm\.Show\("(\d+)"', path.read_text(encoding='utf-8-sig')))
    missing = sorted(references - set(counts))
    print('Alarm definitions requiring review: duplicate IDs =', ', '.join(duplicates) or 'none',
          '; referenced but undefined =', ', '.join(missing) or 'none')
    for language in ('English', 'Spanish'):
        translated = {node.tag: node for node in roots[language]}
        for node in primary:
            other = translated.get(node.tag)
            if other is None:
                print('ALARM REVIEW', language, node.tag, 'missing translation')
            elif any(node.get(name) != other.get(name) for name in ('Type', 'DoStop')):
                print('ALARM REVIEW', language, node.tag, 'metadata differs; runtime uses Chinese primary metadata')


if __name__ == '__main__':
    failures = check_ui()
    inspect_alarms()
    sys.exit(bool(failures))
