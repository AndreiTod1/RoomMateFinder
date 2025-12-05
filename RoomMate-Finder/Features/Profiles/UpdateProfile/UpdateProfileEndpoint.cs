using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public static class UpdateProfileEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/profiles/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            [FromForm] UpdateProfileForm form,
            IMediator mediator,
            IValidator<UpdateProfileRequest> validator) =>
            {
                try
                {
                    var request = new UpdateProfileRequest(
                        form.FullName,
                        form.Age,
                        form.Gender,
                        form.University,
                        form.Bio,
                        form.Lifestyle,
                        form.Interests
                    )
                    {
                        UserId = id
                    };

                    var validationResult = await validator.ValidateAsync(request);
                    if (!validationResult.IsValid)
                    {
                        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                        return Results.BadRequest(new { message = errors });
                    }

                    // Get the authenticated user's ID from JWT token
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authenticatedUserId))
                    {
                        return Results.Unauthorized();
                    }
                    
                    if (id != authenticatedUserId)
                    {
                        return Results.Forbid();
                    }

                    var cmd = new UpdateProfileWithFileCommand(request, form.ProfilePicture);
                    var response = await mediator.Send(cmd);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .DisableAntiforgery() // Disable antiforgery for API called from Blazor WASM
            .RequireAuthorization()
            .WithTags("Profiles")
            .WithName("UpdateProfile")
            .WithSummary("Updates an existing user profile")
            .Accepts<UpdateProfileForm>("multipart/form-data")
            .Produces<UpdateProfileResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);
            
        return app;
    }
}