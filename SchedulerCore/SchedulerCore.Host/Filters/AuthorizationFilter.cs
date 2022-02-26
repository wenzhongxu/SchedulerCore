using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using SchedulerCore.Host.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Filters
{
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                var noNeedLoginAttribute = controllerActionDescriptor
                    .ControllerTypeInfo
                    .GetCustomAttributes(true)
                    .Where(p => p.GetType().Equals(typeof(NoLoginAttribute)))
                    .ToList();
                noNeedLoginAttribute.AddRange(
                    controllerActionDescriptor
                    .MethodInfo
                    .GetCustomAttributes(inherit: true)
                    .Where(p => p.GetType().Equals(typeof(NoLoginAttribute)))
                    .ToList());
                if (noNeedLoginAttribute.Any())
                {
                    return Task.CompletedTask;
                }
            }

            //ToDo：需要用户校验
            return Task.CompletedTask;
        }
    }
}
