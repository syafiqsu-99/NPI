using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Projects_proj_id",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Milestones_task_id",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "Projectsproj_id",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_read",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "task_id",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "task_id",
                table: "Milestones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "StageCompletionLogs",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    stage_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    completed_by = table.Column<int>(type: "int", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageCompletionLogs", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_StageCompletionLogs_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageCompletionLogs_Users_completed_by",
                        column: x => x.completed_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TaskDocumentRequirements",
                columns: table => new
                {
                    req_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    doc_type_id = table.Column<int>(type: "int", nullable: false),
                    is_fulfilled = table.Column<bool>(type: "bit", nullable: false),
                    fulfilled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    file_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDocumentRequirements", x => x.req_id);
                    table.ForeignKey(
                        name: "FK_TaskDocumentRequirements_DocumentTypes_doc_type_id",
                        column: x => x.doc_type_id,
                        principalTable: "DocumentTypes",
                        principalColumn: "doc_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskDocumentRequirements_Files_file_id",
                        column: x => x.file_id,
                        principalTable: "Files",
                        principalColumn: "file_id");
                    table.ForeignKey(
                        name: "FK_TaskDocumentRequirements_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Projectsproj_id",
                table: "Notifications",
                column: "Projectsproj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_task_id",
                table: "Notifications",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_User_Unread",
                table: "Notifications",
                columns: new[] { "user_id", "is_read", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_task_id",
                table: "Milestones",
                column: "task_id",
                unique: true,
                filter: "[task_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StageCompletionLog_Project_Stage",
                table: "StageCompletionLogs",
                columns: new[] { "proj_id", "stage_id" });

            migrationBuilder.CreateIndex(
                name: "IX_StageCompletionLogs_completed_by",
                table: "StageCompletionLogs",
                column: "completed_by");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocumentRequirements_doc_type_id",
                table: "TaskDocumentRequirements",
                column: "doc_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocumentRequirements_file_id",
                table: "TaskDocumentRequirements",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocumentRequirements_Task",
                table: "TaskDocumentRequirements",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskDocumentRequirements_Unique",
                table: "TaskDocumentRequirements",
                columns: new[] { "task_id", "doc_type_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files",
                column: "task_id",
                principalTable: "Tasks",
                principalColumn: "task_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Projects_Projectsproj_id",
                table: "Notifications",
                column: "Projectsproj_id",
                principalTable: "Projects",
                principalColumn: "proj_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Projects_proj_id",
                table: "Notifications",
                column: "proj_id",
                principalTable: "Projects",
                principalColumn: "proj_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tasks_task_id",
                table: "Notifications",
                column: "task_id",
                principalTable: "Tasks",
                principalColumn: "task_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Projects_Projectsproj_id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Projects_proj_id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tasks_task_id",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "StageCompletionLogs");

            migrationBuilder.DropTable(
                name: "TaskDocumentRequirements");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Projectsproj_id",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_task_id",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_User_Unread",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Milestones_task_id",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "Projectsproj_id",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "is_read",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "task_id",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "task_id",
                table: "Milestones",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_task_id",
                table: "Milestones",
                column: "task_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files",
                column: "task_id",
                principalTable: "Tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Projects_proj_id",
                table: "Notifications",
                column: "proj_id",
                principalTable: "Projects",
                principalColumn: "proj_id");
        }
    }
}
