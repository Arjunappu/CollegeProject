using System;
using System.Web;
using System.Web.Mvc;

namespace RestaurantBookingSystem.Helpers
{
    public static class UriHelperExtentions
    {
        /// <summary>
        /// Generates a fully qualified Url String to an action method by using the specified action name, controller name, route values and context
        /// </summary>
        /// <param name="helper">The UriHelper refrence</param>
        /// <param name="actionName">A action name string</param>
        /// <param name="controllerName">A controller name string</param>
        /// <param name="routeValues">A object that contains parameter for routes</param>
        /// <returns>A fully qualified absolute Url of the action method</returns>
        public static string AbsoluteAction(this UrlHelper helper, string actionName, string controllerName, object routeValues)
        {
            var uri = new UriBuilder(helper.Action(actionName,controllerName,routeValues,HttpContext.Current.Request.Url.Scheme));
            //If request is not local then remove Port number by setting port to -1, 
            //this will cause port number to be removed alltogether, and since we 
            //are returning string it wont be noticed by .net Uri parser
            if (!helper.RequestContext.HttpContext.Request.IsLocal)
                uri.Port = -1;
            return uri.ToString();
        }
    }
}