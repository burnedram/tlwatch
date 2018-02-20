using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketMiddleware
{
    public class WebSocketMiddleware
    {
        public Type ControllerType { get; }
        public WebSocketController Controller { get; private set; }
        public RequestDelegate Next { get; }
        public Dictionary<string, MethodInfo> ActionMap { get; private set; }

        public WebSocketMiddleware(Type controller, RequestDelegate next)
        {
            ControllerType = controller;
            Next = next;
        }

        private Dictionary<string, MethodInfo> GenerateActionMap()
        {
            Dictionary<string, MethodInfo> dict = new Dictionary<string, MethodInfo>();
            foreach (var method in Controller.GetType().GetMethods())
            {
                var actionAttr = method.GetCustomAttribute<WebSocketActionAttribute>();
                if (actionAttr == null)
                    continue;
                var action = (actionAttr.Action ?? method.Name).ToLowerInvariant();
                if (dict.ContainsKey(action))
                    throw new Exception("Multiple actions with the same name are not supported");
                dict[action] = method;
            }
            return dict;
        }

        public async Task Invoke(HttpContext ctx)
        {
            if (!ctx.WebSockets.IsWebSocketRequest)
            {
                await Next(ctx);
                return;
            }

            Controller = (WebSocketController)ctx.RequestServices.GetService(ControllerType);
            ActionMap = GenerateActionMap();

            var client = new WebSocketClient(await ctx.WebSockets.AcceptWebSocketAsync());
            if (!Controller.Clients.TryAdd(client.Id, client))
            {
                throw new Exception("This shouldn't happen");
            }

            int bufSize = 4096;
            var textBuffer = new ArraySegment<byte>(new byte[bufSize]);
            var textStream = new MemoryStream(bufSize);

            // Read first message
            var result = await client.Socket.ReceiveAsync(textBuffer, CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var binaryStream = new WebSocketBinaryStream(client, new ArraySegment<byte>(textBuffer.Array, textBuffer.Offset, result.Count), result);
                    try
                    {
                        await Controller.OnBinaryMessage(binaryStream);
                    }
                    catch (Exception ex)
                    {
                    }

                    // Read rest of message if Controller didn't fully exhaust binaryStream.
                    result = binaryStream.Result;
                    while (!result.EndOfMessage)
                    {
                        result = await client.Socket.ReceiveAsync(textBuffer, CancellationToken.None);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    textStream.Write(textBuffer.Array, 0, result.Count);

                    // Read the message until the end
                    while (!result.EndOfMessage)
                    {
                        result = await client.Socket.ReceiveAsync(textBuffer, CancellationToken.None);
                        textStream.Write(textBuffer.Array, 0, result.Count);
                    }

                    // Deserialize and seek to the beginning of the stream.
                    // Reset the stream length, as it is assumed that bufSize should be set to a sane value for your application.
                    // If messages are frequently oversized this will result in increased memory thrashing.
                    var jsonMessage = Encoding.UTF8.GetString(textStream.GetBuffer(), 0, (int)textStream.Position);
                    if (textStream.Length > bufSize)
                        textStream.SetLength(bufSize);
                    textStream.Position = 0;

                    WebSocketMessageRequest message = null;
                    try
                    {
                        message = JsonConvert.DeserializeObject<WebSocketMessageRequest>(jsonMessage);
                    }
                    catch (Exception ex)
                    {
                        SendResponse(client, new WebSocketMessageResponse
                        {
                            Id = null,
                            Success = false,
                            HasValue = false,
                            Value = ex.ToString()
                        });
                    }

                    if (message != null)
                    {
                        if (!ActionMap.TryGetValue(message.Action.ToLowerInvariant(), out MethodInfo method))
                        {
                            SendResponse(client, new WebSocketMessageResponse
                            {
                                Id = message.Id,
                                Success = false,
                                HasValue = false,
                                Value = "No such action"
                            });
                        }
                        else
                        {
                            var ignore = InvokeActionAndRespondAsync(client, method, message);
                        }
                    }
                }

                // Read next message
                result = await client.Socket.ReceiveAsync(textBuffer, CancellationToken.None);
            }
            if (!Controller.Clients.TryRemove(client.Id, out client))
            {
                throw new Exception("This shouldn't happen");
            }
            await client.Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task InvokeActionAndRespondAsync(WebSocketClient client, MethodInfo method, WebSocketMessageRequest message)
        {
            var response = new WebSocketMessageResponse
            {
                Id = message.Id,
                Success = true,
                HasValue = false
            };

            try
            {
                var result = method.Invoke(Controller, message.Args);
                if (method.ReturnType == typeof(Task))
                {
                    // async Task Action(...)
                    await (Task)result;
                }
                else if (method.ReturnType != typeof(void))
                {
                    if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        // async Task<T> Action(...)
                        await (Task)result;
                        result = result.GetType().GetProperty("Result").GetValue(result);
                    }
                    // Else T Action(...), so nothing needs to be done

                    response.HasValue = true;
                    response.Value = result;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.HasValue = true;
                response.Value = ex.ToString();
            }

            SendResponse(client, response);
        }

        private async void SendResponse(WebSocketClient client, WebSocketMessageResponse response)
        {
            var jsonResponse = JsonConvert.SerializeObject(response);
            await client.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonResponse)), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}