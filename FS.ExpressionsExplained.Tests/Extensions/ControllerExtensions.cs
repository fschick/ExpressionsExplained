using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace FS.ExpressionsExplained.Tests.Extensions
{
    public static class ControllerExtensions
    {
        public static string GetRoute<TController>(Expression<Action<TController>> controllerAction, IApiDescriptionGroupCollectionProvider apiDescriptionProvider)
        {
            if (controllerAction.Body is not MethodCallExpression controllerActionCall)
                throw new InvalidOperationException($"No route for {controllerAction} found.");

            var apiMethodDescription = apiDescriptionProvider.ApiDescriptionGroups.Items
                .SelectMany(x => x.Items)
                .FirstOrDefault(action =>
                    ImplementedByController<TController>(action) &&
                    HasMethodName(action, controllerActionCall.Method.Name) &&
                    MethodParametersMatch(action, controllerActionCall)
                );

            if (apiMethodDescription == null)
                throw new InvalidOperationException($"No route for {controllerAction} found.");

            var route = GetRelativePath(apiMethodDescription, controllerActionCall);
            return route;
        }

        private static bool ImplementedByController<TController>(this ApiDescription apiDescription)
            => (apiDescription.ActionDescriptor as ControllerActionDescriptor)?.ControllerTypeInfo == typeof(TController);

        private static bool HasMethodName(this ApiDescription apiDescription, string methodName)
            => (apiDescription.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.Name == methodName;

        private static bool MethodParametersMatch(this ApiDescription apiDescription, MethodCallExpression controllerActionCall)
        {
            var apiMethodParameterTypes = (apiDescription.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToList();
            if (apiMethodParameterTypes == null)
                return false;

            var controllerActionParameterTypes = controllerActionCall.Arguments.Select(parameter => parameter.Type).ToList();

            return apiMethodParameterTypes.SequenceEqual(controllerActionParameterTypes);
        }

        private static string GetRelativePath(this ApiDescription apiMethodDescription, MethodCallExpression controllerActionCall)
        {
            var route = apiMethodDescription.RelativePath;

            var controllerActionCallParameters = controllerActionCall.Method.GetParameters().ToList();
            foreach (var parameter in apiMethodDescription.ParameterDescriptions)
            {
                var parameterIndex = controllerActionCallParameters.FindIndex(x => x.Name == parameter.Name);
                var parameterExpression = controllerActionCall.Arguments[parameterIndex];
                try
                {
                    var parameterValue = Expression.Lambda(parameterExpression).Compile().DynamicInvoke();
                    route = Regex.Replace(route, $"{{{parameter.Name}(\\W.*?)?}}", parameterValue?.ToString() ?? string.Empty);
                }
                catch (Exception)
                {
                    throw new NotSupportedException($"Unable to evaluate parameter {parameter.Name} of {controllerActionCall}.");
                }
            }

            return route;
        }
    }
}
