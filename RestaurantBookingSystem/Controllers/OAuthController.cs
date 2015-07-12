using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Controllers
{
    public class OAuthController : Controller
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

        //
        // GET: /OAuth/

        public ActionResult Authorize(string state)
        {
            if (TempData.Peek(TempDataStringResuorce.FacebookStateData) != null && CryptographyHelper.MatchOneTimeHash(TempData[TempDataStringResuorce.FacebookStateData].ToString(), state))
            {
                var query = new
                                  {
                                      Code = Request.QueryString["code"],
                                      Error = Request.QueryString["error"],
                                      ErrorReason = Request.QueryString["error_reason"],
                                      ErrorDescription = Request.QueryString["error_description"]
                                  };
                if (query.Error != null && query.ErrorReason == "user_denied")
                {
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Message = "It looks like you didn't Allowed us to Sign you in through Facebook, you can try again",
                        Result = false,
                        State = ActionResultNotification.MessageState.Error
                    };
                    return RedirectToAction("LogIn", "Account");
                }
                if (!query.Code.IsNullOrEmpty())
                {
                    var redirecturi = Url.AbsoluteAction("Authorize", "OAuth", null);
                    var request =
                        WebRequest.Create(FacebookHelper.GetAccessTokenUrl(AppConfigHelper.FacebookAppId, redirecturi,
                                                                           AppConfigHelper.FacebookAppSecret, query.Code));
                    try
                    {
                        var response = request.GetResponse();
                        var responsestream = response.GetResponseStream();
                        if (responsestream != null && response.ContentLength > 0)
                        {
                            var responsebody = new StreamReader(responsestream).ReadToEnd();
                            var token = new
                                            {
                                                AccessToken = responsebody.Split('&').First().Split('=').Last(),
                                                ExpiresOn = DateTime.UtcNow.AddSeconds(Convert.ToDouble(responsebody.Split('&').Last().Split('=').Last()))
                                            };
                            //Reuse varaibles to get User Details
                            request = WebRequest.Create(FacebookHelper.GetFacebookNewUserUrl(token.AccessToken));
                            response = request.GetResponse();
                            responsestream = response.GetResponseStream();
                            if (responsestream != null)
                            {
                                responsebody = new StreamReader(responsestream).ReadToEnd();
                                var facebookuser = System.Web.Helpers.Json.Decode<FacebookUser>(responsebody);
                                var facebookdetail = new FacebookUserDetail()
                                                         {
                                                             FacebookId = facebookuser.id,
                                                             ExpiresOn = token.ExpiresOn,
                                                             OAuthToken = token.AccessToken,
                                                             ProfileLink = facebookuser.link
                                                         };

                                //Try update with new facebookuser detail, if user dosent exist it will return false
                                //Though Membership class returns a status of Duplicate user, for now I am going with it
                                var restaurantuser = (RestaurantUser)null;
                                if (!new FacebookUserDetailRepository().Update(facebookdetail))
                                {
                                    restaurantuser = new RestaurantUser(0, facebookuser.name, Guid.NewGuid())
                                                         {
                                                             EmailId = facebookuser.email,
                                                             FacebookDetail = facebookdetail,
                                                             UserRole = UserBase.RestaurantUserRole.Customer
                                                         };
                                    MembershipCreateStatus createstatus;
                                    restaurantuser = MembershipService.CreateUser(restaurantuser, out createstatus);
                                }
                                if (restaurantuser == null) restaurantuser = MembershipService.GetUser(facebookdetail.FacebookId.ToString(), true);
                                if (restaurantuser != null)
                                {
                                    //If all goes well Log the user in
                                    FormsService.SignIn(restaurantuser, true);
                                }
                            }
                            TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                                    {
                                        Message = "You have been successully Logged in",
                                        Result = true,
                                        State = ActionResultNotification.MessageState.Information
                                    };
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    catch (WebException exception)
                    {
                        return ProcessResponseErrorRedirect(redirecturi, exception);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public void Deauthorize()
        {
            //return RedirectToAction("Index", "Home");
        }

        [NonAction]
        private ActionResult ProcessResponseErrorRedirect(string redirecturi, WebException exception)
        {
            if (exception != null && exception.Response != null)
            {
                var responsestream = exception.Response.GetResponseStream();
                if (responsestream != null && exception.Response.ContentLength > 0)
                {
                    var responsebody = new StreamReader(responsestream).ReadToEnd();
                    var errorresponse = System.Web.Helpers.Json.Decode<FacebookErrorResponse>(responsebody);
                    if (errorresponse.error.type == "OAuthException")
                    {
                        return Redirect(FacebookHelper.GetSignUpUrl(AppConfigHelper.FacebookAppId, new List<string> { "email" },
                                                                    redirecturi, TempData).AbsoluteUri);
                    }
                }
            }
            TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Message = "Some network related error occured, Please try again",
                        Result = false,
                        State = ActionResultNotification.MessageState.Warning
                    };
            return RedirectToAction("LogIn", "Account");
        }
    }
}
