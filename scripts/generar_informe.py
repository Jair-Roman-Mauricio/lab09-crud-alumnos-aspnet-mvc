"""Anexa al .docx del laboratorio la sección de evidencias (capturas + código + link de GitHub)."""
import os, shutil, struct, subprocess, sys, tempfile
from xml.sax.saxutils import escape

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
SRC = os.path.join(ROOT, 'Semana_09_Seccion_B_Laboratorio_CRUD_Alumnos_ASP_NET_MVC.docx')
OUT = os.path.join(ROOT, 'Semana_09_Seccion_B_Laboratorio_CRUD_Alumnos_ASP_NET_MVC_con_evidencias.docx')
CAP = os.path.join(ROOT, 'capturas') + '/'
URL = 'https://github.com/Jair-Roman-Mauricio/lab09-crud-alumnos-aspnet-mvc'
items = [
 ('01-inicio.png', 'Página de inicio de la aplicación ASP.NET MVC ejecutándose en localhost:44300.'),
 ('02-listado-inicial.png', 'Primer ingreso: se cargan automáticamente los tres alumnos de ejemplo en Session["alumnos"].'),
 ('03-validacion-registro.png', 'Registro con campos vacíos: las validaciones de DataAnnotations muestran los mensajes obligatorios.'),
 ('04-formulario-registro.png', 'Formulario Create completado con los datos del nuevo alumno.'),
 ('05-alumno-creado.png', 'Tras el POST, el nuevo alumno (Id 4) aparece en el listado con mensaje de confirmación.'),
 ('06-detalles.png', 'Vista Details con la información completa del alumno seleccionado.'),
 ('07-editar.png', 'Vista Edit con los datos modificados (edad y correo) antes de guardar.'),
 ('08-alumno-editado.png', 'Los cambios realizados se reflejan en el listado.'),
 ('09-confirmar-eliminar.png', 'Vista Delete: se solicita confirmación antes de eliminar al alumno (Id 2).'),
 ('10-alumno-eliminado.png', 'Tras confirmar, el alumno Id 2 ya no aparece en el listado.'),
 ('11-persistencia-actualizar.png', 'Al actualizar el navegador (F5), los cambios se mantienen porque siguen en la sesión.'),
]
CODE = '''private List<AlumnoModel> ObtenerAlumnos()
{
    var alumnos = Session["alumnos"] as List<AlumnoModel>;
    if (alumnos == null)
    {
        alumnos = new List<AlumnoModel>
        {
            new AlumnoModel { Id = 1, Nombres = "María Fernanda", Apellidos = "Quispe Rojas", Edad = 20, Correo = "..." },
            new AlumnoModel { Id = 2, Nombres = "Carlos Alberto", Apellidos = "Huamán Soto", Edad = 22, Correo = "..." },
            new AlumnoModel { Id = 3, Nombres = "Lucía Valentina", Apellidos = "Torres Medina", Edad = 19, Correo = "..." }
        };
        Session["alumnos"] = alumnos;
    }
    return alumnos;
}'''

def png_size(p):
    return struct.unpack('>II', open(p, 'rb').read(24)[16:24])
def P(t, style=None):
    pp = f'<w:pPr><w:pStyle w:val="{style}"/></w:pPr>' if style else ''
    return f'<w:p>{pp}<w:r><w:t xml:space="preserve">{escape(t)}</w:t></w:r></w:p>'
def code(t):
    return ('<w:p><w:pPr><w:spacing w:after="0" w:line="240" w:lineRule="auto"/><w:shd w:val="clear" w:color="auto" w:fill="F2F2F2"/></w:pPr>'
            '<w:r><w:rPr><w:rFonts w:ascii="Consolas" w:hAnsi="Consolas" w:cs="Consolas"/><w:sz w:val="18"/></w:rPr>'
            f'<w:t xml:space="preserve">{escape(t)}</w:t></w:r></w:p>')

