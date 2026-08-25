using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                schema: "identity",
                table: "RefreshTokens",
                newName: "TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Token",
                schema: "identity",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_TokenHash");

            // Existing values are raw tokens. They cannot safely become token hashes,
            // so invalidate all sessions before new hashed values are issued.
            migrationBuilder.Sql("DELETE FROM identity.\"RefreshTokens\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHash",
                schema: "identity",
                table: "RefreshTokens",
                newName: "Token");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_TokenHash",
                schema: "identity",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Token");
        }
    }
}
