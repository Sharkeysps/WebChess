using System;
using System.Linq;
using System.Web.Mvc;

namespace Chess.Server.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
