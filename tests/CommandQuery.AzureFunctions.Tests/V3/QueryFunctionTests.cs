#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandQuery.Exceptions;
using CommandQuery.Tests;
using FluentAssertions;
using LoFuUnit.AutoMoq;
using LoFuUnit.NUnit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CommandQuery.AzureFunctions.Tests.V3
{
    public class QueryFunctionTests : LoFuTest<QueryFunction>
    {
        [SetUp]
        public void SetUp()
        {
            Clear();
            Use<Mock<IQueryProcessor>>();
            Use<JsonSerializerSettings>(null);
            Logger = Use<ILogger>();
            QueryName = "FakeQuery";
            The<Mock<IQueryProcessor>>().Setup(x => x.GetQueryType(QueryName)).Returns(typeof(FakeQuery));
        }

        [LoFu, Test]
        public async Task when_handling_the_query_via_Post()
        {
            Req = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"))
            };

            async Task should_return_the_result_from_the_query_processor()
            {
                var expected = new FakeResult();
                The<Mock<IQueryProcessor>>().Setup(x => x.ProcessAsync(It.IsAny<FakeQuery>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expected));

                var result = await Subject.HandleAsync(QueryName, Req, Logger) as ContentResult;

                result.StatusCode.Should().Be(200);
                result.Content.Should().NotBeNull();
            }

            async Task should_throw_when_request_is_null()
            {
                Subject.Awaiting(x => x.HandleAsync(QueryName, null, Logger))
                    .Should().Throw<ArgumentNullException>();
            }

            async Task should_handle_QueryProcessorException()
            {
                The<Mock<IQueryProcessor>>().Setup(x => x.ProcessAsync(It.IsAny<FakeQuery>(), It.IsAny<CancellationToken>())).Throws(new QueryProcessorException("fail"));

                var result = await Subject.HandleAsync(QueryName, Req, Logger);

                result.ShouldBeError("fail", 400);
            }

            async Task should_handle_QueryException()
            {
                The<Mock<IQueryProcessor>>().Setup(x => x.ProcessAsync(It.IsAny<FakeQuery>(), It.IsAny<CancellationToken>())).Throws(new QueryException("invalid"));

                var result = await Subject.HandleAsync(QueryName, Req, Logger);

                result.ShouldBeError("invalid", 400);
            }

            async Task should_handle_Exception()
            {
                The<Mock<IQueryProcessor>>().Setup(x => x.ProcessAsync(It.IsAny<FakeQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception("fail"));

                var result = await Subject.HandleAsync(QueryName, Req, Logger);

                result.ShouldBeError("fail", 500);
            }
        }

        [LoFu, Test]
        public async Task when_handling_the_query_via_Get()
        {
            Req = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET",
                Query = new QueryCollection(new Dictionary<string, StringValues> { { "foo", new StringValues("bar") } })
            };

            async Task should_return_the_result_from_the_query_processor()
            {
                var expected = new FakeResult();
                The<Mock<IQueryProcessor>>().Setup(x => x.ProcessAsync(It.IsAny<FakeQuery>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expected));

                var result = await Subject.HandleAsync(QueryName, Req, Logger) as ContentResult;

                result.StatusCode.Should().Be(200);
                result.Content.Should().NotBeNull();
            }
        }

        DefaultHttpRequest Req;
        ILogger Logger;
        string QueryName;
    }
}
#endif