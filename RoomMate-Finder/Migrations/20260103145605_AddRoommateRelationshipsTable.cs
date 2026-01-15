using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class AddRoommateRelationshipsTable : Migration
    {
        private static readonly string[] UserRelationshipColumns = { "User1Id", "User2Id" };
        private static readonly string[] RequestColumns = { "RequesterId", "TargetUserId" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roommate_requests",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedByAdminId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roommate_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_roommate_requests_profiles_ProcessedByAdminId",
                        column: x => x.ProcessedByAdminId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_roommate_requests_profiles_RequesterId",
                        column: x => x.RequesterId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roommate_requests_profiles_TargetUserId",
                        column: x => x.TargetUserId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roommate_relationships",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roommate_relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_roommate_relationships_profiles_ApprovedByAdminId",
                        column: x => x.ApprovedByAdminId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roommate_relationships_profiles_User1Id",
                        column: x => x.User1Id,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roommate_relationships_profiles_User2Id",
                        column: x => x.User2Id,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roommate_relationships_roommate_requests_OriginalRequestId",
                        column: x => x.OriginalRequestId,
                        principalSchema: "public",
                        principalTable: "roommate_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_roommate_relationships_ApprovedByAdminId",
                schema: "public",
                table: "roommate_relationships",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_roommate_relationships_OriginalRequestId",
                schema: "public",
                table: "roommate_relationships",
                column: "OriginalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_roommate_relationships_User1Id_User2Id",
                schema: "public",
                table: "roommate_relationships",
                columns: UserRelationshipColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roommate_relationships_User2Id",
                schema: "public",
                table: "roommate_relationships",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_roommate_requests_ProcessedByAdminId",
                schema: "public",
                table: "roommate_requests",
                column: "ProcessedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_roommate_requests_RequesterId_TargetUserId",
                schema: "public",
                table: "roommate_requests",
                columns: RequestColumns );

            migrationBuilder.CreateIndex(
                name: "IX_roommate_requests_TargetUserId",
                schema: "public",
                table: "roommate_requests",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "roommate_relationships",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roommate_requests",
                schema: "public");
        }
    }
}
