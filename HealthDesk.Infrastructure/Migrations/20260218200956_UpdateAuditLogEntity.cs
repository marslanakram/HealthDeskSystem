using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Doctors_DoctorId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Patients_PatientId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DoctorId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PatientId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PatientId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Changes",
                schema: "dbo",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                schema: "dbo",
                table: "AuditLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                schema: "dbo",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Changes",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DoctorId",
                schema: "dbo",
                table: "Users",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PatientId",
                schema: "dbo",
                table: "Users",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Doctors_DoctorId",
                schema: "dbo",
                table: "Users",
                column: "DoctorId",
                principalSchema: "dbo",
                principalTable: "Doctors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Patients_PatientId",
                schema: "dbo",
                table: "Users",
                column: "PatientId",
                principalSchema: "dbo",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
