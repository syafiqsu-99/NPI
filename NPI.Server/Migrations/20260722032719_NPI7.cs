using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NpiCategories");

            migrationBuilder.DropTable(
                name: "NpiFormFields");

            migrationBuilder.DropTable(
                name: "NpiFormSections");

            migrationBuilder.RenameColumn(
                name: "npi_category",
                table: "Enquiries",
                newName: "form_category");

            migrationBuilder.CreateTable(
                name: "FormCategories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormCategories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "FormSections",
                columns: table => new
                {
                    section_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    section_label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    trigger_keywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSections", x => x.section_id);
                });

            migrationBuilder.CreateTable(
                name: "FormFields",
                columns: table => new
                {
                    field_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    field_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    field_label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    field_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_required = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFields", x => x.field_id);
                    table.ForeignKey(
                        name: "FK_FormFields_FormSections_section_id",
                        column: x => x.section_id,
                        principalTable: "FormSections",
                        principalColumn: "section_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormFields_section_id",
                table: "FormFields",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormCategories");

            migrationBuilder.DropTable(
                name: "FormFields");

            migrationBuilder.DropTable(
                name: "FormSections");

            migrationBuilder.RenameColumn(
                name: "form_category",
                table: "Enquiries",
                newName: "npi_category");

            migrationBuilder.CreateTable(
                name: "NpiCategories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpiCategories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "NpiFormSections",
                columns: table => new
                {
                    section_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    section_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    section_label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    trigger_keywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpiFormSections", x => x.section_id);
                });

            migrationBuilder.CreateTable(
                name: "NpiFormFields",
                columns: table => new
                {
                    field_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    field_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    field_label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    field_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_required = table.Column<bool>(type: "bit", nullable: false),
                    options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpiFormFields", x => x.field_id);
                    table.ForeignKey(
                        name: "FK_NpiFormFields_NpiFormSections_section_id",
                        column: x => x.section_id,
                        principalTable: "NpiFormSections",
                        principalColumn: "section_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NpiFormFields_section_id",
                table: "NpiFormFields",
                column: "section_id");
        }
    }
}
