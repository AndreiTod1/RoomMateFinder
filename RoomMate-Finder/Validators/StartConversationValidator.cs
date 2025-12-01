using FluentValidation;
using RoomMate_Finder.Features.Conversations.StartConversation;

namespace RoomMate_Finder.Validators;

public class StartConversationValidator : AbstractValidator<StartConversationRequest>
{
    public StartConversationValidator()
    {
        RuleFor(x => x.OtherUserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}

