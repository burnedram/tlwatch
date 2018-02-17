using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketMiddleware
{
    public class WebSocketActionAttribute : Attribute
    {
        public string Route { get; }

        public WebSocketActionAttribute(string route = null)
        {
            Route = route;
        }
    }
}
