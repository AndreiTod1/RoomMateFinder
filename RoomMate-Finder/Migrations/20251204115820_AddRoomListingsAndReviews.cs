﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomListingsAndReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "room_listings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Area = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amenities = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_listings_profiles_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_City",
                schema: "public",
                table: "room_listings",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_CreatedAt",
                schema: "public",
                table: "room_listings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_IsActive",
                schema: "public",
                table: "room_listings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_OwnerId",
                schema: "public",
                table: "room_listings",
                column: "OwnerId");

            migrationBuilder.CreateTable(
                name: "reviews",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reviews_profiles_ReviewerId",
                        column: x => x.ReviewerId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_profiles_ReviewedUserId",
                        column: x => x.ReviewedUserId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_ReviewedUserId",
                schema: "public",
                table: "reviews",
                column: "ReviewedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_ReviewerId_ReviewedUserId",
                schema: "public",
                table: "reviews",
                columns: new[] { "ReviewerId", "ReviewedUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.reviews;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.room_listings;");
        }
    }
}
