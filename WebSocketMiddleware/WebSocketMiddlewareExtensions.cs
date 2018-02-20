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
            return services.AddWebSocketController(typeof(T));
        }

        public static IServiceCollection AddWebSocketController(this IServiceCollection services, Type controller)
        {
            if (controller == null)
                throw new ArgumentException("Must not be null", nameof(controller));
            if (controller == null || controller.IsAbstract)
                throw new ArgumentException($"WebSocketController must not be abstract, but {controller.FullName} is abstract");
            services.AddSingleton(controller);
            return services;
        }

        public static IApplicationBuilder MapWebSocketControllers(this IApplicationBuilder app, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetEntryAssembly();
            foreach (var type in assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(WebSocketController))))
            {
                var controllerName = type.Name;
                if (controllerName.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase))
                    controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

                var routeAttr = type.GetCustomAttribute<WebSocketRouteAttribute>();
                var route = routeAttr?.Route;
                if (route == null)
                {
                    route = $"/{controllerName}";
                } else
                {
                    route = route.Replace("[controller]", controllerName, StringComparison.InvariantCultureIgnoreCase);
                }
                app.MapWebSocketController(route, type);
            }

            return app;
        }

        public static IApplicationBuilder MapWebSocketController<T>(this IApplicationBuilder app, PathString path) where T : WebSocketController
        {
            return app.MapWebSocketController(path, typeof(T));
        }

        public static IApplicationBuilder MapWebSocketController(this IApplicationBuilder app, PathString path, Type controller)
        {
            if (controller == null)
                throw new ArgumentException("Must not be null", nameof(controller));
            if (controller == null || controller.IsAbstract)
                throw new ArgumentException($"WebSocketController must not be abstract, but {controller.FullName} is abstract");
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketMiddleware>(controller));
        }
    }
}
