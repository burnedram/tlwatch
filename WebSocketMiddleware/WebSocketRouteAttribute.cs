using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketMiddleware
{
    public class WebSocketRouteAttribute : Attribute
    {
        public string Route { get; }

        public WebSocketRouteAttribute(string route = null)
        {
            Route = route;
        }
    }
}
