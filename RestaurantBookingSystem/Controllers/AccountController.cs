using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }


        //It shows details of the currently logged in user, named index to match with route defaults
        // **************************************
        // URL: /Account
        // **************************************
        [Authorize]
        public ActionResult Index()
        {
            var user = User.Identity as RestaurantUserIdentity;
            if (user != null && user.GetAuthenticationType != FormsAuthenticationHelper.AuthenticationType.Guest)
                return View(MembershipService.GetUser(user.UserGuid, true));
            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/LogIn
        // **************************************

        public ActionResult LogIn()
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(LogOnModel model, string returnUrl, string utz)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.Email, model.Password))
                {
                    FormsService.SignIn(MembershipService.GetUser(model.Email, true), model.RememberMe);
                    DateTimeHelper.SetUserTimeZoneCookie(utz);
                    if (returnUrl != null && Url.IsLocalUrl(returnUrl) && (!returnUrl.ToUpperInvariant().Contains("LOGIN") && !returnUrl.ToUpperInvariant().Contains("REGISTER")))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // **************************************
        // URL: /Account/LogOut
        // **************************************
        [Authorize]
        public ActionResult LogOut()
        {
            var user = User.Identity as RestaurantUserIdentity;
            //For now i think we should allow Guest user to logout, though he can never login again but his details will remain in DB
            //if (user != null && user.GetAuthenticationType != FormsAuthenticationHelper.AuthenticationType.Guest)
                FormsService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                var restaurantuser = MembershipService.CreateUser(
                     new RestaurantUser(0, model.UserName, Guid.NewGuid())
                     {
                         EmailId = model.Email,
                         MobileNumber = UInt64.Parse(model.MobileNumber ?? "0"),
                         Address = model.Address,
                         UserRole = UserBase.RestaurantUserRole.Customer,
                         Password = model.Password
                     }
                     , out createStatus, model.SecretQuestion, model.SecretAnswer);

                if (createStatus == MembershipCreateStatus.Success && restaurantuser != null)
                {
                    FormsService.SignIn(restaurantuser, true);
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Message = "You have been successully Registered and Logged in",
                        Result = true,
                        State = ActionResultNotification.MessageState.Information
                    };
                    if (Request.QueryString["returnurl"] != null && Url.IsLocalUrl(Request.QueryString["returnurl"]) && (!Request.QueryString["returnurl"].ToUpperInvariant().Contains("LOGIN") && !Request.QueryString["returnurl"].ToUpperInvariant().Contains("REGISTER")))
                        return Redirect(Request.QueryString["returnurl"]);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("createstatus", AccountValidation.ErrorCodeToString(createStatus));
            }
            // If we got this far, something failed, redisplay form
            TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
            {
                Message = ModelState.ContainsKey("createstatus") ? ModelState["createstatus"].Errors[0].ErrorMessage : "There was an Error registering you as our Guest User, please try again",
                Result = false,
                State = ActionResultNotification.MessageState.Error
            };
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/RegisterGuest
        // **************************************

        public ActionResult RegisterGuest()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(new RegisterGuest());
        }

        [HttpPost]
        public ActionResult RegisterGuest(RegisterGuest model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                var restaurantuser = MembershipService.CreateUser(
                     new RestaurantUser(0, model.UserName, Guid.NewGuid())
                         {
                             MobileNumber = UInt64.Parse(model.MobileNumber),
                             UserRole = UserBase.RestaurantUserRole.Guest
                         }
                     , out createStatus);

                if (createStatus == MembershipCreateStatus.Success && restaurantuser != null)
                {
                    FormsService.SignIn(restaurantuser, true);
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Message = "You have been successully Registered as a Guest",
                        Result = true,
                        State = ActionResultNotification.MessageState.Information
                    };
                    if (Request.QueryString["returnurl"] != null && Url.IsLocalUrl(Request.QueryString["returnurl"]))
                        return Redirect(Request.QueryString["returnurl"]);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("createstatus", AccountValidation.ErrorCodeToString(createStatus));
            }
            // If we got this far, something failed, redisplay form
            TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
            {
                Message = ModelState.ContainsKey("createstatus") ? ModelState["createstatus"].Errors[0].ErrorMessage : "There was an Error registering you as our Guest User, please try again",
                Result = false,
                State = ActionResultNotification.MessageState.Error
            };
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var restaurantuseridentity = User.Identity as RestaurantUserIdentity;
            if (restaurantuseridentity == null) return RedirectToAction("Index", "Home");
            try
            {
                if (ModelState.IsValid)
                {
                    if (MembershipService.ChangePassword(restaurantuseridentity.UserGuid, model.OldPassword, model.NewPassword))
                    {
                        TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                                {
                                    Message = "Password was successfully changed",
                                    Result = true,
                                    State = ActionResultNotification.MessageState.Information
                                };
                        return RedirectToAction("Index");
                    }
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = "Failed to change your password, please try again",
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View(model);

            }
            catch (Exception e)
            {
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = e.Message,
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                ViewBag.PasswordLength = MembershipService.MinPasswordLength;
                return View(model);
            }

        }
    }
}
