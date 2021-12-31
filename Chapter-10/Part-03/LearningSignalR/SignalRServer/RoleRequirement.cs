using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SignalRServer
{
    public class RoleRequirement : AuthorizationHandler<RoleRequirement, HubInvocationContext>,
    IAuthorizationRequirement
    {
        private readonly string requiredRole;

        public RoleRequirement(string requiredRole)
        {
            this.requiredRole = requiredRole;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            RoleRequirement requirement,
            HubInvocationContext resource)
        {
            var roles = ((ClaimsIdentity)context.User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            if (roles.Contains(requiredRole))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
