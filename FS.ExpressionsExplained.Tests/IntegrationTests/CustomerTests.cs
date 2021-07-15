using FluentAssertions;
using FS.ExpressionsExplained.Tests.Services;
using FS.ExpressionsExplained.WebApi.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace FS.ExpressionsExplained.Tests.IntegrationTests
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public async Task WhenCustomerIsReceived_ItMatchesExpectedResult()
        {
            // Prepare
            await using var testHost = await TestHost.Create();
            using var client = testHost.GetTestClient();

            // Act
            var getCustomer = testHost.GetRoute((CustomerController x) => x.GetCustomer(2 + 3));
            var customer = await client.GetStringAsync(getCustomer);

            // Check
            customer.Should().Be("Customer with ID 5");
        }
    }
}
