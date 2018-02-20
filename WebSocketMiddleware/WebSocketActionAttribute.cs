using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketMiddleware
{
    public class WebSocketActionAttribute : Attribute
    {
        public string Action { get; }

        public WebSocketActionAttribute(string action = null)
        {
            Action = action;
        }
    }
}
