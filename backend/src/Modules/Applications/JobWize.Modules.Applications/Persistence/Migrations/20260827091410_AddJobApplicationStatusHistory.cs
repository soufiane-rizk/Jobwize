using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobApplicationStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobApplicationStatusChanges",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobApplicationStatusChanges",
                schema: "applications");
        }
    }
}
