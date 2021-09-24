using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asp_core_observability.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DependencyController : ControllerBase
    {
        private readonly ILogger<DependencyController> _logger;

        public DependencyController(
            ILogger<DependencyController> logger
            )
        {
            _logger = logger;
        }


    }
}
