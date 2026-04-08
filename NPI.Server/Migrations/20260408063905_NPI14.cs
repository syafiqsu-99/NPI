using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRoles_Users_assigned_by",
                table: "ProjectRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRoles_Users_user_id",
                table: "ProjectRoles");

            migrationBuilder.DropIndex(
                name: "IX_ProjectRoles_assigned_by",
                table: "ProjectRoles");

            migrationBuilder.DropColumn(
                name: "assigned_at",
                table: "ProjectRoles");

            migrationBuilder.DropColumn(
                name: "assigned_by",
                table: "ProjectRoles");

            migrationBuilder.RenameColumn(
                name: "project_role_id",
                table: "ProjectRoles",
                newName: "proj_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectRoles_Project_User",
                table: "ProjectRoles",
                newName: "IX_ProjectRoles_ProjUser");

            migrationBuilder.AlterColumn<string>(
                name: "role_name",
                table: "ProjectRoles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Member");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "ProjectRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "ProjectRoles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "ProjectRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "ProjectRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRoles_Users_user_id",
                table: "ProjectRoles",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRoles_Users_user_id",
                table: "ProjectRoles");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "ProjectRoles");

            migrationBuilder.DropColumn(
                name: "description",
                table: "ProjectRoles");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "ProjectRoles");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "ProjectRoles");

            migrationBuilder.RenameColumn(
                name: "proj_role_id",
                table: "ProjectRoles",
                newName: "project_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectRoles_ProjUser",
                table: "ProjectRoles",
                newName: "IX_ProjectRoles_Project_User");

            migrationBuilder.AlterColumn<string>(
                name: "role_name",
                table: "ProjectRoles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Member",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "assigned_at",
                table: "ProjectRoles",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<int>(
                name: "assigned_by",
                table: "ProjectRoles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_assigned_by",
                table: "ProjectRoles",
                column: "assigned_by");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRoles_Users_assigned_by",
                table: "ProjectRoles",
                column: "assigned_by",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRoles_Users_user_id",
                table: "ProjectRoles",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id");
        }
    }
}
