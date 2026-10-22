"""静态盘点应用工程资源声明；不加载程序集、不实例化对象、不访问设备。"""

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path
import xml.etree.ElementTree as ET


def evidence(path, source, offset):
    """返回仓库相对路径及声明的一基行号。"""
    return {"file": path, "line": source.count("\n", 0, offset) + 1}


def compiled_sources(root):
    """读取 csproj 中显式列出的源码；不执行 MSBuild 条件或导入。"""
    project = root / "AlphaRap/AlphaRap.csproj"
    ns = {"ms": "http://schemas.microsoft.com/developer/msbuild/2003"}
    includes = ET.parse(project).findall(".//ms:Compile", ns)
    paths = sorted({("AlphaRap/" + item.attrib["Include"].replace("\\", "/"))
                    for item in includes})
    missing = [path for path in paths if not (root / path).is_file()]
    if missing:
        raise ValueError("缺少显式编译源文件：" + ", ".join(missing))
    return [(path, (root / path).read_text(encoding="utf-8-sig")) for path in paths]


def properties(source, variable):
    """抽取设计器内简单赋值表达式；保留表达式，不执行 C#。"""
    pattern = rf"\bthis\.{re.escape(variable)}\.(\w+)\s*=\s*([^;]+);"
    return {match[1]: match[2].strip() for match in re.finditer(pattern, source)}


def schema_tables(path, source):
    """抽取设计器声明的配方表和机器参数表，不将默认值当作现场数据。"""
    tables = []
    datasets = re.finditer(
        r"this\.(RecipeData|SettingData)\.Tables\.AddRange\(.*?\{(.*?)\}\);",
        source, re.S)
    for dataset in datasets:
        for variable in re.findall(r"this\.(\w+)", dataset[2]):
            attrs = properties(source, variable)
            columns = re.search(rf"this\.{re.escape(variable)}\.Columns\.AddRange\(.*?\{{(.*?)\}}\);",
                                source, re.S)
            fields = []
            for column in re.findall(r"this\.(\w+)", columns[1] if columns else ""):
                field = properties(source, column)
                fields.append({"variable": column, "name": field.get("ColumnName"),
                               "type": field.get("DataType", "typeof(string)"),
                               "default_expression": field.get("DefaultValue")})
            tables.append({"scope": dataset[1], "variable": variable,
                           "name": attrs.get("TableName"), "columns": fields,
                           "source": evidence(path, source, dataset.start())})
    return tables


def resources(path, source):
    """抽取应用设计器中 SDK IO、运动、点位和数据表控件的声明及绑定。"""
    result = []
    types = r"NPSDK\.Motion\.NP_\w+|DataSave\.TypeCollection1"
    pattern = rf"this\.(\w+)\s*=\s*new\s+({types})\s*\(\s*\);"
    wanted = {"Port", "Vendor", "Text", "DataSet", "DataSet1", "WorkMotor",
              "CyTimeoutTime", "SavePath"}
    for match in re.finditer(pattern, source):
        attrs = properties(source, match[1])
        bindings = {key: value for key, value in attrs.items()
                    if key in wanted or key.startswith(("CyOn_", "CyOff_"))}
        resource = {"variable": match[1], "type": match[2], "bindings": bindings,
                    "source": evidence(path, source, match.start())}
        if match[2].endswith("NP_PositionTable"):
            parameter_set = re.search(
                rf"this\.{re.escape(match[1])}\.ParameterSet\.AddRange\(.*?\{{(.*?)\}}\);",
                source, re.S)
            column_names = re.findall(r"\bpositionColumn\w*\b", parameter_set[1] if parameter_set else "")
            column_names.extend(re.findall(
                rf"this\.{re.escape(match[1])}\.ParameterSet\.Add\((\w+)\);", source))
            mapping = []
            for column in dict.fromkeys(column_names):
                values = {m[1]: m[2].strip() for m in re.finditer(
                    rf"\b{re.escape(column)}\.(\w+)\s*=\s*([^;]+);", source)}
                mapping.append({"variable": column, "properties": values})
            resource["axis_columns"] = mapping
        result.append(resource)
    return result


