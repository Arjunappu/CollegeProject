// --------------------------------
// <copyright file="FacebookHelper.cs" company="Samaj Shekhar">
//     Copyright (c) 2010-2011 Samaj Shekhar
// </copyright>
// <author>Samaj Shekhar (samaj.shekhar@hotmail.com)</author>
// <license>Released under the terms of the Microsoft Public License (Ms-PL)</license>
// <website>http://hg.shekharpro.com</website>
// ---------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace RestaurantBookingSystem.Helpers
{
    public static class FacebookHelper
    {
        public static Uri GetSignUpUrl(ulong appId, IList<string> permissions, string redirectUri, TempDataDictionary tempData)
        {
            if (permissions == null) throw new ArgumentNullException("permissions", "Value cannot be null.");
            if (appId < 1) throw new ArgumentOutOfRangeException("appId", "Value cannot be less than 1.");
            if (redirectUri.IsNullOrEmpty()) throw new ArgumentNullException("redirecturi", "Value cannot be Null or Empty.");

            tempData[TempDataStringResuorce.FacebookStateData] = Guid.NewGuid().ToString("x");
            var queryparameters = new Dictionary<string, object>
                                      {
                                          {"client_id", appId},
                                          {"redirect_uri", redirectUri},
                                          {"scope", permissions.ToString(',')},
                                          {"state", CryptographyHelper.GetOneTimeHash(tempData.Peek(TempDataStringResuorce.FacebookStateData).ToString())}
                                      };
            var uribuilder = new UriBuilder
                                 {
                                     Scheme = "https",
                                     Host = "www.facebook.com",
                                     Path = "dialog/oauth",
                                     Query = queryparameters.ToUriQueryString(false)
                                 };
            var result = uribuilder.Uri;

            return result;
       }

        public static Uri GetAccessTokenUrl(ulong appId, string redirectUri, string clientSecret, string code)
        {
            if (appId < 1) throw new ArgumentOutOfRangeException("appId", "Value cannot be less than 1.");
            if (redirectUri.IsNullOrEmpty()) throw new ArgumentNullException("redirecturi", "Value cannot be Null or Empty.");

            var queryparameters = new Dictionary<string, object>
                                      {
                                          {"client_id", appId},
                                          {"redirect_uri", redirectUri},
                                          {"client_secret", clientSecret},
                                          {"code", code}
                                      };
            var uribuilder = new UriBuilder
                                 {
                                     Scheme = "https",
                                     Host = "graph.facebook.com",
                                     Path = "oauth/access_token",
                                     Query = queryparameters.ToUriQueryString(false)
                                 };
            var result = uribuilder.Uri;

            return result;
        }

        public static Uri GetFacebookNewUserUrl(string accessToken)
        {
            if (accessToken.IsNullOrEmpty()) throw new ArgumentNullException("accessToken", "Value cannot be null or Empty.");

            var queryparameters = new Dictionary<string, object>
                                      {
                                          {"access_token", accessToken}
                                      };
            var uribuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "graph.facebook.com",
                Path = "me",
                Query = queryparameters.ToUriQueryString(false)
            };
            var result = uribuilder.Uri;

            return result;
        }

        public static string GetFacebookPictureUrl(Int64 userID)
        {
            var sb = new StringBuilder();
            sb.Append("https://");
            sb.AppendFormat("graph.facebook.com/{0}/picture", userID.ToString());
            return sb.ToString();
        }


        public static string GetFacebookPictureUrl(string userID)
        {
            var sb = new StringBuilder();
            sb.Append("https://");
            sb.AppendFormat("graph.facebook.com/{0}/picture", userID);
            return sb.ToString();
        }

        public static Uri GetFacebookBigPictureUrl(ulong userid, string accessToken)
        {
            if (accessToken.IsNullOrEmpty()) throw new ArgumentNullException("accessToken", "Value cannot be null or Empty.");

            var queryparameters = new Dictionary<string, object>
                                      {
                                          {"type", "large"},
                                          {"access_token", accessToken}
                                      };
            var uribuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "graph.facebook.com",
                Path = string.Format("{0}/picture", userid),
                Query = queryparameters.ToUriQueryString(false)
            };
            var result = uribuilder.Uri;

            return result;
        }

        public static string GetFacebookBigPictureUrl(Int64 userID)
        {
            var sb = new StringBuilder();
            sb.Append("https://");
            sb.AppendFormat("graph.facebook.com/{0}/picture", userID.ToString());
            sb.AppendFormat("?&type=large");
            return sb.ToString();
        }

        public static string GetFacebookFriendsUrl(Int64 userID, string accessToken)
        {
            var sb = new StringBuilder();
            sb.Append("https://");
            sb.AppendFormat("graph.facebook.com/{0}/friends", userID.ToString());
            sb.AppendFormat("?access_token={0}", accessToken);
            return sb.ToString();
        }


        public static string GetFacebookFeedUrl(Int64 userID, string accessToken)
        {
            var sb = new StringBuilder();
            sb.Append("https://");
            sb.AppendFormat("graph.facebook.com/{0}/feed", userID.ToString());
            sb.AppendFormat("?limit=5&access_token={0}", accessToken);
            return sb.ToString();
        }
    }

    // ReSharper disable InconsistentNaming
    [Serializable]
    public class FacebookUser
    {
        /// <summary>
        /// The user's facebook Unique ID
        /// </summary>
        public ulong id { get; set; }

        /// <summary>
        /// The user's Name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The user's link to Profile on facebook
        /// </summary>
        public Uri link { get; set; }

        /// <summary>
        /// The user's gender
        /// </summary>
        public string gender { get; set; }

        /// <summary>
        /// The user's Email
        /// </summary>
        public string email { get; set; }
    }

    [Serializable]
    public class FacebookErrorResponse
    {
        public FacebookError error { get; set; }
    }

    [Serializable]
    public class FacebookError
    {
        public string message { get; set; }
        public string type { get; set; }
        public string code { get; set; }
    }
    // ReSharper restore InconsistentNaming
}