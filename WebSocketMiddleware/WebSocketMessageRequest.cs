using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketMiddleware
{
    public class WebSocketMessageRequest
    {
        [JsonProperty("id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty("action", Required = Required.Always)]
        public string Action { get; set; }

        [JsonProperty("args", Required = Required.Always)]
        public object[] Args { get; set; }
    }
}
