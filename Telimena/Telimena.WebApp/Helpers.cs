using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Telimena.WebApp
{
    /// <summary>
    /// Url helper
    /// </summary>
    public static class TelimenaUrlHelper
    {
        /// <summary>
        /// Adds the latest API version to the route values dictionary
        /// </summary>
        /// <param name="url"></param>
        /// <param name="routeName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string LatestApiUrl(this UrlHelper url, string routeName, object routeValues = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(routeValues) {{"version", "1"}};

            return url.HttpRouteUrl(routeName, routeValueDictionary);
        }

        /// <summary>
        /// Gets an API url without version component.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>System.String.</returns>
        public static string NeutralApiUrl(this UrlHelper url, string routeName, object routeValues = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(routeValues);

            return url.HttpRouteUrl(routeName, routeValueDictionary);
        }
    }
}