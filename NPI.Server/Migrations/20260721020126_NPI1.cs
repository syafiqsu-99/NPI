using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskTemplates",
                columns: table => new
                {
                    template_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stage_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    task_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    dept_id = table.Column<int>(type: "int", nullable: false),
                    default_duration = table.Column<int>(type: "int", nullable: false),
                    has_link = table.Column<bool>(type: "bit", nullable: false),
                    doc_format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    role_gated = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplates", x => x.template_id);
                    table.ForeignKey(
                        name: "FK_TaskTemplates_Departments_dept_id",
                        column: x => x.dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_dept_id",
                table: "TaskTemplates",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTemplates_Stage_Code",
                table: "TaskTemplates",
                columns: new[] { "stage_id", "task_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTemplates");
        }
    }
}
