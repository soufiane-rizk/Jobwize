using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace JobWize.Modules.Identity.Persistence.Migrations
{
    [DbContext(typeof(IdentityDbContext))]
    [Migration("20260826160000_BackfillUserCreatedAt")]
    public partial class BackfillUserCreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE identity.\"Users\" SET \"CreatedAt\" = NOW() WHERE \"CreatedAt\" = '-infinity';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
