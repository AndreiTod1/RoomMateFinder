using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoomMate_Finder.Features.Profiles.CreateProfile;
using RoomMate_Finder.Features.Profiles.Login;

namespace RoomMate_Finder.Features.Profiles;

public static class CreateProfileEndpoint 
{
    public static IEndpointRouteBuilder MapCreateProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles", async ([FromForm] CreateProfileForm form, IMediator mediator) =>
            {
                try 
                {
                    var cmd = new CreateProfileWithFileCommand(
                        new CreateProfileRequest(
                            form.Email,
                            form.Password,
                            form.FullName,
                            form.Bio,
                            form.Age,
                            form.Gender,
                            form.University,
                            form.Lifestyle,
                            form.Interests
                        ),
                        form.ProfilePicture
                    );

                    var response = await mediator.Send(cmd);
                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .DisableAntiforgery() // Disable antiforgery for API called from Blazor WASM
            .WithTags("Authentication")
            .WithName("CreateProfile")
            .WithSummary("Creates a new user profile")
            .Accepts<CreateProfileForm>("multipart/form-data")
            .Produces<AuthResponse>(200)
            .ProducesProblem(400);
            
        return app;
    }
}