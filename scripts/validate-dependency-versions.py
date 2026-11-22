#!/usr/bin/env python3
"""Reject partial dependency/tool upgrades before NuGet restore."""

import json
from pathlib import Path
import sys
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parents[1]
PROJECTS = ("Application", "Domain", "Infrastructure", "WebUI")
references = {}
frameworks = set()
errors = []

for project in PROJECTS:
    path = ROOT / project / f"{project}.csproj"
    tree = ET.parse(path)
    frameworks.update(node.text for node in tree.findall(".//TargetFramework"))
    for reference in tree.findall(".//PackageReference"):
        name = reference.attrib["Include"]
        version = reference.get("Version") or reference.findtext("Version")
        if not version:
            errors.append(f"{path.relative_to(ROOT)}: {name} has no explicit version")
        references.setdefault(name, {}).setdefault(version, []).append(project)

for name, versions in sorted(references.items()):
    if len(versions) != 1:
        errors.append(f"{name} differs between projects: {versions}")

if len(frameworks) != 1:
    errors.append(f"Projects target different frameworks: {sorted(frameworks)}")

ef_versions = set()
for name, versions in references.items():
    if name.startswith("Microsoft.EntityFrameworkCore") or name in {
        "Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore",
        "Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore",
    }:
        ef_versions.update(versions)

manifest = json.loads((ROOT / "WebUI/.config/dotnet-tools.json").read_text())
tool_version = manifest["tools"]["dotnet-ef"]["version"]
if ef_versions != {tool_version}:
    errors.append(
        f"EF runtime/design packages {sorted(ef_versions)} and dotnet-ef "
        f"{tool_version} must be updated together"
    )

if errors:
    print("Dependency alignment failed:", file=sys.stderr)
    for error in errors:
        print(f"- {error}", file=sys.stderr)
    sys.exit(1)

print(f"Dependency alignment passed; EF packages and local tool use {tool_version}.")
