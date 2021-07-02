using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandQuery.Internal;
using CommandQuery.NewtonsoftJson;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace CommandQuery.AzureFunctions
{
    /// <summary>
    /// Handles queries for the Azure function.
    /// </summary>
    public class QueryFunction : IQueryFunction
    {
        private readonly IQueryProcessor _queryProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFunction"/> class.
        /// </summary>
        /// <param name="queryProcessor">An <see cref="IQueryProcessor"/>.</param>
        public QueryFunction(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        /// <summary>
        /// Handle a query.
        /// </summary>
        /// <param name="queryName">The name of the query.</param>
        /// <param name="req">A <see cref="HttpRequest"/>.</param>
        /// <param name="log">An <see cref="ILogger"/>.</param>
        /// <returns>The result + 200, 400 or 500.</returns>
        public async Task<IActionResult> HandleAsync(string queryName, HttpRequest req, ILogger log)
        {
            log.LogInformation($"Handle {queryName}");

            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            try
            {
                var result = req.Method == "GET"
                    ? await HandleAsync(queryName, Dictionary(req.Query)).ConfigureAwait(false)
                    : await HandleAsync(queryName, await req.ReadAsStringAsync().ConfigureAwait(false)).ConfigureAwait(false);

                return new OkObjectResult(result);
            }
            catch (Exception exception)
            {
                var payload = req.Method == "GET" ? (object)req.Query : await req.ReadAsStringAsync().ConfigureAwait(false);
                log.LogError(exception.GetQueryEventId(), exception, "Handle query failed: {QueryName}, {Payload}", queryName, payload);

                return exception.IsHandled() ? new BadRequestObjectResult(exception.ToError()) : new ObjectResult(exception.ToError()) { StatusCode = 500 };
            }

            Dictionary<string, IEnumerable<string>> Dictionary(IQueryCollection query)
            {
                return query.ToDictionary(kv => kv.Key, kv => kv.Value as IEnumerable<string>, StringComparer.OrdinalIgnoreCase);
            }
        }

        private async Task<object> HandleAsync(string queryName, string content)
        {
            return await _queryProcessor.ProcessAsync<object>(queryName, content).ConfigureAwait(false);
        }

        private async Task<object> HandleAsync(string queryName, IDictionary<string, IEnumerable<string>> query)
        {
            return await _queryProcessor.ProcessAsync<object>(queryName, query).ConfigureAwait(false);
        }
    }
}
