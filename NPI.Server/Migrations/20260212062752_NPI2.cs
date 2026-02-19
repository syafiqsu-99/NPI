using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_email",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "Tasks",
                newName: "planned_start_date");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "Tasks",
                newName: "planned_end_date");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateOnly>(
                name: "actual_end_date",
                table: "Tasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "actual_start_date",
                table: "Tasks",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectRevisions",
                columns: table => new
                {
                    revision_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    revision_number = table.Column<int>(type: "int", nullable: false),
                    revision_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revised_by = table.Column<int>(type: "int", nullable: false),
                    revision_notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    previous_target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    new_target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRevisions", x => x.revision_id);
                    table.ForeignKey(
                        name: "FK_ProjectRevisions_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectRevisions_Users_revised_by",
                        column: x => x.revised_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskRevisions",
                columns: table => new
                {
                    task_revision_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    revision_id = table.Column<int>(type: "int", nullable: false),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    old_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    old_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    new_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    new_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    revised_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration = table.Column<float>(type: "real", nullable: true),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskRevisions", x => x.task_revision_id);
                    table.ForeignKey(
                        name: "FK_TaskRevisions_Departments_dept_id",
                        column: x => x.dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                    table.ForeignKey(
                        name: "FK_TaskRevisions_ProjectRevisions_revision_id",
                        column: x => x.revision_id,
                        principalTable: "ProjectRevisions",
                        principalColumn: "revision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskRevisions_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRevisions_proj_id",
                table: "ProjectRevisions",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRevisions_revised_by",
                table: "ProjectRevisions",
                column: "revised_by");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRevisions_dept_id",
                table: "TaskRevisions",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRevisions_revision_id",
                table: "TaskRevisions",
                column: "revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRevisions_task_id",
                table: "TaskRevisions",
                column: "task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskRevisions");

            migrationBuilder.DropTable(
                name: "ProjectRevisions");

            migrationBuilder.DropIndex(
                name: "IX_Users_email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "actual_end_date",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "actual_start_date",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "planned_start_date",
                table: "Tasks",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "planned_end_date",
                table: "Tasks",
                newName: "end_date");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);
        }
    }
}
