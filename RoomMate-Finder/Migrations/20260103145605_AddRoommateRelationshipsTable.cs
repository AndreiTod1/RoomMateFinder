using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomMate_Finder.Migrations
{
    /// <inheritdoc />
    public partial class AddRoommateRelationshipsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if table exists before creating
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'roommate_requests') THEN
                        CREATE TABLE public.roommate_requests (
                            ""Id"" uuid NOT NULL,
                            ""RequesterId"" uuid NOT NULL,
                            ""TargetUserId"" uuid NOT NULL,
                            ""Status"" integer NOT NULL,
                            ""Message"" text,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""ProcessedAt"" timestamp with time zone,
                            ""ProcessedByAdminId"" uuid,
                            CONSTRAINT ""PK_roommate_requests"" PRIMARY KEY (""Id""),
                            CONSTRAINT ""FK_roommate_requests_profiles_ProcessedByAdminId"" FOREIGN KEY (""ProcessedByAdminId"") REFERENCES public.profiles (""Id"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_roommate_requests_profiles_RequesterId"" FOREIGN KEY (""RequesterId"") REFERENCES public.profiles (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_roommate_requests_profiles_TargetUserId"" FOREIGN KEY (""TargetUserId"") REFERENCES public.profiles (""Id"") ON DELETE CASCADE
                        );
                    END IF;
                END $$;
            ");

            // Check if table exists before creating
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'roommate_relationships') THEN
                        CREATE TABLE public.roommate_relationships (
                            ""Id"" uuid NOT NULL,
                            ""User1Id"" uuid NOT NULL,
                            ""User2Id"" uuid NOT NULL,
                            ""ApprovedByAdminId"" uuid NOT NULL,
                            ""OriginalRequestId"" uuid,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""IsActive"" boolean NOT NULL,
                            CONSTRAINT ""PK_roommate_relationships"" PRIMARY KEY (""Id""),
                            CONSTRAINT ""FK_roommate_relationships_profiles_ApprovedByAdminId"" FOREIGN KEY (""ApprovedByAdminId"") REFERENCES public.profiles (""Id"") ON DELETE RESTRICT,
                            CONSTRAINT ""FK_roommate_relationships_profiles_User1Id"" FOREIGN KEY (""User1Id"") REFERENCES public.profiles (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_roommate_relationships_profiles_User2Id"" FOREIGN KEY (""User2Id"") REFERENCES public.profiles (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_roommate_relationships_roommate_requests_OriginalRequestId"" FOREIGN KEY (""OriginalRequestId"") REFERENCES public.roommate_requests (""Id"") ON DELETE SET NULL
                        );
                    END IF;
                END $$;
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_roommate_relationships_ApprovedByAdminId"" ON public.roommate_relationships (""ApprovedByAdminId"");
                CREATE INDEX IF NOT EXISTS ""IX_roommate_relationships_OriginalRequestId"" ON public.roommate_relationships (""OriginalRequestId"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_roommate_relationships_User1Id_User2Id"" ON public.roommate_relationships (""User1Id"", ""User2Id"");
                CREATE INDEX IF NOT EXISTS ""IX_roommate_relationships_User2Id"" ON public.roommate_relationships (""User2Id"");
                CREATE INDEX IF NOT EXISTS ""IX_roommate_requests_ProcessedByAdminId"" ON public.roommate_requests (""ProcessedByAdminId"");
                CREATE INDEX IF NOT EXISTS ""IX_roommate_requests_RequesterId_TargetUserId"" ON public.roommate_requests (""RequesterId"", ""TargetUserId"");
                CREATE INDEX IF NOT EXISTS ""IX_roommate_requests_TargetUserId"" ON public.roommate_requests (""TargetUserId"");
            ");
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
