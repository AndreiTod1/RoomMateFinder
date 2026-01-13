using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    public partial class AddUserActionsAndMatches_Retry : Migration
    {
        private const string SchemaName = "public";
        private const string UserActionsTable = "user_actions";
        private const string MatchesTable = "matches";
        private const string ProfilesTable = "profiles";
        private const string UuidType = "uuid";
        private const string TimestampType = "timestamp with time zone";
        private const string IntegerType = "integer";
        private const string BooleanType = "boolean";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: UserActionsTable,
                schema: SchemaName,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: UuidType, nullable: false),
                    UserId = table.Column<Guid>(type: UuidType, nullable: false),
                    TargetUserId = table.Column<Guid>(type: UuidType, nullable: false),
                    ActionType = table.Column<int>(type: IntegerType, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_actions_profiles_TargetUserId",
                        column: x => x.TargetUserId,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_actions_profiles_UserId",
                        column: x => x.UserId,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: MatchesTable,
                schema: SchemaName,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: UuidType, nullable: false),
                    User1Id = table.Column<Guid>(type: UuidType, nullable: false),
                    User2Id = table.Column<Guid>(type: UuidType, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: TimestampType, nullable: false),
                    IsActive = table.Column<bool>(type: BooleanType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matches_profiles_User1Id",
                        column: x => x.User1Id,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_matches_profiles_User2Id",
                        column: x => x.User2Id,
                        principalSchema: SchemaName,
                        principalTable: ProfilesTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matches_User1Id_User2Id",
                schema: SchemaName,
                table: MatchesTable,
                columns: new[] { "User1Id", "User2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_matches_User2Id",
                schema: SchemaName,
                table: MatchesTable,
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_actions_TargetUserId",
                schema: SchemaName,
                table: UserActionsTable,
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_actions_UserId_TargetUserId",
                schema: SchemaName,
                table: UserActionsTable,
                columns: new[] { "UserId", "TargetUserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: MatchesTable,
                schema: SchemaName);

            migrationBuilder.DropTable(
                name: UserActionsTable,
                schema: SchemaName);
        }
    }
}
