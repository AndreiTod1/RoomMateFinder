﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomListingsAndReviews : Migration
    {
        private const string SchemaName = "public";
        private const string RoomListingsTable = "room_listings";
        private const string ReviewsTable = "reviews";
        private const string ProfilesTable = "profiles";
        private const string TextType = "text";
        private const string UuidType = "uuid";
        private const string TimestampType = "timestamp with time zone";
        private const string NumericType = "numeric";
        private const string IntegerType = "integer";
        private const string BooleanType = "boolean";
        private const string VarChar1000Type = "character varying(1000)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: RoomListingsTable,
                schema: SchemaName,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: UuidType, nullable: false),
                    OwnerId = table.Column<Guid>(type: UuidType, nullable: false),
                    Title = table.Column<string>(type: TextType, nullable: false),
                    Description = table.Column<string>(type: TextType, nullable: false),
                    City = table.Column<string>(type: TextType, nullable: false),
                    Area = table.Column<string>(type: TextType, nullable: false),
                    Price = table.Column<decimal>(type: NumericType, nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: TimestampType, nullable: false),
                    Amenities = table.Column<string>(type: TextType, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampType, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: TimestampType, nullable: true),
                    IsActive = table.Column<bool>(type: BooleanType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_listings_profiles_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_City",
                schema: SchemaName,
                table: RoomListingsTable,
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_CreatedAt",
                schema: SchemaName,
                table: RoomListingsTable,
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_IsActive",
                schema: SchemaName,
                table: RoomListingsTable,
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_room_listings_OwnerId",
                schema: SchemaName,
                table: RoomListingsTable,
                column: "OwnerId");

            migrationBuilder.CreateTable(
                name: ReviewsTable,
                schema: SchemaName,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: UuidType, nullable: false),
                    ReviewerId = table.Column<Guid>(type: UuidType, nullable: false),
                    ReviewedUserId = table.Column<Guid>(type: UuidType, nullable: false),
                    Rating = table.Column<int>(type: IntegerType, nullable: false),
                    Comment = table.Column<string>(type: VarChar1000Type, maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reviews_profiles_ReviewerId",
                        column: x => x.ReviewerId,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_profiles_ReviewedUserId",
                        column: x => x.ReviewedUserId,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_ReviewedUserId",
                schema: SchemaName,
                table: ReviewsTable,
                column: "ReviewedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_ReviewerId_ReviewedUserId",
                schema: SchemaName,
                table: ReviewsTable,
                columns: new[] { "ReviewerId", "ReviewedUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP TABLE IF EXISTS {SchemaName}.{ReviewsTable};");
            migrationBuilder.Sql($"DROP TABLE IF EXISTS {SchemaName}.{RoomListingsTable};");
        }
    }
}
