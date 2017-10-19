using System.Net;
using System.Web;
using System.Web.Http;

namespace Elmah.Io.WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        public IHttpActionResult Get()
        {
            throw new HttpException(500, "I Am the Danger");
        }
    }
}
