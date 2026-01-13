using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class SendMessage : Migration
    {
        private const string SchemaName = "public";
        private const string MessagesTable = "messages";
        private const string ConversationsTable = "conversations";
        private const string ProfilesTable = "profiles";
        private const string UuidType = "uuid";
        private const string TextType = "text";
        private const string TimestampType = "timestamp with time zone";
        private const string BooleanType = "boolean";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: MessagesTable,
                schema: SchemaName,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: UuidType, nullable: false),
                    ConversationId = table.Column<Guid>(type: UuidType, nullable: false),
                    SenderId = table.Column<Guid>(type: UuidType, nullable: false),
                    Content = table.Column<string>(type: TextType, nullable: false),
                    SentAt = table.Column<DateTime>(type: TimestampType, nullable: false),
                    IsRead = table.Column<bool>(type: BooleanType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_messages_conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalSchema: SchemaName,
                        principalTable: ConversationsTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_profiles_SenderId",
                        column: x => x.SenderId,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_messages_ConversationId",
                schema: SchemaName,
                table: MessagesTable,
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_SenderId",
                schema: SchemaName,
                table: MessagesTable,
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_SentAt",
                schema: SchemaName,
                table: MessagesTable,
                column: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: MessagesTable,
                schema: SchemaName);
        }
    }
}
