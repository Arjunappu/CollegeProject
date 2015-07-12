using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Helpers;

namespace RestaurantBookingSystem.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }

        //
        // GET: /Customers/

        public ActionResult Index()
        {
            return null;
        }

        //
        // GET: /Customers/CheckIn

        public ActionResult CheckIn()
        {
            return null;
        }

        //
        // GET: /Customers/CheckIn
        [HttpPost]
        public ActionResult CheckIn(int bookingid)
        {
            return null;
        }
    }
}
