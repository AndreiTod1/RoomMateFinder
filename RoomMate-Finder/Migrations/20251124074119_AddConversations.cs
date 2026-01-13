using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class AddConversations : Migration
    {
        private const string SchemaName = "public";
        private const string ConversationsTable = "conversations";
        private const string ProfilesTable = "profiles";
        private const string UuidType = "uuid";
        private const string TimestampType = "timestamp with time zone";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: ConversationsTable,
                schema: SchemaName,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: UuidType, nullable: false),
                    User1Id = table.Column<Guid>(type: UuidType, nullable: false),
                    User2Id = table.Column<Guid>(type: UuidType, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversations_profiles_User1Id",
                        column: x => x.User1Id,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conversations_profiles_User2Id",
                        column: x => x.User2Id,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversations_User1Id_User2Id",
                schema: SchemaName,
                table: ConversationsTable,
                columns: new[] { "User1Id", "User2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversations_User2Id",
                schema: SchemaName,
                table: ConversationsTable,
                column: "User2Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: ConversationsTable,
                schema: SchemaName);
        }
    }
}
