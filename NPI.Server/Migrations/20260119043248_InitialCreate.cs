using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NPI.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    cust_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    comp_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    cust_addr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    contact_email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    contact_phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.cust_id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    dept_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dept_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dept_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.dept_id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    doc_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    is_required = table.Column<bool>(type: "bit", nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    permission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    resource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    can_create = table.Column<bool>(type: "bit", nullable: false),
                    can_read = table.Column<bool>(type: "bit", nullable: false),
                    can_update = table.Column<bool>(type: "bit", nullable: false),
                    can_delete = table.Column<bool>(type: "bit", nullable: false),
                    can_approve = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.permission_id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_role_id",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_dept_id",
                        column: x => x.dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                    table.ForeignKey(
                        name: "FK_Users_Roles_role_id",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    proj_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    proj_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    cust_id = table.Column<int>(type: "int", nullable: false),
                    enquiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    project_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    target_completion_date = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_completion_date = table.Column<DateOnly>(type: "date", nullable: true),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.proj_id);
                    table.ForeignKey(
                        name: "FK_Projects_Customers_cust_id",
                        column: x => x.cust_id,
                        principalTable: "Customers",
                        principalColumn: "cust_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_Users_created_by",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    session_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    table_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    record_id = table.Column<int>(type: "int", nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Enquiries",
                columns: table => new
                {
                    enquiry_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    proj_id = table.Column<int>(type: "int", nullable: true),
                    npi_category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquiries", x => x.enquiry_id);
                    table.ForeignKey(
                        name: "FK_Enquiries_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id");
                    table.ForeignKey(
                        name: "FK_Enquiries_Users_created_by",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    milestone_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    milestone_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    planned_date = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    responsible_dept_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.milestone_id);
                    table.ForeignKey(
                        name: "FK_Milestones_Departments_responsible_dept_id",
                        column: x => x.responsible_dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                    table.ForeignKey(
                        name: "FK_Milestones_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    notif_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    notif_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.notif_id);
                    table.ForeignKey(
                        name: "FK_Notifications_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatusHistory",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    old_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    new_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    changed_by = table.Column<int>(type: "int", nullable: false),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "ProjectTeam",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    assigned_by = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    parent_task_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    duration = table.Column<float>(type: "real", nullable: true),
                    per_complete = table.Column<float>(type: "real", nullable: true),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    assigned_to = table.Column<int>(type: "int", nullable: true),
                    assigned_by = table.Column<int>(type: "int", nullable: true),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_Tasks_Departments_dept_id",
                        column: x => x.dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Tasks_parent_task_id",
                        column: x => x.parent_task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_assigned_to",
                        column: x => x.assigned_to,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "EnquiryCustomerRef",
                columns: table => new
                {
                    customer_ref_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    mould_ownership = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryCustomerRef", x => x.customer_ref_id);
                    table.ForeignKey(
                        name: "FK_EnquiryCustomerRef_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryGeneralInfo",
                columns: table => new
                {
                    general_info_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    company_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    estimated_qty_per_year = table.Column<int>(type: "int", nullable: true),
                    estimated_required_date = table.Column<DateOnly>(type: "date", nullable: true),
                    color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    material_used = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    weight_g = table.Column<float>(type: "real", nullable: true),
                    neck_size_mm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    shape = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    hot_cold_filling = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    qty_first_submission = table.Column<int>(type: "int", nullable: true),
                    cap_bottle_source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    filling_content = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    capping_method = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    cap_seal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryGeneralInfo", x => x.general_info_id);
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
                    seal_info_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    customer_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    apply_to_product = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    estimated_required_date = table.Column<DateOnly>(type: "date", nullable: true),
                    reason_of_change = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qty_first_submission = table.Column<int>(type: "int", nullable: true),
                    other_requirements = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquirySealInfo", x => x.seal_info_id);
                    table.ForeignKey(
                        name: "FK_EnquirySealInfo_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: true),
                    task_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    comment_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_Comments_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id");
                    table.ForeignKey(
                        name: "FK_Comments_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id");
                    table.ForeignKey(
                        name: "FK_Comments_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    file_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    task_id = table.Column<int>(type: "int", nullable: true),
                    enquiry_id = table.Column<int>(type: "int", nullable: true),
                    doc_type_id = table.Column<int>(type: "int", nullable: true),
                    file_version = table.Column<int>(type: "int", nullable: false),
                    upload_by = table.Column<int>(type: "int", nullable: false),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_latest = table.Column<bool>(type: "bit", nullable: false),
                    replaced_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.file_id);
                    table.ForeignKey(
                        name: "FK_Files_Departments_dept_id",
                        column: x => x.dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                    table.ForeignKey(
                        name: "FK_Files_DocumentTypes_doc_type_id",
                        column: x => x.doc_type_id,
                        principalTable: "DocumentTypes",
                        principalColumn: "doc_type_id");
                    table.ForeignKey(
                        name: "FK_Files_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id");
                    table.ForeignKey(
                        name: "FK_Files_Files_replaced_by",
                        column: x => x.replaced_by,
                        principalTable: "Files",
                        principalColumn: "file_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Files_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id");
                    table.ForeignKey(
                        name: "FK_Files_Users_upload_by",
                        column: x => x.upload_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Approvals",
                columns: table => new
                {
                    appr_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    file_id = table.Column<int>(type: "int", nullable: true),
                    task_id = table.Column<int>(type: "int", nullable: true),
                    appr_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    appr_level = table.Column<int>(type: "int", nullable: true),
                    appr_order = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    apprv_by = table.Column<int>(type: "int", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    rejected_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approvals", x => x.appr_id);
                    table.ForeignKey(
                        name: "FK_Approvals_Files_file_id",
                        column: x => x.file_id,
                        principalTable: "Files",
                        principalColumn: "file_id");
                    table.ForeignKey(
                        name: "FK_Approvals_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Approvals_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id");
                    table.ForeignKey(
                        name: "FK_Approvals_Users_apprv_by",
                        column: x => x.apprv_by,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_apprv_by",
                table: "Approvals",
                column: "apprv_by");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_file_id",
                table: "Approvals",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_proj_id",
                table: "Approvals",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Approvals_task_id",
                table: "Approvals",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_proj_id",
                table: "AuditLogs",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_user_id",
                table: "AuditLogs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_proj_id",
                table: "Comments",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_task_id",
                table: "Comments",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_user_id",
                table: "Comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_dept_id",
                table: "DocumentTypes",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_created_by",
                table: "Enquiries",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_enquiry_no",
                table: "Enquiries",
                column: "enquiry_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_proj_id",
                table: "Enquiries",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryCustomerRef_enquiry_id",
                table: "EnquiryCustomerRef",
                column: "enquiry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryGeneralInfo_enquiry_id",
                table: "EnquiryGeneralInfo",
                column: "enquiry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnquirySealInfo_enquiry_id",
                table: "EnquirySealInfo",
                column: "enquiry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_dept_id",
                table: "Files",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Files_doc_type_id",
                table: "Files",
                column: "doc_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Files_enquiry_id",
                table: "Files",
                column: "enquiry_id");

            migrationBuilder.CreateIndex(
                name: "IX_Files_proj_id",
                table: "Files",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Files_replaced_by",
                table: "Files",
                column: "replaced_by");

            migrationBuilder.CreateIndex(
                name: "IX_Files_task_id",
                table: "Files",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_Files_upload_by",
                table: "Files",
                column: "upload_by");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_proj_id",
                table: "Milestones",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_responsible_dept_id",
                table: "Milestones",
                column: "responsible_dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_proj_id",
                table: "Notifications",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_created_by",
                table: "Projects",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_cust_id",
                table: "Projects",
                column: "cust_id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_proj_no",
                table: "Projects",
                column: "proj_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_updated_by",
                table: "Projects",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatusHistory_changed_by",
                table: "ProjectStatusHistory",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatusHistory_proj_id",
                table: "ProjectStatusHistory",
                column: "proj_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_role_id",
                table: "RolePermissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_assigned_by",
                table: "Tasks",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_assigned_to",
                table: "Tasks",
                column: "assigned_to");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_dept_id",
                table: "Tasks",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_parent_task_id",
                table: "Tasks",
                column: "parent_task_id");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_proj_id",
                table: "Tasks",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_dept_id",
                table: "Users",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_role_id",
                table: "Users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_username",
                table: "Users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_user_id",
                table: "UserSessions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Approvals");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "EnquiryCustomerRef");

            migrationBuilder.DropTable(
                name: "EnquiryGeneralInfo");

            migrationBuilder.DropTable(
                name: "EnquirySealInfo");

            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProjectStatusHistory");

            migrationBuilder.DropTable(
                name: "ProjectTeam");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Enquiries");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
