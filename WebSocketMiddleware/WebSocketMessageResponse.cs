using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocketMiddleware
{
    public class WebSocketMessageResponse
    {
        [JsonProperty(Required = Required.AllowNull)]
        public Guid? Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool Success { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool HasValue { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public object Value { get; set; }
    }
}
