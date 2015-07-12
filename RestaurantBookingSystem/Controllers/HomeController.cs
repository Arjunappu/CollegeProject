using System.Web.Mvc;
using System.Web.Security;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Helpers;

namespace RestaurantBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        public readonly HomeViewModel ViewModel;

        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }

        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View(ViewModel);
        }

        //
        // GET: /Home/About

        public ActionResult About()
        {
            return View();
        }

        //
        // GET: /Home/Privacy

        public ActionResult Privacy()
        {
            return View();
        }

        public HomeController() :
            this(new HomeViewModel())
        {
        }

        public HomeController(HomeViewModel viewmodel)
        {
            ViewModel = viewmodel;
        }
    }
}
