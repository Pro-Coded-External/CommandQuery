using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Tracing;
using CommandQuery.AspNet.WebApi;
using CommandQuery.DependencyInjection;
using CommandQuery.Sample.AspNet.WebApi.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Sample.AspNet.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // IoC
            var services = new ServiceCollection();

            services.AddCommands(typeof(WebApiConfig).Assembly);
            services.AddQueries(typeof(WebApiConfig).Assembly);

            services.AddTransient<ICultureService, CultureService>();
            services.AddTransient<IDateTimeProxy, DateTimeProxy>();

            services.AddTransient<ITraceWriter>(_ => config.EnableSystemDiagnosticsTracing()); // Logging

            config.DependencyResolver = new CommandQueryDependencyResolver(services);

            // Web API routes
            config.MapHttpAttributeRoutes(new CommandQueryDirectRouteProvider());

            // Json
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
        }
    }
}
