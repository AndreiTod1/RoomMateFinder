using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class FixRoomListingRelationship : Migration
    {
        private const string SchemaName = "public";
        private const string RoomListingsTable = "room_listings";
        private const string ReviewsTable = "reviews";
        private const string TextType = "text";
        private const string VarChar100Type = "character varying(100)";
        private const string VarChar2000Type = "character varying(2000)";
        private const string VarChar50Type = "character varying(50)";
        private const string VarChar500Type = "character varying(500)";
        private const string VarChar1000Type = "character varying(1000)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: SchemaName,
                table: RoomListingsTable,
                type: VarChar100Type,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: TextType);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: SchemaName,
                table: RoomListingsTable,
                type: VarChar2000Type,
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: TextType);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                schema: SchemaName,
                table: RoomListingsTable,
                type: VarChar50Type,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: TextType);

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                schema: SchemaName,
                table: RoomListingsTable,
                type: VarChar100Type,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: TextType);

            migrationBuilder.AlterColumn<string>(
                name: "Amenities",
                schema: SchemaName,
                table: RoomListingsTable,
                type: VarChar500Type,
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: TextType);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                schema: SchemaName,
                table: ReviewsTable,
                type: VarChar1000Type,
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: TextType);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: SchemaName,
                table: RoomListingsTable,
                type: TextType,
                nullable: false,
                oldClrType: typeof(string),
                oldType: VarChar100Type,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: SchemaName,
                table: RoomListingsTable,
                type: TextType,
                nullable: false,
                oldClrType: typeof(string),
                oldType: VarChar2000Type,
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                schema: SchemaName,
                table: RoomListingsTable,
                type: TextType,
                nullable: false,
                oldClrType: typeof(string),
                oldType: VarChar50Type,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Area",
                schema: SchemaName,
                table: RoomListingsTable,
                type: TextType,
                nullable: false,
                oldClrType: typeof(string),
                oldType: VarChar100Type,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Amenities",
                schema: SchemaName,
                table: RoomListingsTable,
                type: TextType,
                nullable: false,
                oldClrType: typeof(string),
                oldType: VarChar500Type,
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                schema: SchemaName,
                table: ReviewsTable,
                type: TextType,
                nullable: false,
                oldClrType: typeof(string),
                oldType: VarChar1000Type,
                oldMaxLength: 1000);
        }
    }
}
