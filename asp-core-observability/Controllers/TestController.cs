using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace asp_core_observability.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public TestController(
            ILogger<TestController> logger,
            IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }
        
        [HttpGet("inc")]
        public async Task<IActionResult> Inc()
        {
            // custom metrics
            var counter = Metrics.CreateCounter("customcounter1", "custom counter 1");
            counter.Inc();

            // structured logging
            int fizz = 3, buzz = 5;
            var obj = new { Name = "Obj", Timestamp = DateTime.Now };
            _logger.LogInformation("The current values are {Fizz} and {Buzz} and {Obj}", fizz, buzz, obj);

            await Task.Delay(200);
            return Ok();
        }

        private static ActivitySource source = new ActivitySource("Sample.DistributedTracing", "1.0.0");

        [HttpGet("trace")]
        public async Task<IActionResult> Trace()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://www.google.com");


            using (Activity activity = source.StartActivity("SomeWork"))
            {
                activity?.SetTag("foo", "foo");
                activity?.SetTag("bar", "bar");
                await Task.Delay(200);
            }

            return Ok(response.StatusCode);
        }


        [HttpGet("one")]
        public IActionResult One()
        {
            return Ok();
        }

        [HttpGet("two")]
        public IActionResult Two()
        {
            return Ok();
        }
    }
}
