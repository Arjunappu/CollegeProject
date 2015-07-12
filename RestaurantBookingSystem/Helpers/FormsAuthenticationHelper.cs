using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Helpers;
using System.Web.Script.Serialization;

namespace RestaurantBookingSystem.Helpers
{
    public static class FormsAuthenticationHelper
    {
        public static FormsAuthenticationTicket MakeAuthTicket(RestaurantUser user, bool ispersistent)
        {
            var ticket = (FormsAuthenticationTicket)null;
            if (user != null)
            {
                //check if login has expired according to user instance, if it has then set loginexpire to new and correct datetime
                var loginexpiry = user.LoginExpiresOn.ToUniversalTime() < DateTime.UtcNow
                                            ? (user.UserRole == UserBase.RestaurantUserRole.Guest
                                                ? DateTime.UtcNow.AddMinutes(86400D)
                                                : DateTime.UtcNow.AddMinutes(FormsAuthentication.Timeout.TotalMinutes))
                                            : user.LoginExpiresOn.ToUniversalTime();
                ticket = new FormsAuthenticationTicket(1, // Version number
                    user.UserName,          //User unique name according to Membership database
                    DateTime.UtcNow,        //Time of creation
                    loginexpiry,            // Time of Ticket Expiry
                    ispersistent,           // Is ticket Persistent
                    SerializeUserdata(user) // Add user specific data like ID, UniqueId and Friendly Name
                    );
            }

            //Also set the loginexpireson datetime for user
            //if (ticket != null)
            //{
            //    user.LoginExpiresOn = ticket.Expiration;
            //    new RestaurantUserRepository().Update(user);
            //}

            return ticket;
        }

        public static void SetAuthCookie(FormsAuthenticationTicket ticket)
        {
            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            if (encryptedTicket.IsNullOrEmpty())
                throw new HttpException("Failed to set Authentication Ticket");
            var httpCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                                 {
                                     HttpOnly = true,
                                     Path = FormsAuthentication.FormsCookiePath,
                                     Secure = FormsAuthentication.RequireSSL
                                 };
            if (FormsAuthentication.CookieDomain != null)
                httpCookie.Domain = FormsAuthentication.CookieDomain;
            if (ticket.IsPersistent)
                httpCookie.Expires = ticket.Expiration;
            HttpContext.Current.Response.Cookies.Add(httpCookie);
        }

        public static void SetAuthCookie(RestaurantUser user, bool ispersistent)
        {
            SetAuthCookie(MakeAuthTicket(user, ispersistent));
        }

        public static RestaurantUserIdentity GetRestaurantUserIdentityFromCookie(HttpCookie cookie)
        {
            var identity = (RestaurantUserIdentity)null;
            if (cookie != null && cookie.Name == FormsAuthentication.FormsCookieName && cookie.Value != null)
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket != null)
                {
                    dynamic userdata = null;
                    try
                    {
                        userdata = DeserializeUserData(ticket.UserData);
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch { }
                    // ReSharper restore EmptyGeneralCatchClause
                    if (userdata != null)
                    {
                        var authenticationtype = GetAuthenticationTypeForRestaurantUser(userdata.UserId);
                        if ((int)authenticationtype > -1)
                            identity = new RestaurantUserIdentity(
                                ticket.Name,
                                authenticationtype,
                                true, //if we are this far then the user is obviously authenticated
                                userdata.UserId,
                                userdata.FriendlyName,
                                userdata.UserGuid
                                );
                    }
                }
            }
            return identity;
        }

        public static AuthenticationType GetAuthenticationTypeForRestaurantUser(int userid)
        {
            var result = (AuthenticationType)(-1);
            if (userid > 0)
            {
                var user = new RestaurantUserRepository().Find(userid);
                if (user != null)
                {
                    var restaurantuser = RestaurantUserRepository.UserBaseToRestaurantUser(user);
                    if (restaurantuser.UserRole == UserBase.RestaurantUserRole.Guest)
                        result = AuthenticationType.Guest;
                    if (restaurantuser.UserRole == UserBase.RestaurantUserRole.Customer && restaurantuser.FacebookDetail != null && restaurantuser.FacebookDetail.FacebookId > 0)
                        result = AuthenticationType.Facebook;
                    if (restaurantuser.UserRole == UserBase.RestaurantUserRole.Customer && (restaurantuser.FacebookDetail == null || restaurantuser.FacebookDetail.FacebookId < 1))
                        result = AuthenticationType.Normal;
                    if (restaurantuser.UserRole == UserBase.RestaurantUserRole.Employee || restaurantuser.UserRole == UserBase.RestaurantUserRole.Admin)
                        result = AuthenticationType.Normal;
                }
            }
            return result;
        }

        private static string SerializeUserdata(UserBase user)
        {
            if (user == null)
                throw new ArgumentNullException("user", "A user data cannot be made for ticket from a null RestaurantUser instance");
            if (user.UserId < 1 || user.Name.IsNullOrEmpty() || user.UserGuid == Guid.Empty)
                throw new InvalidOperationException("A user data cannot be made for ticket from an uninitialized RestaurantUser instance");
            return new JavaScriptSerializer().Serialize(new { FriendlyName = user.Name, user.UserId, UserGuid = user.UserGuid.ToString("x") });
        }

        private static dynamic DeserializeUserData(string userdata)
        {
            var data = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(userdata);
            return new { FriendlyName = data["FriendlyName"].ToString(), UserId = Convert.ToInt32(data["UserId"]), UserGuid = Guid.Parse(data["UserGuid"].ToString()) };
        }

        /// <summary>
        /// Represents the different types of authentication supported by this app
        /// </summary>
        public enum AuthenticationType
        {
            /// <summary>
            /// Authentication type for a Guset user
            /// </summary>
            Guest = 100,

            /// <summary>
            /// Authentication type for a Facebook user of Customer Role
            /// </summary>
            Facebook = 200,

            /// <summary>
            /// Authentication type for a any User of any Role except Guest
            /// </summary>
            Normal = 300
        }
    }
}