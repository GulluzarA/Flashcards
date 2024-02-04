using Flashcards.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Flashcards.Authorization;

public class CardAuthorizationHandler :
    AuthorizationHandler<OperationAuthorizationRequirement, Card>
{
    private readonly UserManager<IdentityUser> _userManager;

    public CardAuthorizationHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Card resource)
    {
        // switch statement based on the requirement name
        switch (requirement.Name)
        {
            case nameof(Operations.Read):
                if (_userManager.GetUserId(context.User) == resource.Deck?.Subject?.OwnerId
                    || resource.Deck?.Subject?.Visibility == SubjectVisibility.Public)
                {
                    context.Succeed(requirement);
                }

                break;
            default: // Create, Update, Delete
                if (_userManager.GetUserId(context.User) == resource.Deck?.Subject?.OwnerId)
                {
                    context.Succeed(requirement);
                }

                break;
        }

        return Task.CompletedTask;
    }
}