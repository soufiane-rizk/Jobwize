using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyLocationProjectionVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyLocationProjections_CompanyId_IsActive",
                schema: "applications",
                table: "CompanyLocationProjections");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByCandidateId",
                schema: "applications",
                table: "CompanyLocationProjections",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                schema: "applications",
                table: "CompanyLocationProjections",
                type: "text",
                nullable: false,
                defaultValue: "Shared");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLocationProjections_CompanyId_IsActive_Visibility_Cr~",
                schema: "applications",
                table: "CompanyLocationProjections",
                columns: new[] { "CompanyId", "IsActive", "Visibility", "CreatedByCandidateId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyLocationProjections_CompanyId_IsActive_Visibility_Cr~",
                schema: "applications",
                table: "CompanyLocationProjections");

            migrationBuilder.DropColumn(
                name: "CreatedByCandidateId",
                schema: "applications",
                table: "CompanyLocationProjections");

            migrationBuilder.DropColumn(
                name: "Visibility",
                schema: "applications",
                table: "CompanyLocationProjections");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLocationProjections_CompanyId_IsActive",
                schema: "applications",
                table: "CompanyLocationProjections",
                columns: new[] { "CompanyId", "IsActive" });
        }
    }
}
