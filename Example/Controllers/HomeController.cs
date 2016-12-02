using System.Web.Mvc;
using IdentityPermissionExtension;

namespace Example.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            if (HttpContext.AuthorizePermission(name: "Authorized_About", description: "Show if authorized."))
            {
                ViewBag.Message = "Authorized.";
            }
            else
            {
                ViewBag.Message = "UnAthorized.";
            }

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}