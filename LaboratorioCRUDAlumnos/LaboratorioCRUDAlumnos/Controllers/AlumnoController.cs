using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using LaboratorioCRUDAlumnos.Models;

namespace LaboratorioCRUDAlumnos.Controllers
{
    public class AlumnoController : Controller
    {
        private const string SessionKey = "alumnos";

        // Recupera la colección desde Session["alumnos"].
        // Solo se inicializa (con datos de ejemplo) si todavía no existe en la sesión.
        private List<AlumnoModel> ObtenerAlumnos()
        {
            var alumnos = Session[SessionKey] as List<AlumnoModel>;
            if (alumnos == null)
            {
                alumnos = new List<AlumnoModel>
                {
                    new AlumnoModel { Id = 1, Nombres = "María Fernanda", Apellidos = "Quispe Rojas",  Edad = 20, Correo = "maria.quispe@instituto.edu.pe" },
                    new AlumnoModel { Id = 2, Nombres = "Carlos Alberto",  Apellidos = "Huamán Soto",   Edad = 22, Correo = "carlos.huaman@instituto.edu.pe" },
                    new AlumnoModel { Id = 3, Nombres = "Lucía Valentina", Apellidos = "Torres Medina", Edad = 19, Correo = "lucia.torres@instituto.edu.pe" }
                };
                Session[SessionKey] = alumnos;
            }
            return alumnos;
        }

        // GET: Alumno
        public ActionResult Index()
        {
            return View(ObtenerAlumnos());
        }

        // GET: Alumno/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var alumno = ObtenerAlumnos().FirstOrDefault(a => a.Id == id);
            if (alumno == null) return HttpNotFound();
            return View(alumno);
        }

        // GET: Alumno/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Alumno/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombres,Apellidos,Edad,Correo")] AlumnoModel alumno)
        {
            if (!ModelState.IsValid) return View(alumno);

            var alumnos = ObtenerAlumnos();
            alumno.Id = alumnos.Count == 0 ? 1 : alumnos.Max(a => a.Id) + 1;
            alumnos.Add(alumno);
            Session[SessionKey] = alumnos;

            TempData["Mensaje"] = "Alumno registrado correctamente.";
            return RedirectToAction("Index");
        }

        // GET: Alumno/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var alumno = ObtenerAlumnos().FirstOrDefault(a => a.Id == id);
            if (alumno == null) return HttpNotFound();
            return View(alumno);
        }

        // POST: Alumno/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombres,Apellidos,Edad,Correo")] AlumnoModel alumno)
        {
            if (!ModelState.IsValid) return View(alumno);

            var alumnos = ObtenerAlumnos();
            var existente = alumnos.FirstOrDefault(a => a.Id == alumno.Id);
            if (existente == null) return HttpNotFound();

            existente.Nombres = alumno.Nombres;
            existente.Apellidos = alumno.Apellidos;
            existente.Edad = alumno.Edad;
            existente.Correo = alumno.Correo;
            Session[SessionKey] = alumnos;

            TempData["Mensaje"] = "Alumno actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // GET: Alumno/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var alumno = ObtenerAlumnos().FirstOrDefault(a => a.Id == id);
            if (alumno == null) return HttpNotFound();
            return View(alumno);
        }

        // POST: Alumno/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var alumnos = ObtenerAlumnos();
            var alumno = alumnos.FirstOrDefault(a => a.Id == id);
            if (alumno == null) return HttpNotFound();

            alumnos.Remove(alumno);
            Session[SessionKey] = alumnos;

            TempData["Mensaje"] = "Alumno eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
