using System.Web.Mvc;

namespace LaboratorioCRUDAlumnos.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Laboratorio de CRUD de alumnos con ASP.NET MVC.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Página de contacto.";
            return View();
        }
    }
}
