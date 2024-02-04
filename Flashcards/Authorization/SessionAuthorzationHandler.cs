using Flashcards.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Flashcards.Authorization;

public class SessionAuthorizationHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, Session>
{
    private readonly UserManager<IdentityUser> _userManager;

    public SessionAuthorizationHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Session resource)
    {
        // switch statement based on the requirement name
        switch (requirement.Name)
        {
            default: // Read, Create, Update, Delete
                if (_userManager.GetUserId(context.User) == resource.UserId)
                {
                    context.Succeed(requirement);
                }

                break;
        }

        return Task.CompletedTask;
    }
}