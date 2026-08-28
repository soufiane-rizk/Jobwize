using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyLinkAndProjections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                schema: "applications",
                table: "JobApplications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                schema: "applications",
                table: "JobApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyLocationId",
                schema: "applications",
                table: "JobApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanyProjections",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Visibility = table.Column<string>(type: "text", nullable: false),
                    CreatedByCandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProjections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyLocationProjections",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyLocationProjections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyLocationProjections_CompanyProjections_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "applications",
                        principalTable: "CompanyProjections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CompanyId",
                schema: "applications",
                table: "JobApplications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLocationProjections_CompanyId_IsActive",
                schema: "applications",
                table: "CompanyLocationProjections",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProjections_IsActive_Visibility_CreatedByCandidateId",
                schema: "applications",
                table: "CompanyProjections",
                columns: new[] { "IsActive", "Visibility", "CreatedByCandidateId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyLocationProjections",
                schema: "applications");

            migrationBuilder.DropTable(
                name: "CompanyProjections",
                schema: "applications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CompanyId",
                schema: "applications",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "applications",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CompanyLocationId",
                schema: "applications",
                table: "JobApplications");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                schema: "applications",
                table: "JobApplications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
