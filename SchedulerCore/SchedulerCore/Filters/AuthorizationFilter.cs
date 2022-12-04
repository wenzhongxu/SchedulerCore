using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using SchedulerCore.Host.Attributes;
using SchedulerCore.Host.Common;
using System.Net;

namespace SchedulerCore.Host.Filters
{
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                var noNeedLoginAttribute = controllerActionDescriptor.
                                   ControllerTypeInfo.
                                   GetCustomAttributes(true)
                                   .Where(a => a.GetType().Equals(typeof(NoLoginAttribute)))
                                   .ToList();
                noNeedLoginAttribute.AddRange(controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
                                 .Where(a => a.GetType().Equals(typeof(NoLoginAttribute))).ToList());

                //如果标记了 NoLoginAttribute 则不验证其登录状态
                if (noNeedLoginAttribute.Any())
                {
                    return Task.CompletedTask;
                }
            }

            var token = context.HttpContext.Request.Headers["token"].ToString();
            if (!string.IsNullOrWhiteSpace(token))
            {
                if (DateTime.TryParse(token.DES3Decrypt(), out DateTime _dt))
                {
                    //登录信息有效期为当天
                    if (DateTime.Now.Date == _dt.Date)
                    {
                        return Task.CompletedTask;
                    }
                }
            }

            context.Result = new JsonResult(new
            {
                ErrorMsg = "请登录",
                ResultUrl = "/signin",
            });
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return Task.CompletedTask;
        }
    }
}
