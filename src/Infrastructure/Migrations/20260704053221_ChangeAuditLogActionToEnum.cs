using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAuditLogActionToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "AuditLogs");

            migrationBuilder.AddColumn<int>(
                name: "Action",
                table: "AuditLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "Action",
            table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
            name: "Action",
            table: "AuditLogs",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "");
        }
    }
}
