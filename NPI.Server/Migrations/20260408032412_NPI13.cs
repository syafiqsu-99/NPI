using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskRevisions_ProjectRevisions_revision_id",
                table: "TaskRevisions");

            migrationBuilder.AlterColumn<int>(
                name: "revision_id",
                table: "TaskRevisions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ProjectRoles",
                columns: table => new
                {
                    project_role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Member"),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    assigned_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRoles", x => x.project_role_id);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_assigned_by",
                table: "ProjectRoles",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_Project_User",
                table: "ProjectRoles",
                columns: new[] { "proj_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_user_id",
                table: "ProjectRoles",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRevisions_ProjectRevisions_revision_id",
                table: "TaskRevisions",
                column: "revision_id",
                principalTable: "ProjectRevisions",
                principalColumn: "revision_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskRevisions_ProjectRevisions_revision_id",
                table: "TaskRevisions");

            migrationBuilder.DropTable(
                name: "ProjectRoles");

            migrationBuilder.AlterColumn<int>(
                name: "revision_id",
                table: "TaskRevisions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskRevisions_ProjectRevisions_revision_id",
                table: "TaskRevisions",
                column: "revision_id",
                principalTable: "ProjectRevisions",
                principalColumn: "revision_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
