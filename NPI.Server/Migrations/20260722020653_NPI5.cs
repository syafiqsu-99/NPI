using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectStatusHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectStatusHistory",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    changed_by = table.Column<int>(type: "int", nullable: false),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    new_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    old_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    remark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatusHistory", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_ProjectStatusHistory_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectStatusHistory_Users_changed_by",
                        column: x => x.changed_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatusHistory_changed_by",
                table: "ProjectStatusHistory",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatusHistory_proj_id",
                table: "ProjectStatusHistory",
                column: "proj_id");
        }
    }
}
