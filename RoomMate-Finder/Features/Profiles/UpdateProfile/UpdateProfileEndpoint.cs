using System.Security.Claims;
using FluentValidation;
using MediatR;

namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public static class UpdateProfileEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/profiles/{id:guid}", async (
            Guid id, 
            UpdateProfileRequest request, 
            IMediator mediator, 
            ClaimsPrincipal user,
            IValidator<UpdateProfileRequest> validator) =>
            {
                try
                {
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
                    
                    
                    request.UserId = id;
                    var response = await mediator.Send(request);
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
            .RequireAuthorization()
            .WithName("UpdateProfile")
            .WithSummary("Updates an existing user profile")
            .Produces<UpdateProfileResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);
            
        return app;
    }
}