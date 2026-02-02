// Filters/AuthorizeSessionAttribute.cs
// SECURITY: Custom authorization filter for session-based auth
// RISK: Without this, unauthorized users can access protected pages

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LuxeSmilesDental.Filters
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeSessionAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                var userRole = context.HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}