def flow_module(path, source, by_path):
    """盘点 Flow_BaseForm 的控件图、既有回调和入口；不推断回调业务语义。"""
    cls = re.search(r"class\s+(\w+)\s*:\s*(?:NPSDK\.)?Flow_BaseForm\b", source)
    if not cls:
        return None
    designer_path = path[:-3] + ".Designer.cs"
    designer = by_path.get(designer_path, "")
    nodes = []
    for match in re.finditer(r"this\.(\w+)\s*=\s*new\s+NPSDK\.Flow_Chart\(\);", designer):
        attrs = properties(designer, match[1])
        callback = re.search(rf"this\.{re.escape(match[1])}\.FlowChartRun\s*\+=\s*([^;]+);", designer)
        nodes.append({"variable": match[1], "text_expression": attrs.get("text"),
                      "branches": {key: attrs[key] for key in ("Next", "Casse1", "Casse2", "Casse3") if key in attrs},
                      "callback_expression": callback[1] if callback else None,
                      "source": evidence(designer_path, designer, match.start())})
    entries = [{"node": match[1], "source": evidence(path, source, match.start())}
               for match in re.finditer(r"\b(\w+)\.FlowChart_Run\s*\(", source)]
    return {"class": cls[1], "source": evidence(path, source, cls.start()),
            "nodes": nodes, "entry_calls": entries}


def inventory(root):
    """组合编译源码中的可核查声明，不声称已获取运行实例或现场配置。"""
    sources = compiled_sources(root)
    by_path = dict(sources)
    forms, tables, controls, flows, devices = [], [], [], [], []
    for path, source in sources:
        for match in re.finditer(r"class\s+(\w+)\s*:\s*([\w.]+)", source):
            if match[2].split(".")[-1] in {"Form", "ModuleBaseForm", "Flow_BaseForm"}:
                forms.append({"class": match[1], "base": match[2],
                              "source": evidence(path, source, match.start())})
            if match[2] == "AbstractDevice":
                methods = [{"signature": m[1].strip(), "source": evidence(path, source, m.start())}
                           for m in re.finditer(r"\bpublic\s+((?:(?:override|virtual|static|async)\s+)*[\w<>\[\],.]+\s+\w+\s*\([^)]*\))", source)]
                devices.append({"class": match[1], "declared_public_methods": methods,
                                "source": evidence(path, source, match.start())})
        if path.lower().endswith(".designer.cs"):
            tables.extend(schema_tables(path, source))
            controls.extend(resources(path, source))
        flow = flow_module(path, source, by_path)
        if flow:
            flows.append(flow)
    counts = dict(sorted(Counter(control["type"] for control in controls).items()))
    return {
        "format_version": 1,
        "scope": "AlphaRap.csproj 显式 Compile 项的静态声明，不含硬件探测、动态对象或配置实例计数",
        "limitations": ["基于常见设计器语法的正则抽取，不是 C# 语义分析器。",
                        "未计算 MSBuild 条件、继承方法、动态创建的相机/VPP/Device 实例或动态托盘列。",
                        "配方字段默认值和 IO Port 是源码表达式，不能当作现场已验证配置。",
                        "公开方法包含底层入口和事件回调，不等于应用工程师可直接选择的动作清单。"],
        "summary": {"explicit_compile_files": len(sources), "forms": len(forms),
                    "flow_modules": len(flows), "flow_nodes": sum(len(flow["nodes"]) for flow in flows),
                    "device_types": len(devices), "declared_tables": len(tables),
                    "resource_types": counts},
        "forms": forms, "flow_modules": flows, "device_types": devices,
        "tables": tables, "resources": controls,
    }


def main():
    """生成清单或验证清单与当前源码一致；差异返回非零退出码。"""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=Path("docs/EquipmentFrameworkInventory.json"))
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    result = inventory(args.root.resolve())
    content = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    output = args.output if args.output.is_absolute() else args.root / args.output
    if args.check:
        if not output.is_file() or output.read_text(encoding="utf-8") != content:
            raise ValueError("资源清单与源码不一致，请先重新生成。")
    else:
        output.write_text(content, encoding="utf-8", newline="\n")
    print(json.dumps(result["summary"], ensure_ascii=False))


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, ET.ParseError) as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
