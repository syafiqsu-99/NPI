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
            migrationBuilder.DropTable(
                name: "ProjectTeam");

            migrationBuilder.CreateTable(
                name: "ProjectTeams",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    assigned_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTeams", x => x.team_id);
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_assigned_by",
                table: "ProjectTeams",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_proj_id",
                table: "ProjectTeams",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_user_id",
                table: "ProjectTeams",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTeams");

            migrationBuilder.CreateTable(
                name: "ProjectTeam",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assigned_by = table.Column<int>(type: "int", nullable: true),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTeam", x => x.team_id);
                    table.ForeignKey(
                        name: "FK_ProjectTeam_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTeam_Users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectTeam_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeam_assigned_by",
                table: "ProjectTeam",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeam_proj_id",
                table: "ProjectTeam",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeam_user_id",
                table: "ProjectTeam",
                column: "user_id");
        }
    }
}
