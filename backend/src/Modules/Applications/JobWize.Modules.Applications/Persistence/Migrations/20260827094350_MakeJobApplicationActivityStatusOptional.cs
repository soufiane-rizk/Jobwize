using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Applications.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeJobApplicationActivityStatusOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "applications",
                table: "JobApplicationActivities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE applications."JobApplicationActivities"
                SET "Status" = 'Draft'
                WHERE "Status" IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "applications",
                table: "JobApplicationActivities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
