using DevQuiz.Repository;
using System.Web.Mvc;

namespace DevQuiz.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationRepository repository = new ApplicationRepository();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "This is a fun quiz app for developers";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact Danielle Greenberg at danielle.9897@gmail.com";

            return View();
        }
    }
}