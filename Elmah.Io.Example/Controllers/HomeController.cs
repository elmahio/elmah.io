using System;
using System.Web.Mvc;

namespace Elmah.Io.Example.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            throw new ApplicationException("Where is the center of the maze, Dolores?");
        }
    }
}