using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "dept_id",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Projectsproj_id",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "Departments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Departments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Projectsproj_id",
                table: "Departments",
                column: "Projectsproj_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Projects_Projectsproj_id",
                table: "Departments",
                column: "Projectsproj_id",
                principalTable: "Projects",
                principalColumn: "proj_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Projects_Projectsproj_id",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Projectsproj_id",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "dept_id",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "description",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "Projectsproj_id",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Departments");
        }
    }
}
