using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public static class CreateListingEndpoint
{
    public static IEndpointRouteBuilder MapCreateListingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/room-listings", async (
            [FromForm] CreateListingForm form, 
            HttpContext httpContext,
            ClaimsPrincipal user, 
            IMediator mediator) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
                {
                    return Results.Unauthorized();
                }

                // Check if user is admin
                var isAdmin = user.IsInRole("Admin");

                var amenities = form.Amenities
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                var request = new CreateListingRequest
                {
                    Title = form.Title,
                    Description = form.Description,
                    City = form.City,
                    Area = form.Area,
                    Price = form.Price,
                    AvailableFrom = form.AvailableFrom,
                    Amenities = amenities,
                    OwnerId = ownerId
                };

                // Get images directly from HttpContext like profile picture does
                var images = httpContext.Request.Form.Files.GetFiles("Images").ToList();
                Console.WriteLine($"[CreateListingEndpoint] Found {images.Count} files in form, IsAdmin: {isAdmin}");

                var command = new CreateListingWithImagesCommand(request, images, isAdmin);

                try
                {
                    var response = await mediator.Send(command);
                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .DisableAntiforgery()
            .RequireAuthorization()
            .WithTags("RoomListings")
            .WithName("CreateListing")
            .WithSummary("Create a new room listing with up to 8 images")
            .Accepts<CreateListingForm>("multipart/form-data")
            .Produces<CreateListingResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401);

        return app;
    }
}
