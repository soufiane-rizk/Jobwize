using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStatusHistoryWithActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "JobApplicationStatusChanges",
                schema: "applications",
                newName: "JobApplicationActivities",
                newSchema: "applications");

            migrationBuilder.RenameIndex(
                name: "IX_JobApplicationStatusChanges_JobApplicationId",
                schema: "applications",
                table: "JobApplicationActivities",
                newName: "IX_JobApplicationActivities_JobApplicationId");

            migrationBuilder.RenameColumn(
                name: "ChangedAt",
                schema: "applications",
                table: "JobApplicationActivities",
                newName: "OccurredAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityAt",
                schema: "applications",
                table: "JobApplications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "applications",
                table: "JobApplicationActivities",
                type: "text",
                nullable: false,
                defaultValue: "StatusChanged");

            migrationBuilder.Sql("""
                INSERT INTO applications."JobApplicationActivities" ("Id", "JobApplicationId", "Type", "Status", "OccurredAt", "Note")
                SELECT application."Id", application."Id", 'StatusChanged', application."Status", application."CreatedAt", NULL
                FROM applications."JobApplications" application
                WHERE NOT EXISTS (
                    SELECT 1 FROM applications."JobApplicationActivities" activity
                    WHERE activity."JobApplicationId" = application."Id");
                """);

            migrationBuilder.Sql("""
                UPDATE applications."JobApplications" application
                SET "LastActivityAt" = COALESCE(
                    (SELECT MAX(activity."OccurredAt") FROM applications."JobApplicationActivities" activity
                     WHERE activity."JobApplicationId" = application."Id"),
                    application."CreatedAt");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobApplicationActivities",
                schema: "applications");

            migrationBuilder.DropColumn(
                name: "LastActivityAt",
                schema: "applications",
                table: "JobApplications");

            migrationBuilder.CreateTable(
                name: "JobApplicationStatusChanges",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationStatusChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationStatusChanges_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalSchema: "applications",
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStatusChanges_JobApplicationId",
                schema: "applications",
                table: "JobApplicationStatusChanges",
                column: "JobApplicationId");
        }
    }
}
