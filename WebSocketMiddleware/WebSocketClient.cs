using System;
using System.Net.WebSockets;

namespace WebSocketMiddleware
{
    public class WebSocketClient
    {
        public Guid Id { get; } = Guid.NewGuid();
        public WebSocket Socket { get; }

        public WebSocketClient(WebSocket socket)
        {
            this.Socket = socket;
        }
    }
}
