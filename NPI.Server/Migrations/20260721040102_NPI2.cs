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
            migrationBuilder.DropForeignKey(
                name: "FK_Files_DocumentTypes_doc_type_id",
                table: "Files");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Files_doc_type_id",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "color_hex",
                table: "Departments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color_hex",
                table: "Departments");

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    doc_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_required = table.Column<bool>(type: "bit", nullable: false),
                    type_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.doc_type_id);
                    table.ForeignKey(
                        name: "FK_DocumentTypes_Departments_dept_id",
                        column: x => x.dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_doc_type_id",
                table: "Files",
                column: "doc_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_dept_id",
                table: "DocumentTypes",
                column: "dept_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_DocumentTypes_doc_type_id",
                table: "Files",
                column: "doc_type_id",
                principalTable: "DocumentTypes",
                principalColumn: "doc_type_id");
        }
    }
}
