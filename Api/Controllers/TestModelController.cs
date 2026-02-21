using Microsoft.AspNetCore.Mvc;
using Data.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestModelController : ControllerBase
    {

        private readonly ILogger<TestModelController> _logger;

        public TestModelController(ILogger<TestModelController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetModel")]
        public IEnumerable<Model1> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new Model1
            {
                Id = new Guid(),
                Name = "Test",
                Model2s = new List<Model2>()
            })
            .ToArray();
        }
    }
}
