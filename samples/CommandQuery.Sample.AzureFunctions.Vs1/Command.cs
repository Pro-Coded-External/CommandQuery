using System.Net.Http;
using System.Threading.Tasks;
using CommandQuery.AzureFunctions;
using CommandQuery.DependencyInjection;
using CommandQuery.Sample.Contracts.Commands;
using CommandQuery.Sample.Handlers;
using CommandQuery.Sample.Handlers.Commands;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Sample.AzureFunctions.Vs1
{
    public static class Command
    {
        private static readonly CommandFunction Func = new CommandFunction(GetServiceCollection().GetCommandProcessor(typeof(FooCommandHandler).Assembly, typeof(FooCommand).Assembly));

        [FunctionName("Command")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "command/{commandName}")] HttpRequestMessage req, TraceWriter log, string commandName)
        {
            return await Func.Handle(commandName, req, log);
        }

        private static IServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();
            // Add handler dependencies
            services.AddTransient<ICultureService, CultureService>();

            return services;
        }
    }
}