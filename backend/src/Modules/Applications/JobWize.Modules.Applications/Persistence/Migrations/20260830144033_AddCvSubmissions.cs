using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCvSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobApplicationCvSubmissions",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CompanyContactId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactRoleTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ContactPhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationCvSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationCvSubmissions_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalSchema: "applications",
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationCvSubmissionDocuments",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationCvSubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationCvSubmissionDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationCvSubmissionDocuments_JobApplicationCvSubmiss~",
                        column: x => x.JobApplicationCvSubmissionId,
                        principalSchema: "applications",
                        principalTable: "JobApplicationCvSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationCvSubmissionDocuments_JobApplicationCvSubmiss~",
                schema: "applications",
                table: "JobApplicationCvSubmissionDocuments",
                column: "JobApplicationCvSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationCvSubmissions_JobApplicationId",
                schema: "applications",
                table: "JobApplicationCvSubmissions",
                column: "JobApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobApplicationCvSubmissionDocuments",
                schema: "applications");

            migrationBuilder.DropTable(
                name: "JobApplicationCvSubmissions",
                schema: "applications");
        }
    }
}
