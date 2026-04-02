using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files",
                column: "task_id",
                principalTable: "Tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Tasks_task_id",
                table: "Files",
                column: "task_id",
                principalTable: "Tasks",
                principalColumn: "task_id");
        }
    }
}
