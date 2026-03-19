using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addtenantinpermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantRolePermissions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantRolePermissions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "dbo",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantRolePermissions_TenantId_RoleName_Permission",
                schema: "dbo",
                table: "TenantRolePermissions",
                columns: new[] { "TenantId", "RoleName", "Permission" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantRolePermissions",
                schema: "dbo");
        }
    }
}
