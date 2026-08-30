using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyContactProjections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyContactProjections",
                schema: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RoleTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Visibility = table.Column<string>(type: "text", nullable: false),
                    CreatedByCandidateId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsRejected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyContactProjections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyContactProjections_CompanyProjections_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "applications",
                        principalTable: "CompanyProjections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyContactProjections_Selectability",
                schema: "applications",
                table: "CompanyContactProjections",
                columns: new[] { "CompanyId", "CompanyLocationId", "IsActive", "IsRejected", "Visibility", "CreatedByCandidateId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyContactProjections",
                schema: "applications");
        }
    }
}
