// ReSharper disable UnusedMember.Local
using FS.ExpressionsExplained.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FS.ExpressionsExplained.Client
{
    public class Program
    {
        public static async Task Main()
        {
            await InspectAnExpressionsTree();
            await InspectWebApiActionRoute();

            await CreateAnExpressionsTree();
            await CrateQueryableFilterExpression();
        }

        private static Task InspectAnExpressionsTree()
        {
            Console.WriteLine("------------------------");
            Console.WriteLine("InspectAnExpressionsTree");
            Console.WriteLine("------------------------");

            // Create an expression tree.  
            Expression<Func<int, bool>> addFiveLambda = value => value < 5;

            // Decompose the expression tree.  
            var param = (ParameterExpression)addFiveLambda.Parameters[0];
            var operation = (BinaryExpression)addFiveLambda.Body;
            var left = (ParameterExpression)operation.Left;
            var opertor = operation.NodeType;
            var right = (ConstantExpression)operation.Right;

            Console.WriteLine($"Expression: {param.Name} => {left.Name} {opertor} {right.Value}"); //Output:
            // Expression: value => value LessThan 5

            Console.WriteLine();
            return Task.CompletedTask;
        }

        private static async Task InspectWebApiActionRoute()
        {
            Console.WriteLine("------------------------");
            Console.WriteLine("InspectWebApiActionRoute");
            Console.WriteLine("------------------------");

            Expression<Action<CustomerController>> getCustomerLambda = x => x.GetCustomer(2 + 3);

            var lambdaParam = getCustomerLambda.Parameters[0];
            var getCustomerExpression = (MethodCallExpression)getCustomerLambda.Body;

            // WebAPI controller
            var controllerType = lambdaParam.Type;

            // Method name withing WebAPI controller
            var methodName = getCustomerExpression.Method.Name;

            // Parameter 'id' for method
            var methodParamExpr = getCustomerExpression.Arguments[0]; // 2 + 3
            var methodParamLambda = Expression.Lambda(methodParamExpr); // _ => 2 + 3
            var methodParamValue = methodParamLambda.Compile().DynamicInvoke(); // (_ => 2 + 3)()

            var apiActions = await GetWebApiActions();
            var getCustomerAction = apiActions
                .First(api =>
                   api.ActionDescriptor is ControllerActionDescriptor ctrl &&
                   ctrl.ControllerTypeInfo == controllerType &&
                   ctrl.MethodInfo.Name == methodName
                );

            var path = getCustomerAction.RelativePath;
            var parameterName = getCustomerAction.ParameterDescriptions[0].Name;
            var route = path.Replace($"{{{parameterName}}}", methodParamValue!.ToString());

            Console.WriteLine(route); //Output:
            // Customer/GetCustomer/5

            Console.WriteLine();
        }

        private static Task CreateAnExpressionsTree()
        {
            Console.WriteLine("------------------------");
            Console.WriteLine("CreateAnExpressionsTree");
            Console.WriteLine("------------------------");

            var value = Expression.Parameter(typeof(int), "value");
            var five = Expression.Constant(5, typeof(int));
            var valueAddFiveExpr = Expression.Add(value, five);

            var addFiveLambda = Expression.Lambda<Func<int, int>>(body: valueAddFiveExpr, parameters: value);
            Console.WriteLine(addFiveLambda); //Output:
            // value => (value + 5)

            Console.WriteLine();
            return Task.CompletedTask;
        }

        private static Task CrateQueryableFilterExpression()
        {
            Console.WriteLine("------------------------");
            Console.WriteLine("CrateQueryableFilterExpression");
            Console.WriteLine("------------------------");

            // Filter 'person => person.Age < 5' for 'persons = new[] { Person { Name = "Eve", Age = 4 } }'
            var filter = "<5";

            // Build filter
            var personParam = Expression.Parameter(typeof(Person), "person"); // person
            var personAgeSelector = Expression.Property(personParam, nameof(Person.Age)); // person.Age

            var filterOperator = filter[0]; // <
            var filterValueStr = filter[1..]; // 5
            var filterValue = int.Parse(filterValueStr);
            var filterValueExpr = Expression.Constant(filterValue, typeof(int)); // 5

            Expression personAgeFilterExpression = filterOperator switch
            {
                '<' => Expression.LessThan(personAgeSelector, filterValueExpr), // person.Age < 5
                '>' => Expression.GreaterThan(personAgeSelector, filterValueExpr), // person.Age > 5
                '=' => Expression.Equal(personAgeSelector, filterValueExpr), // person.Age == 5
                _ => throw new ArgumentOutOfRangeException()
            };

            var personAgeFilter = Expression.Lambda<Func<Person, bool>>(
                    body: personAgeFilterExpression,
                    parameters: personParam
                ); // person => person.Age < 5

            // Test filter
            var persons = new[] {
                new Person("Eve", 4),
                new Person("Joe", 5),
                new Person("Amy", 6)
            };

            var filteredPersons = persons.AsQueryable().Where(personAgeFilter).ToArray();
            //var filteredPersons = persons.Where(personAgeFilter.Compile()).ToArray();

            Console.WriteLine($"persons: {joinPersons(persons)}"); //Output:
            // persons: Person { Name = Eve, Age = 4 }, Person { Name = Joe, Age = 5 }, Person { Name = Amy, Age = 6 }
            Console.WriteLine($"filteredPersons: {joinPersons(filteredPersons)}"); //Output:
            // filteredPersons: Person { Name = Eve, Age = 4 }

            Console.WriteLine();
            return Task.CompletedTask;

            static string joinPersons(IEnumerable<Person> persons)
                => string.Join(", ", persons.Select(x => x.ToString()));
        }

        private static async Task<List<ApiDescription>> GetWebApiActions()
        {
            var testServer = await StartTestServer();
            var apiDescriptionProvider = testServer.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();
            var webApiActions = apiDescriptionProvider.ApiDescriptionGroups.Items.SelectMany(x => x.Items).ToList();
            await testServer.StopAsync();
            return webApiActions;
        }

        private static async Task<IHost> StartTestServer()
            => await WebApi.Program.CreateHostBuilder(null)
                .ConfigureWebHost(webHostBuilder => webHostBuilder.UseTestServer())
                .StartAsync();

        private record Person(string Name, int Age);
    }
}
