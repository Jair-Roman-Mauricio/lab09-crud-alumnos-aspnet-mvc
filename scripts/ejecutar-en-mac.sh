#!/bin/bash
# Ejecuta la app ASP.NET MVC (.NET Framework 4.8) en macOS con el wrapper de Sikarugir (Wine).
# Uso: scripts/ejecutar-en-mac.sh [puerto]
set -e
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PORT="${1:-44300}"
A="$HOME/Applications/Sikarugir/LaboratorioMVC.app/Contents"; W="$A/SharedSupport/wine"
export WINEPREFIX="$A/SharedSupport/prefix" WINEDEBUG=-all
export DYLD_FALLBACK_LIBRARY_PATH="$A/Frameworks:$W/lib:/usr/lib"
export PATH="$W/bin:$PATH"
cd "$ROOT/LabHost" && dotnet build -c Release -o out -v q >/dev/null
cd "$ROOT/LaboratorioCRUDAlumnos/LaboratorioCRUDAlumnos"
echo "Abre http://localhost:$PORT/"
exec wine "$ROOT/LabHost/out/LabHost.exe" "$(pwd)" "$PORT"
