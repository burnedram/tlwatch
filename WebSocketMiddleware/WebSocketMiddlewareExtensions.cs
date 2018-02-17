using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace WebSocketMiddleware
{
    public static class WebSocketMiddlewareExtensions
    {
        public static IServiceCollection AddWebSocketControllers(this IServiceCollection services, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetEntryAssembly();
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(WebSocketController))))
            {
                services.AddSingleton(type);
            }

            return services;
        }

        public static IServiceCollection AddWebSocketController<T>(this IServiceCollection services) where T : WebSocketController
        {
            var t = typeof(T);
            if (t.IsAbstract)
                throw new ArgumentException($"WebSocketController must not be abstract, but {t.FullName} is abstract");
            services.AddSingleton<T>();
            return services;
        }

        public static IApplicationBuilder MapWebSocketController(this IApplicationBuilder app, PathString path, WebSocketController controller)
        {
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketMiddleware>(controller));
        }
    }
}
