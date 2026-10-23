# Laboratorio CRUD de Alumnos — ASP.NET MVC 5 (.NET Framework 4.8)

CRUD de alumnos (listar, detalles, registrar, editar, eliminar con confirmación) con **ASP.NET MVC sobre
.NET Framework 4.8**, C#, Razor y Bootstrap 3. Los alumnos se guardan **únicamente en `Session["alumnos"]`**
(sin base de datos, Entity Framework, archivos ni variables estáticas), como pide el laboratorio.

## Estructura (abrir `LaboratorioCRUDAlumnos/LaboratorioCRUDAlumnos.sln` en Visual Studio)

| Ruta | Contenido |
|---|---|
| `Models/AlumnoModel.cs` | Id, Nombres, Apellidos, Edad, Correo (con validaciones) |
| `Controllers/AlumnoController.cs` | Acciones GET/POST; inicializa `Session["alumnos"]` solo si no existe (3 alumnos de ejemplo) |
| `Views/Alumno/` | `Index`, `Details`, `Create`, `Edit`, `Delete` |
| `Views/Shared/_Layout.cshtml` | Diseño base Bootstrap |

En Windows: abrir la solución, restaurar paquetes NuGet (`packages.config`) y presionar **F5** (IIS Express).

## Ejecutarlo en macOS (sin Windows)

Se usó un wrapper de **Sikarugir** (Wine 10) con **.NET Framework 4.8** instalado vía winetricks.
IIS Express no funciona en Wine (depende de `http.sys`), por eso `LabHost/` es un mini‑host
(estilo Cassini) que entrega cada petición TCP al runtime real de ASP.NET (`System.Web`).

```bash
# 1) Compilar la app (genera bin/ con el MVC 5) y restaurar paquetes en ../packages (ver packages.config)
# 2) Lanzar con Wine:
scripts/ejecutar-en-mac.sh 44300     # abre http://localhost:44300/
```

Detalles específicos del entorno Wine (no afectan a IIS): `shadowCopyBinAssemblies="false"`,
`healthMonitoring` deshabilitado y `AntiForgeryConfig.SuppressXFrameOptionsHeader = true`.

## Capturas

Ver carpeta [`capturas/`](capturas) y el documento `Semana_09_..._con_evidencias.docx`.
