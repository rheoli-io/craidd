
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Craidd.Auth
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer)) {
                return Task.CompletedTask;
            }

            var scopes = context.User.FindAll(c => c.Type == "scope" && c.Issuer == requirement.Issuer).ToList();

            // // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s.Value == requirement.Scope)) {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}