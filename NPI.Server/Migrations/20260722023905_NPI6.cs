using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "contact_email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "contact_name",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "contact_phone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "cust_addr",
                table: "Customers");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_seen_at",
                table: "UserSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "revoked_at",
                table: "UserSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "revoked_reason",
                table: "UserSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_seen_at",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "revoked_reason",
                table: "UserSessions");

            migrationBuilder.AddColumn<string>(
                name: "contact_email",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_name",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_phone",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cust_addr",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    permission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    can_approve = table.Column<bool>(type: "bit", nullable: false),
                    can_create = table.Column<bool>(type: "bit", nullable: false),
                    can_delete = table.Column<bool>(type: "bit", nullable: false),
                    can_read = table.Column<bool>(type: "bit", nullable: false),
                    can_update = table.Column<bool>(type: "bit", nullable: false),
                    resource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.permission_id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_role_id",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_role_id",
                table: "RolePermissions",
                column: "role_id");
        }
    }
}
