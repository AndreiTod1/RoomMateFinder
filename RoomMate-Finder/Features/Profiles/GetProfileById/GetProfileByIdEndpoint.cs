using MediatR;

namespace RoomMate_Finder.Features.Profiles.GetProfileById;

public static class GetProfileByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetProfileByIdEndpoint(this IEndpointRouteBuilder app)
    {  
        app.MapGet("/profiles/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var query = new GetProfileByIdRequest(id);
                var response = await mediator.Send(query);
                
                if (response == null)
                {
                    return Results.NotFound(new { message = "Profile not found" });
                }
                
                return Results.Ok(response);
            })
            .WithTags("Profiles")
            .WithName("GetProfileById")
            .WithSummary("Retrieves a profile by its unique identifier")
            .Produces<GetProfileByIdResponse>(200)
            .ProducesProblem(404);

        return app;
    }
}