using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LaboratorioCRUDAlumnos
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Necesario al hospedar fuera de IIS integrado (Wine/Sikarugir); sin efecto negativo en IIS.
            System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
