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
            migrationBuilder.AddColumn<int>(
                name: "task_id",
                table: "Milestones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_task_id",
                table: "Milestones",
                column: "task_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Milestones_Tasks_task_id",
                table: "Milestones",
                column: "task_id",
                principalTable: "Tasks",
                principalColumn: "task_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestones_Tasks_task_id",
                table: "Milestones");

            migrationBuilder.DropIndex(
                name: "IX_Milestones_task_id",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "task_id",
                table: "Milestones");
        }
    }
}
