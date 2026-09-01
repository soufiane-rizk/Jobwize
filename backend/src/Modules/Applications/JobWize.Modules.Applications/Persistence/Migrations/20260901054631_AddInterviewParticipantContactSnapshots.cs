using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewParticipantContactSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyContactId",
                schema: "applications",
                table: "JobInterviewParticipants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyLocationId",
                schema: "applications",
                table: "JobInterviewParticipants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyLocationLabel",
                schema: "applications",
                table: "JobInterviewParticipants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "applications",
                table: "JobInterviewParticipants",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "applications",
                table: "JobInterviewParticipants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyContactId",
                schema: "applications",
                table: "JobInterviewParticipants");

            migrationBuilder.DropColumn(
                name: "CompanyLocationId",
                schema: "applications",
                table: "JobInterviewParticipants");

            migrationBuilder.DropColumn(
                name: "CompanyLocationLabel",
                schema: "applications",
                table: "JobInterviewParticipants");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "applications",
                table: "JobInterviewParticipants");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "applications",
                table: "JobInterviewParticipants");
        }
    }
}
