using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebSocketMiddleware;

namespace TLWatch.Controllers
{
    public class ChatController : WebSocketController
    {

        public override async Task OnBinaryMessage(Stream stream)
        {
            byte[] buf = new byte[4096];
            int totalRead = 0;
            int read;
            while ((read = await stream.ReadAsync(buf, 0, buf.Length)) > 0) {
                totalRead += read;
                Console.WriteLine("binary part length " + read);
            }
            Console.WriteLine("total length " + totalRead);
        }

        [WebSocketAction]
        public string Echo(string msg)
        {
            return msg;
        }
    }
}
