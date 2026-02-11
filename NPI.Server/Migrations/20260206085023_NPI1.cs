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
            migrationBuilder.DropPrimaryKey(
                name: "PK_EnquirySealInfo",
                table: "EnquirySealInfo");

            migrationBuilder.DropIndex(
                name: "IX_EnquirySealInfo_enquiry_id",
                table: "EnquirySealInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnquiryGeneralInfo",
                table: "EnquiryGeneralInfo");

            migrationBuilder.DropIndex(
                name: "IX_EnquiryGeneralInfo_enquiry_id",
                table: "EnquiryGeneralInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnquiryCustomerRef",
                table: "EnquiryCustomerRef");

            migrationBuilder.DropIndex(
                name: "IX_EnquiryCustomerRef_enquiry_id",
                table: "EnquiryCustomerRef");

            migrationBuilder.DropColumn(
                name: "seal_info_id",
                table: "EnquirySealInfo");

            migrationBuilder.DropColumn(
                name: "general_info_id",
                table: "EnquiryGeneralInfo");

            migrationBuilder.DropColumn(
                name: "customer_ref_id",
                table: "EnquiryCustomerRef");

            migrationBuilder.AddColumn<string>(
                name: "storage_path",
                table: "Projects",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cust_id",
                table: "Enquiries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnquirySealInfo",
                table: "EnquirySealInfo",
                column: "enquiry_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnquiryGeneralInfo",
                table: "EnquiryGeneralInfo",
                column: "enquiry_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnquiryCustomerRef",
                table: "EnquiryCustomerRef",
                column: "enquiry_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_cust_id",
                table: "Enquiries",
                column: "cust_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enquiries_Customers_cust_id",
                table: "Enquiries",
                column: "cust_id",
                principalTable: "Customers",
                principalColumn: "cust_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enquiries_Customers_cust_id",
                table: "Enquiries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnquirySealInfo",
                table: "EnquirySealInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnquiryGeneralInfo",
                table: "EnquiryGeneralInfo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnquiryCustomerRef",
                table: "EnquiryCustomerRef");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_cust_id",
                table: "Enquiries");

            migrationBuilder.DropColumn(
                name: "storage_path",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "cust_id",
                table: "Enquiries");

            migrationBuilder.AddColumn<int>(
                name: "seal_info_id",
                table: "EnquirySealInfo",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "general_info_id",
                table: "EnquiryGeneralInfo",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "customer_ref_id",
                table: "EnquiryCustomerRef",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnquirySealInfo",
                table: "EnquirySealInfo",
                column: "seal_info_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnquiryGeneralInfo",
                table: "EnquiryGeneralInfo",
                column: "general_info_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnquiryCustomerRef",
                table: "EnquiryCustomerRef",
                column: "customer_ref_id");

            migrationBuilder.CreateIndex(
                name: "IX_EnquirySealInfo_enquiry_id",
                table: "EnquirySealInfo",
                column: "enquiry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryGeneralInfo_enquiry_id",
                table: "EnquiryGeneralInfo",
                column: "enquiry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryCustomerRef_enquiry_id",
                table: "EnquiryCustomerRef",
                column: "enquiry_id",
                unique: true);
        }
    }
}
