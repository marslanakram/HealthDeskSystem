using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeparateAuditLogTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "AuditLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                schema: "dbo",
                table: "AuditLogs",
                column: "TenantId",
                principalSchema: "dbo",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                schema: "dbo",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId",
                schema: "dbo",
                table: "AuditLogs",
                column: "TenantId",
                principalSchema: "dbo",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
