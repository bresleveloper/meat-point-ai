using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.SessionState;

namespace Starter_.NET_4._8_NG_18
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            string frontUrl = ConfigurationManager.AppSettings["FrontUrL"];

            // Enable CORS
            // if no frontUrl then wildcard. for dev make FrontUrL to "http://localhost:4200
            var cors = new EnableCorsAttribute(string.IsNullOrEmpty(frontUrl) ? frontUrl : "*", "*", "*");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