tmp = tempfile.mkdtemp()
subprocess.check_call(['unzip', '-q', SRC, '-d', tmp])
rels = ''; body = '<w:p><w:r><w:br w:type="page"/></w:r></w:p>'
body += P('10. Desarrollo y evidencias de funcionamiento', 'Heading1')
body += P('Entorno de ejecución', 'Heading2')
body += P('La aplicación se desarrolló como proyecto ASP.NET Web Application (.NET Framework 4.8) de tipo MVC 5, en C#, sin autenticación, con el nombre LaboratorioCRUDAlumnos (solución .sln y proyecto .csproj que se abren en Visual Studio). Como el equipo de desarrollo es un Mac (Apple Silicon) sin Windows, la aplicación se ejecutó con Sikarugir (Wine) usando el motor de ASP.NET de .NET Framework 4.8. No se utilizó base de datos: el almacenamiento es exclusivamente Session["alumnos"], según los requerimientos técnicos.')
body += P('Código que evidencia el uso de Session["alumnos"]', 'Heading2')
body += P('En AlumnoController, la colección List<AlumnoModel> se recupera de la sesión y solo se inicializa con los tres alumnos de ejemplo si todavía no existe:')
for l in CODE.split('\n'): body += code(l)
body += P('Cada acción de escritura (Create, Edit y Delete por POST) modifica la lista y vuelve a asignarla con Session["alumnos"] = alumnos.')
body += P('Capturas de las pruebas de funcionamiento', 'Heading2')
n = 20
for i, (f, sub) in enumerate(items, 1):
    w, h = png_size(CAP + f); cx = 6172200; cy = int(cx * h / w)
    shutil.copy(CAP + f, f'{tmp}/word/media/{f}')
    rid = f'rId{n}'; n += 1
    rels += f'<Relationship Id="{rid}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/{f}"/>'
    alt = escape(sub, {'"': '&quot;'})
    body += (f'<w:p><w:pPr><w:keepNext/></w:pPr><w:r><w:drawing><wp:inline xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture">'
             f'<wp:extent cx="{cx}" cy="{cy}"/><wp:docPr id="{10+i}" name="Captura {i}" descr="{alt}"/><wp:cNvGraphicFramePr><a:graphicFrameLocks noChangeAspect="1"/></wp:cNvGraphicFramePr>'
             f'<a:graphic><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture"><pic:pic><pic:nvPicPr><pic:cNvPr id="0" name="{f}"/><pic:cNvPicPr/></pic:nvPicPr>'
             f'<pic:blipFill><a:blip r:embed="{rid}"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill><pic:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm><a:prstGeom prst="rect"/></pic:spPr></pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>')
    body += P(f'Captura {i}. {sub}', 'Caption')
body += P('Repositorio en GitHub', 'Heading2')
lid = f'rId{n}'
rels += f'<Relationship Id="{lid}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink" Target="{URL}" TargetMode="External"/>'
body += P('Todo el código desarrollado (solución de Visual Studio, host para macOS, scripts y capturas) está en:')
body += f'<w:p><w:hyperlink r:id="{lid}"><w:r><w:rPr><w:color w:val="0563C1"/><w:u w:val="single"/></w:rPr><w:t>{URL}</w:t></w:r></w:hyperlink></w:p>'
p = f'{tmp}/word/document.xml'; s = open(p, encoding='utf8').read()
open(p, 'w', encoding='utf8').write(s.replace('<w:sectPr w:rsidR', body + '<w:sectPr w:rsidR', 1))
p = f'{tmp}/word/_rels/document.xml.rels'; s = open(p, encoding='utf8').read()
open(p, 'w', encoding='utf8').write(s.replace('</Relationships>', rels + '</Relationships>'))
if os.path.exists(OUT): os.remove(OUT)
subprocess.check_call(f'cd "{tmp}" && zip -Xqr "{OUT}" .', shell=True)
print('OK', OUT)
