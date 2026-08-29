using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Companies.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyChildModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyLocations_CompanyId",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByCandidateId",
                schema: "companies",
                table: "CompanyLocations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "companies",
                table: "CompanyLocations",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                schema: "companies",
                table: "CompanyLocations",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                schema: "companies",
                table: "CompanyLocations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                schema: "companies",
                table: "CompanyLocations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                schema: "companies",
                table: "CompanyLocations",
                type: "text",
                nullable: false,
                defaultValue: "Shared");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "companies",
                table: "CompanyContacts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLocations_CompanyId_Visibility_CreatedByCandidateId_~",
                schema: "companies",
                table: "CompanyLocations",
                columns: new[] { "CompanyId", "Visibility", "CreatedByCandidateId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyLocations_CompanyId_Visibility_CreatedByCandidateId_~",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.DropColumn(
                name: "CreatedByCandidateId",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.DropColumn(
                name: "ReviewReason",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.DropColumn(
                name: "Visibility",
                schema: "companies",
                table: "CompanyLocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "companies",
                table: "CompanyContacts");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLocations_CompanyId",
                schema: "companies",
                table: "CompanyLocations",
                column: "CompanyId");
        }
    }
}
