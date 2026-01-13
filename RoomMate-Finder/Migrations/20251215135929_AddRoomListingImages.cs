using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomListingImages : Migration
    {
        private const string TableName = "room_listing_images";
        private const string SchemaName = "public";
        private const string PrincipalTableName = "room_listings";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: TableName,
                schema: SchemaName,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_listing_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_listing_images_room_listings_RoomListingId",
                        column: x => x.RoomListingId,
                        principalSchema: SchemaName,
                        principalTable: PrincipalTableName,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_room_listing_images_RoomListingId",
                schema: SchemaName,
                table: TableName,
                column: "RoomListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: TableName,
                schema: SchemaName);
        }
    }
}
