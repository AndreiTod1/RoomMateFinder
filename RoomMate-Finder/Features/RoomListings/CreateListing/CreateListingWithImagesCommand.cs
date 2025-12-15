using MediatR;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public record CreateListingWithImagesCommand(
    CreateListingRequest Listing,
    List<IFormFile>? Images
) : IRequest<CreateListingResponse>;
