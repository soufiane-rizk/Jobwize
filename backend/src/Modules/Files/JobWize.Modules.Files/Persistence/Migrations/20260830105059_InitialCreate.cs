using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Files.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "file_bindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccessPolicy = table.Column<int>(type: "integer", nullable: false),
                    BoundAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_bindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_file_bindings_file_assets_FileAssetId",
                        column: x => x.FileAssetId,
                        principalTable: "file_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_file_assets_CandidateId_ArchivedAt",
                table: "file_assets",
                columns: new[] { "CandidateId", "ArchivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_file_bindings_FileAssetId_ReleasedAt",
                table: "file_bindings",
                columns: new[] { "FileAssetId", "ReleasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_file_bindings_ResourceType_ResourceId_Usage_ReleasedAt",
                table: "file_bindings",
                columns: new[] { "ResourceType", "ResourceId", "Usage", "ReleasedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_bindings");

            migrationBuilder.DropTable(
                name: "file_assets");
        }
    }
}
