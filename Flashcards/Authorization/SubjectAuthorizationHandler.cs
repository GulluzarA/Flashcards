using Flashcards.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Flashcards.Authorization;

public class SubjectAuthorizationHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, Subject>
{
    private readonly UserManager<IdentityUser> _userManager;

    public SubjectAuthorizationHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Subject resource)
    {
        // switch statement based on the requirement name
        switch (requirement.Name)
        {
            case nameof(Operations.Read):
                if (_userManager.GetUserId(context.User) == resource.OwnerId
                    || resource.Visibility == SubjectVisibility.Public)
                {
                    context.Succeed(requirement);
                }

                break;
            default: // Create, Update, Delete
                if (_userManager.GetUserId(context.User) == resource.OwnerId)
                {
                    context.Succeed(requirement);
                }

                break;
        }

        return Task.CompletedTask;
    }
}