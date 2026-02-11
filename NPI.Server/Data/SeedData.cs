using Microsoft.EntityFrameworkCore;
using NPI.Server.Models;

namespace NPI.Server.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new[]
                {
                    new Departments { dept_name = "Sales", dept_code = "SLS", description = "Sales & Customer Service" },
                    new Departments { dept_name = "Technical", dept_code = "TEC", description = "Technical & Engineering" },
                    new Departments { dept_name = "Purchaser", dept_code = "PUR", description = "Purchasing Department" },
                    new Departments { dept_name = "Production", dept_code = "PRD", description = "Production Department" },
                    new Departments { dept_name = "QA", dept_code = "QA", description = "Quality Assurance" },
                    new Departments { dept_name = "Management", dept_code = "MGT", description = "Management" }
                };
                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Roles { role_name = "Admin", description = "System Administrator" },
                    new Roles { role_name = "Manager", description = "Department Manager" },
                    new Roles { role_name = "Team Lead", description = "Team Leader" },
                    new Roles { role_name = "Member", description = "Team Member" },
                    new Roles { role_name = "Viewer", description = "Read-only access" }
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed Admin User
            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.role_name == "Admin");
                var mgtDept = await context.Departments.FirstAsync(d => d.dept_code == "MGT");

                var adminUser = new Users
                {
                    username = "admin",
                    email = "admin@npi.com",
                    full_name = "System Administrator",
                    dept_id = mgtDept.dept_id,
                    role_id = adminRole.role_id,
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    is_active = true,
                    created_at = DateTime.Now
                };
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }

            // Seed Document Types
            if (!await context.DocumentTypes.AnyAsync())
            {
                var techDept = await context.Departments.FirstAsync(d => d.dept_code == "TEC");
                var salesDept = await context.Departments.FirstAsync(d => d.dept_code == "SLS");
                var qaDept = await context.Departments.FirstAsync(d => d.dept_code == "QA");
                var purDept = await context.Departments.FirstAsync(d => d.dept_code == "PUR");
                var prdDept = await context.Departments.FirstAsync(d => d.dept_code == "PRD");

                var docTypes = new[]
                {
                    // Sales Documents
                    new DocumentTypes { type_name = "Sales Enquiry Form", dept_id = salesDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Customer Approval", dept_id = salesDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Customer Info", dept_id = salesDept.dept_id, is_required = false },
                    
                    // Technical Documents
                    new DocumentTypes { type_name = "Proposal Drawing", dept_id = techDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "DFM Report", dept_id = techDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Quotation", dept_id = techDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Mold Cost Comparison", dept_id = techDept.dept_id, is_required = false },
                    new DocumentTypes { type_name = "Product Drawing", dept_id = techDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Mold Drawing", dept_id = techDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Gantt Chart", dept_id = techDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Blue Card", dept_id = techDept.dept_id, is_required = false },
                    new DocumentTypes { type_name = "Improvement Report", dept_id = techDept.dept_id, is_required = false },
                    
                    // Purchaser Documents
                    new DocumentTypes { type_name = "Material SDS", dept_id = purDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Material TDS", dept_id = purDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Material FDA", dept_id = purDept.dept_id, is_required = false },
                    new DocumentTypes { type_name = "Delivery Order", dept_id = purDept.dept_id, is_required = false },
                    
                    // Production Documents
                    new DocumentTypes { type_name = "Packing Format", dept_id = prdDept.dept_id, is_required = true },
                    
                    // QA Documents
                    new DocumentTypes { type_name = "FAI Report", dept_id = qaDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Cp/Cpk Report", dept_id = qaDept.dept_id, is_required = true }
                };
                await context.DocumentTypes.AddRangeAsync(docTypes);
                await context.SaveChangesAsync();
            }

            // Seed Role Permissions
            if (!await context.RolePermissions.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.role_name == "Admin");
                var managerRole = await context.Roles.FirstAsync(r => r.role_name == "Manager");
                var memberRole = await context.Roles.FirstAsync(r => r.role_name == "Member");
                var viewerRole = await context.Roles.FirstAsync(r => r.role_name == "Viewer");

                var resources = new[] { "projects", "tasks", "files", "approvals", "users", "departments" };

                var permissions = new List<RolePermissions>();

                // Admin - Full access
                foreach (var resource in resources)
                {
                    permissions.Add(new RolePermissions
                    {
                        role_id = adminRole.role_id,
                        resource = resource,
                        can_create = true,
                        can_read = true,
                        can_update = true,
                        can_delete = true,
                        can_approve = true
                    });
                }

                // Manager - Most access except user management
                foreach (var resource in resources.Where(r => r != "users" && r != "departments"))
                {
                    permissions.Add(new RolePermissions
                    {
                        role_id = managerRole.role_id,
                        resource = resource,
                        can_create = true,
                        can_read = true,
                        can_update = true,
                        can_delete = false,
                        can_approve = true
                    });
                }

                // Member - CRUD on projects, tasks, files
                foreach (var resource in new[] { "projects", "tasks", "files" })
                {
                    permissions.Add(new RolePermissions
                    {
                        role_id = memberRole.role_id,
                        resource = resource,
                        can_create = true,
                        can_read = true,
                        can_update = true,
                        can_delete = false,
                        can_approve = false
                    });
                }

                // Viewer - Read-only
                foreach (var resource in resources)
                {
                    permissions.Add(new RolePermissions
                    {
                        role_id = viewerRole.role_id,
                        resource = resource,
                        can_create = false,
                        can_read = true,
                        can_update = false,
                        can_delete = false,
                        can_approve = false
                    });
                }

                await context.RolePermissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
