using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnquiryGeneralInfo");

            migrationBuilder.DropTable(
                name: "EnquirySealInfo");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "NpiFormSections",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "NpiFormSections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "NpiFormFields",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "NpiFormFields",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "NpiFormSections");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "NpiFormSections");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "NpiFormFields");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "NpiFormFields");

            migrationBuilder.CreateTable(
                name: "EnquiryGeneralInfo",
                columns: table => new
                {
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    cap_bottle_source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    cap_seal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    capping_method = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    company_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    estimated_qty_per_year = table.Column<int>(type: "int", nullable: true),
                    estimated_required_date = table.Column<DateOnly>(type: "date", nullable: true),
                    filling_content = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    hot_cold_filling = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    material_used = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    neck_size_mm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    qty_first_submission = table.Column<int>(type: "int", nullable: true),
                    shape = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    weight_g = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryGeneralInfo", x => x.enquiry_id);
                    table.ForeignKey(
                        name: "FK_EnquiryGeneralInfo_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnquirySealInfo",
                columns: table => new
                {
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    apply_to_product = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    estimated_required_date = table.Column<DateOnly>(type: "date", nullable: true),
                    other_requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qty_first_submission = table.Column<int>(type: "int", nullable: true),
                    reason_of_change = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquirySealInfo", x => x.enquiry_id);
                    table.ForeignKey(
                        name: "FK_EnquirySealInfo_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
