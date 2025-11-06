using MediatR;

namespace RoomMate_Finder.Features.Profiles.Login;

public record LoginRequest(string Email, string Password) : IRequest<AuthResponse>;
