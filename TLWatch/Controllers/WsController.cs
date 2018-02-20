using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketMiddleware;

namespace TLWatch.Controllers
{
    [WebSocketRoute("/ws/[controller]")]
    public class WsController : WebSocketController
    {
    }
}
