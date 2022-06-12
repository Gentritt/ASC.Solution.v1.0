using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Solution.v1._1.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {

    }
}
