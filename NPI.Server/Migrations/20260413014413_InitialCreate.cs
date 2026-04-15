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
                name: "NpiCategories",
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
                    table.PrimaryKey("PK_NpiCategories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "NpiFormSections",
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
                    table.PrimaryKey("PK_NpiFormSections", x => x.section_id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRoles",
                columns: table => new
                {
                    proj_role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRoles", x => x.proj_role_id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "NpiFormFields",
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
                    table.PrimaryKey("PK_NpiFormFields", x => x.field_id);
                    table.ForeignKey(
                        name: "FK_NpiFormFields_NpiFormSections_section_id",
                        column: x => x.section_id,
                        principalTable: "NpiFormSections",
                        principalColumn: "section_id",
                        onDelete: ReferentialAction.Cascade);
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
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    dept_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dept_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dept_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Projectsproj_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.dept_id);
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
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    updated_by = table.Column<int>(type: "int", nullable: true),
                    storage_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    pilot_mould_required = table.Column<bool>(type: "bit", nullable: false),
                    machine_purchase_required = table.Column<bool>(type: "bit", nullable: false),
                    dept_id = table.Column<int>(type: "int", nullable: false)
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
                name: "Enquiries",
                columns: table => new
                {
                    enquiry_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enquiry_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    proj_id = table.Column<int>(type: "int", nullable: true),
                    cust_id = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_Enquiries_Customers_cust_id",
                        column: x => x.cust_id,
                        principalTable: "Customers",
                        principalColumn: "cust_id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "ProjectRevisions",
                columns: table => new
                {
                    revision_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    revision_number = table.Column<int>(type: "int", nullable: false),
                    revision_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revised_by = table.Column<int>(type: "int", nullable: false),
                    revision_notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    previous_target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    new_target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRevisions", x => x.revision_id);
                    table.ForeignKey(
                        name: "FK_ProjectRevisions_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectRevisions_Users_revised_by",
                        column: x => x.revised_by,
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

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    parent_task_id = table.Column<int>(type: "int", nullable: true),
                    stage_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    task_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    planned_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    planned_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    actual_end_date = table.Column<DateOnly>(type: "date", nullable: true),
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
                    enquiry_id = table.Column<int>(type: "int", nullable: false),
                    mould_ownership = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryCustomerRef", x => x.enquiry_id);
                    table.ForeignKey(
                        name: "FK_EnquiryCustomerRef_Enquiries_enquiry_id",
                        column: x => x.enquiry_id,
                        principalTable: "Enquiries",
                        principalColumn: "enquiry_id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                name: "Milestones",
                columns: table => new
                {
                    milestone_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proj_id = table.Column<int>(type: "int", nullable: false),
                    task_id = table.Column<int>(type: "int", nullable: true),
                    milestone_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Milestones_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    notif_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    proj_id = table.Column<int>(type: "int", nullable: true),
                    task_id = table.Column<int>(type: "int", nullable: true),
                    notif_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Projectsproj_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.notif_id);
                    table.ForeignKey(
                        name: "FK_Notifications_Projects_Projectsproj_id",
                        column: x => x.Projectsproj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id");
                    table.ForeignKey(
                        name: "FK_Notifications_Projects_proj_id",
                        column: x => x.proj_id,
                        principalTable: "Projects",
                        principalColumn: "proj_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskRevisions",
                columns: table => new
                {
                    task_revision_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    revision_id = table.Column<int>(type: "int", nullable: true),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    old_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    old_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    new_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    new_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    revised_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration = table.Column<float>(type: "real", nullable: true),
                    dept_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskRevisions", x => x.task_revision_id);
                    table.ForeignKey(
                        name: "FK_TaskRevisions_Departments_dept_id",
                        column: x => x.dept_id,
                        principalTable: "Departments",
                        principalColumn: "dept_id");
                    table.ForeignKey(
                        name: "FK_TaskRevisions_ProjectRevisions_revision_id",
                        column: x => x.revision_id,
                        principalTable: "ProjectRevisions",
                        principalColumn: "revision_id");
                    table.ForeignKey(
                        name: "FK_TaskRevisions_Tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "Tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_proj_id",
                table: "AuditLogs",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_user_id",
                table: "AuditLogs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Projectsproj_id",
                table: "Departments",
                column: "Projectsproj_id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_dept_id",
                table: "DocumentTypes",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_created_by",
                table: "Enquiries",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_cust_id",
                table: "Enquiries",
                column: "cust_id");

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
                name: "IX_EnquiryFieldValues_Unique",
                table: "EnquiryFieldValues",
                columns: new[] { "enquiry_id", "section_key", "field_key" },
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
                name: "IX_Milestones_task_id",
                table: "Milestones",
                column: "task_id",
                unique: true,
                filter: "[task_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_proj_id",
                table: "Notifications",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Projectsproj_id",
                table: "Notifications",
                column: "Projectsproj_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_task_id",
                table: "Notifications",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_User_Unread",
                table: "Notifications",
                columns: new[] { "user_id", "is_read", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_NpiFormFields_section_id",
                table: "NpiFormFields",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRevisions_proj_id",
                table: "ProjectRevisions",
                column: "proj_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRevisions_revised_by",
                table: "ProjectRevisions",
                column: "revised_by");

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
                name: "IX_ProjectTeams_assigned_by",
                table: "ProjectTeams",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_ProjUser",
                table: "ProjectTeams",
                columns: new[] { "proj_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_user_id",
                table: "ProjectTeams",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_role_id",
                table: "RolePermissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRevisions_dept_id",
                table: "TaskRevisions",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRevisions_revision_id",
                table: "TaskRevisions",
                column: "revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRevisions_task_id",
                table: "TaskRevisions",
                column: "task_id");

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
                unique: true,
                filter: "[email] IS NOT NULL");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Projects_proj_id",
                table: "AuditLogs",
                column: "proj_id",
                principalTable: "Projects",
                principalColumn: "proj_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_user_id",
                table: "AuditLogs",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Projects_Projectsproj_id",
                table: "Departments",
                column: "Projectsproj_id",
                principalTable: "Projects",
                principalColumn: "proj_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Projects_Projectsproj_id",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EnquiryCustomerRef");

            migrationBuilder.DropTable(
                name: "EnquiryFieldValues");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "NpiCategories");

            migrationBuilder.DropTable(
                name: "NpiFormFields");

            migrationBuilder.DropTable(
                name: "ProjectRoles");

            migrationBuilder.DropTable(
                name: "ProjectStatusHistory");

            migrationBuilder.DropTable(
                name: "ProjectTeams");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "TaskRevisions");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Enquiries");

            migrationBuilder.DropTable(
                name: "NpiFormSections");

            migrationBuilder.DropTable(
                name: "ProjectRevisions");

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
