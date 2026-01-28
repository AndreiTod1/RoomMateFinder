# RoomMate Finder

Web application to find roommates and shared housing.

## What it does

A full-stack application that helps users find compatible roommates and available rooms. Users can create listings, browse available options, chat with potential matches, and leave reviews.

## Features

- **User Authentication** - JWT-based login and registration with profile pictures
- **Room Listings** - Create, edit, and submit listings for available rooms
- **Discover** - Browse listings and find compatible roommates
- **Matching System** - Find and view your matches
- **Real-time Chat** - Conversations with potential roommates via SignalR
- **Reviews** - Leave and view reviews for users
- **Admin Panel** - Manage users, listings, and pending approvals

## Tech Stack

### Backend (ASP.NET Core)

- .NET 9
- Entity Framework Core
- PostgreSQL
- MediatR (CQRS pattern)
- FluentValidation
- SignalR (real-time chat)
- JWT Authentication
- Swagger/OpenAPI

### Frontend (Blazor)

- Blazor WebAssembly
- Component-based UI

## Project Structure

```
RoomMate-Finder/         # Backend API
├── Features/            # CQRS handlers (Admins, Conversations, Matching, Profiles, Reviews, RoomListings, Roommates)
├── Entities/            # Database models
├── Hubs/                # SignalR hubs for real-time chat
├── Infrastructure/      # Database context, repositories
└── Validators/          # FluentValidation validators

RoomMate-Finder_Frontend/  # Blazor frontend
├── Pages/               # Razor pages (Login, Register, Listings, Profile, Conversations, etc.)
├── Services/            # API communication services
└── Shared/              # Shared components
```

## Running

Backend:

```bash
cd RoomMate-Finder
dotnet run
```

Frontend:

```bash
cd RoomMate-Finder_Frontend
dotnet run
```

API docs available at `/swagger`

## Testing

```bash
dotnet test
```

Coverage reports are generated in the `CoverageReport` folder.
