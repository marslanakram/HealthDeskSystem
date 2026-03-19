using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                schema: "dbo",
                table: "Users",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "dbo",
                table: "Users");
        }
    }
}
