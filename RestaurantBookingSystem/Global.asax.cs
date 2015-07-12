using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public override void Init()
        {
            RegisterAuthentication();
            TryCreateFirstAdmin();
            base.Init();
        }

        private void TryCreateFirstAdmin()
        {
            if (Membership.Provider.GetUser("spock@enterprise.com", false)  != null) return;
            
            try
            {
                var firstadmin = new RestaurantUser(0,"Mr. Spock", Guid.NewGuid())
                                     {
                                         UserRole = UserBase.RestaurantUserRole.Admin,
                                         EmailId = "spock@enterprise.com",
                                         Password = "warpspeedfactor4",
                                         Address = "On Starship Enterprise, The Advanced FTL Fighter Ship, Crusing through space, going places where no one has gone before :)",
                                         MobileNumber = 9876543210
                                     };
                MembershipCreateStatus status;
                ((Infrastructure.Providers.CustomSqlMembershipProviderWrapper)Membership.Provider).CreateUser(
                     firstadmin,out status, null, null);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch{}
            // ReSharper restore EmptyGeneralCatchClause
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "", // Route name
                "Images/{action}/{id}", // URL with parameters
                new { controller = "Images", action = "GetImage", id = "" }, // Parameter defaults
                new[] { "RestaurantBookingSystem.Controllers" }
                );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = "" }, // Parameter defaults
                new[] { "RestaurantBookingSystem.Controllers" }
                );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private void RegisterAuthentication()
        {
            AuthenticateRequest += OnAuthenticateRequest;
            PostAuthenticateRequest += OnPostAuthenticateRequest;
        }

        private void OnPostAuthenticateRequest(object s, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return;
            var restaurantidentity = FormsAuthenticationHelper.GetRestaurantUserIdentityFromCookie(authCookie);
            if (restaurantidentity == null) return;
            var userrole = new RestaurantUserRepository().GetRestaurantUserRole(restaurantidentity.UserId);
            HttpContext.Current.User = new GenericPrincipal(
                restaurantidentity,
                new[] { userrole.ToString() }
                );
        }

        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
        }
    }
}