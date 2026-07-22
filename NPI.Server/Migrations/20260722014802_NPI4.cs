using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Departments_Projects_Projectsproj_id",
            //    table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Projects_Projectsproj_id",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Projectsproj_id",
                table: "Notifications");

            //migrationBuilder.DropIndex(
            //    name: "IX_Departments_Projectsproj_id",
            //    table: "Departments");

            migrationBuilder.DropColumn(
                name: "doc_format",
                table: "TaskTemplates");

            migrationBuilder.DropColumn(
                name: "role_gated",
                table: "TaskTemplates");

            migrationBuilder.DropColumn(
                name: "dept_id",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Projectsproj_id",
                table: "Notifications");

            //migrationBuilder.DropColumn(
            //    name: "Projectsproj_id",
            //    table: "Departments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "doc_format",
                table: "TaskTemplates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "role_gated",
                table: "TaskTemplates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "dept_id",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Projectsproj_id",
                table: "Notifications",
                type: "int",
                nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "Projectsproj_id",
            //    table: "Departments",
            //    type: "int",
            //    nullable: true);

            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    milestone_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    responsible_dept_id = table.Column<int>(type: "int", nullable: true),
                    task_id = table.Column<int>(type: "int", nullable: true),
                    actual_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    milestone_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    planned_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.milestone_id);
                    table.ForeignKey(
                        name: "FK_Milestones_Departments_responsible_dept_id",
                        column: x => x.responsible_dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                    table.ForeignKey(
                        name: "FK_Milestones_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Milestones_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Projectsproj_id",
                table: "Notifications",
                column: "Projectsproj_id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Departments_Projectsproj_id",
            //    table: "Departments",
            //    column: "Projectsproj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_proj_id",
                table: "Milestones",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_responsible_dept_id",
                table: "Milestones",
                column: "responsible_dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_task_id",
                table: "Milestones",
                column: "task_id",
                unique: true,
                filter: "[task_id] IS NOT NULL");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Departments_Projects_Projectsproj_id",
            //    table: "Departments",
            //    column: "Projectsproj_id",
            //    principalTable: "Projects",
            //    principalColumn: "proj_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Projects_Projectsproj_id",
                table: "Notifications",
                column: "Projectsproj_id",
                principalTable: "Projects",
                principalColumn: "proj_id");
        }
    }
}
