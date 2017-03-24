using System.Web;
using System.Web.Mvc;

namespace Elmah.Io.Mvc.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            throw new HttpException(500, "I Am The One Who Knocks");
        }
    }
}