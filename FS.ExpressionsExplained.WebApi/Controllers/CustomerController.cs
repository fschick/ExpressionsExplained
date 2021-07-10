using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace FS.ExpressionsExplained.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        [HttpGet("GetCustomer/{id}")]
        public string GetCustomer(int id)
            => $"Customer with ID {id}";

        [HttpPut("GenerateCustomer")]
        public void GenerateCustomer()
            => Expression.Empty();
    }
}
