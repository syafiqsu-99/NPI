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
            migrationBuilder.DropColumn(
                name: "doc_type_id",
                table: "Files");

            migrationBuilder.AddColumn<bool>(
                name: "is_assignable",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_assignable",
                table: "Departments");

            migrationBuilder.AddColumn<int>(
                name: "doc_type_id",
                table: "Files",
                type: "int",
                nullable: true);
        }
    }
}
