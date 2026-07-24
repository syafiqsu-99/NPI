using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "enquiry_id",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_enquiry_id",
                table: "Notifications",
                column: "enquiry_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Enquiries_enquiry_id",
                table: "Notifications",
                column: "enquiry_id",
                principalTable: "Enquiries",
                principalColumn: "enquiry_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Enquiries_enquiry_id",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_enquiry_id",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "enquiry_id",
                table: "Notifications");
        }
    }
}
