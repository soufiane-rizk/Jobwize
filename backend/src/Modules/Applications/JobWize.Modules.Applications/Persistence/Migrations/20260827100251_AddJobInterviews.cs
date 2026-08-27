using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobInterviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobInterviews",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Format = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PreparationNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobInterviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobInterviews_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalSchema: "applications",
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobInterviewParticipants",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RoleTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobInterviewParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobInterviewParticipants_JobInterviews_JobInterviewId",
                        column: x => x.JobInterviewId,
                        principalSchema: "applications",
                        principalTable: "JobInterviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobInterviewParticipants_JobInterviewId",
                schema: "applications",
                table: "JobInterviewParticipants",
                column: "JobInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_JobInterviews_JobApplicationId",
                schema: "applications",
                table: "JobInterviews",
                column: "JobApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobInterviewParticipants",
                schema: "applications");

            migrationBuilder.DropTable(
                name: "JobInterviews",
                schema: "applications");
        }
    }
}
