using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketMiddleware
{
    public class WebSocketMessageRequest
    {
        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Action { get; set; }

        [JsonProperty(Required = Required.Always)]
        public object[] Arguments { get; set; }
    }
}
