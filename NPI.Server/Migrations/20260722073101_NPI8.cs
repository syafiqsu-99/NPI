using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class NPI8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "revision_no",
                table: "Enquiries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EnquiryReviews",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    revision_no = table.Column<int>(type: "int", nullable: false),
                    reviewed_by = table.Column<int>(type: "int", nullable: false),
                    decision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    remark = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryReviews", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_EnquiryReviews_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryReviews_Users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryRevisionSnapshots",
                columns: table => new
                {
                    snapshot_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    revision_no = table.Column<int>(type: "int", nullable: false),
                    cust_id = table.Column<int>(type: "int", nullable: false),
                    form_category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    field_values_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    submitted_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryRevisionSnapshots", x => x.snapshot_id);
                    table.ForeignKey(
                        name: "FK_EnquiryRevisionSnapshots_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryRevisionSnapshots_Users_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskComments",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskComments", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_TaskComments_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskComments_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryReviews_enquiry_id_created_at",
                table: "EnquiryReviews",
                columns: new[] { "enquiry_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryReviews_reviewed_by",
                table: "EnquiryReviews",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryRevisionSnapshots_submitted_by",
                table: "EnquiryRevisionSnapshots",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryRevSnap_Enquiry_Rev",
                table: "EnquiryRevisionSnapshots",
                columns: new[] { "enquiry_id", "revision_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_Task_Created",
                table: "TaskComments",
                columns: new[] { "task_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_user_id",
                table: "TaskComments",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnquiryReviews");

            migrationBuilder.DropTable(
                name: "EnquiryRevisionSnapshots");

            migrationBuilder.DropTable(
                name: "TaskComments");

            migrationBuilder.DropColumn(
                name: "revision_no",
                table: "Enquiries");
        }
    }
}
