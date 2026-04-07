using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnquiryFieldValues",
                columns: table => new
                {
                    value_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    section_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    field_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    field_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryFieldValues", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_EnquiryFieldValues_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFieldValues_Unique",
                table: "EnquiryFieldValues",
                columns: new[] { "enquiry_id", "section_key", "field_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnquiryFieldValues");
        }
    }
}
