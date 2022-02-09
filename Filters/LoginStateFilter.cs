using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
namespace TRS.Filters
{
    /// <summary>
    /// Action interceptor
    /// </summary>
    public class LoginStateFilter : ActionFilterAttribute, IActionFilter
    {
        #region  Execute this method after executing action
        /// <summary>
        ///  Execute this method after executing action
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }
        #endregion


        #region  Execute this method before executing action 
        /// <summary>
        ///  Execute this method before executing action
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            
            if(!filterContext.HttpContext.Request.Cookies.ContainsKey(TRS.Constants.USER_COOKIE))
            {// login expired: redirect to login screen
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary 
                { 
                    { "controller", "Login" }, 
                    { "action", "Index" } 
                });
            }
            else
            {
                Controller controller = filterContext.Controller as Controller;
                controller.ViewData["Username"] = filterContext.HttpContext.Request.Cookies[TRS.Constants.USER_COOKIE];
            }
        }
        #endregion

    }
}