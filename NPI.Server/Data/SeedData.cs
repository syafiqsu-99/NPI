using Microsoft.EntityFrameworkCore;
using NPI.Server.Models;

namespace NPI.Server.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            // ── Departments ───────────────────────────────────────────────────
            if (!await context.Departments.AnyAsync())
            {
                var departments = new[]
                {
                    new Departments { dept_name = "Sales",      dept_code = "SLS", description = "Sales & Customer Service" },
                    new Departments { dept_name = "Technical",  dept_code = "TEC", description = "Technical & Engineering" },
                    new Departments { dept_name = "Purchaser",  dept_code = "PUR", description = "Purchasing Department" },
                    new Departments { dept_name = "Production", dept_code = "PRD", description = "Production Department" },
                    new Departments { dept_name = "QA",         dept_code = "QA",  description = "Quality Assurance" },
                    new Departments { dept_name = "Management", dept_code = "MGT", description = "Management" }
                };
                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // ── System Roles ──────────────────────────────────────────────────
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Roles { role_name = "Admin",   description = "System Administrator" },
                    new Roles { role_name = "Manager", description = "Department Manager" },
                    new Roles { role_name = "Member",  description = "Team Member" }
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            if (!await context.ProjectRoles.AnyAsync())
            {
                var projectRoles = new[]
                {
                    new ProjectRoles
                    {
                        role_name   = "Team Lead",
                        description = "Full edit access within the project. Can reassign tasks and update dates.",
                        is_active   = true,
                        created_at  = DateTime.Now
                    },
                    new ProjectRoles
                    {
                        role_name   = "Member",
                        description = "Can update tasks in their own department.",
                        is_active   = true,
                        created_at  = DateTime.Now
                    },
                    new ProjectRoles
                    {
                        role_name   = "Viewer",
                        description = "Read-only access. Cannot modify any task or upload files.",
                        is_active   = true,
                        created_at  = DateTime.Now
                    }
                };
                await context.ProjectRoles.AddRangeAsync(projectRoles);
                await context.SaveChangesAsync();
            }

            // ── Admin User ────────────────────────────────────────────────────
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

            // ── Document Types ────────────────────────────────────────────────
            if (!await context.DocumentTypes.AnyAsync())
            {
                var techDept = await context.Departments.FirstAsync(d => d.dept_code == "TEC");
                var salesDept = await context.Departments.FirstAsync(d => d.dept_code == "SLS");
                var qaDept = await context.Departments.FirstAsync(d => d.dept_code == "QA");
                var purDept = await context.Departments.FirstAsync(d => d.dept_code == "PUR");
                var prdDept = await context.Departments.FirstAsync(d => d.dept_code == "PRD");

                var docTypes = new[]
                {
                    new DocumentTypes { type_name = "Sales Enquiry Form",     dept_id = salesDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Customer Approval",       dept_id = salesDept.dept_id, is_required = true },
                    new DocumentTypes { type_name = "Customer Info",           dept_id = salesDept.dept_id, is_required = false },
                    new DocumentTypes { type_name = "Proposal Drawing",        dept_id = techDept.dept_id,  is_required = true },
                    new DocumentTypes { type_name = "DFM Report",              dept_id = techDept.dept_id,  is_required = true },
                    new DocumentTypes { type_name = "Quotation",               dept_id = techDept.dept_id,  is_required = true },
                    new DocumentTypes { type_name = "Mold Cost Comparison",    dept_id = techDept.dept_id,  is_required = false },
                    new DocumentTypes { type_name = "Product Drawing",         dept_id = techDept.dept_id,  is_required = true },
                    new DocumentTypes { type_name = "Mold Drawing",            dept_id = techDept.dept_id,  is_required = true },
                    new DocumentTypes { type_name = "Gantt Chart",             dept_id = techDept.dept_id,  is_required = true },
                    new DocumentTypes { type_name = "Blue Card",               dept_id = techDept.dept_id,  is_required = false },
                    new DocumentTypes { type_name = "Improvement Report",      dept_id = techDept.dept_id,  is_required = false },
                    new DocumentTypes { type_name = "Material SDS",            dept_id = purDept.dept_id,   is_required = true },
                    new DocumentTypes { type_name = "Material TDS",            dept_id = purDept.dept_id,   is_required = true },
                    new DocumentTypes { type_name = "Material FDA",            dept_id = purDept.dept_id,   is_required = false },
                    new DocumentTypes { type_name = "Delivery Order",          dept_id = purDept.dept_id,   is_required = false },
                    new DocumentTypes { type_name = "Packing Format",          dept_id = prdDept.dept_id,   is_required = true },
                    new DocumentTypes { type_name = "FAI Report",              dept_id = qaDept.dept_id,    is_required = true },
                    new DocumentTypes { type_name = "Cp/Cpk Report",           dept_id = qaDept.dept_id,    is_required = true }
                };
                await context.DocumentTypes.AddRangeAsync(docTypes);
                await context.SaveChangesAsync();
            }

            // ── NPI Categories ────────────────────────────────────────────────
            if (!await context.NpiCategories.AnyAsync())
            {
                var cats = new[]
                {
                    new NpiCategory { category_name = "New Design - Bottle/ Jar/ Tub/ Container", display_order = 1 },
                    new NpiCategory { category_name = "Duplication - Bottle/ Jar/ Tub Container",  display_order = 2 },
                    new NpiCategory { category_name = "New Design - Cap/ Lid",                      display_order = 3 },
                    new NpiCategory { category_name = "Duplication - Cap/ Lid",                     display_order = 4 },
                    new NpiCategory { category_name = "Label",                                      display_order = 5 },
                    new NpiCategory { category_name = "Wadding/ Seal",                              display_order = 6 },
                    new NpiCategory { category_name = "New Material/New Color to Existing Product", display_order = 7 },
                };
                await context.NpiCategories.AddRangeAsync(cats);
                await context.SaveChangesAsync();
            }

            // ── NPI Form Sections ─────────────────────────────────────────────
            if (!await context.NpiFormSections.AnyAsync())
            {
                var sections = new[]
                {
                    new NpiFormSection
                    {
                        section_key      = "generalInfo",
                        section_label    = "General Information - Cap/ Lid",
                        trigger_keywords = "bottle,jar,tub,container,cap,lid,label,new material,new color",
                        display_order    = 1
                    },
                    new NpiFormSection
                    {
                        section_key      = "sealInfo",
                        section_label    = "Seal/ Wadding/ Gasket",
                        trigger_keywords = "wadding,seal",
                        display_order    = 2
                    }
                };
                await context.NpiFormSections.AddRangeAsync(sections);
                await context.SaveChangesAsync();
            }

            // ── NPI Form Fields ───────────────────────────────────────────────
            if (!await context.NpiFormFields.AnyAsync())
            {
                var genSection = await context.NpiFormSections.FirstAsync(s => s.section_key == "generalInfo");
                var sealSection = await context.NpiFormSections.FirstAsync(s => s.section_key == "sealInfo");

                var fields = new List<NpiFormField>
                {
                    // General Info
                    new() { section_id = genSection.section_id,  field_key = "company_name",          field_label = "Company Name",                        field_type = "text",   is_required = true,  display_order = 1 },
                    new() { section_id = genSection.section_id,  field_key = "estimated_qty_per_year",field_label = "Estimated Quantity / Year",             field_type = "number", is_required = false, display_order = 2 },
                    new() { section_id = genSection.section_id,  field_key = "estimated_required_date",field_label = "Estimated Required Date",              field_type = "date",   is_required = false, display_order = 3 },
                    new() { section_id = genSection.section_id,  field_key = "color",                 field_label = "Color",                               field_type = "text",   is_required = false, display_order = 4 },
                    new() { section_id = genSection.section_id,  field_key = "material_used",         field_label = "Material Used",                       field_type = "text",   is_required = false, display_order = 5 },
                    new() { section_id = genSection.section_id,  field_key = "weight_g",              field_label = "Weight (g)",                          field_type = "number", is_required = false, display_order = 6 },
                    new() { section_id = genSection.section_id,  field_key = "neck_size_mm",          field_label = "Neck Size (mm)",                      field_type = "select", is_required = false, display_order = 7,options = """["A 32mm","AA 28mm","B 28mm","BS 38mm","BW 38mm","BX 38mm","C 28mm","CX 32mm","CY 28mm","CS 28mm","D 24mm","J 63mm","JN 90mm","W 110mm"]""" },
                    new() { section_id = genSection.section_id,  field_key = "shape",                field_label = "Shape",                                field_type = "select", is_required = false, display_order = 8,options = """["Round","Square","Hexagonal","Oval","Rectangular","Other"]""" },
                    new() { section_id = genSection.section_id,  field_key = "hot_cold_filling",     field_label = "Hot/ Cold Filling",                    field_type = "select", is_required = false, display_order = 9,options = """["Hot","Cold"]""" },
                    new() { section_id = genSection.section_id,  field_key = "qty_first_submission", field_label = "Quantity Required For First Submission",field_type = "number", is_required = false, display_order = 10 },
                    new() { section_id = genSection.section_id,  field_key = "cap_bottle_source",    field_label = "Cap Matching With Bottle, Bottle Source By", field_type = "select", is_required = false, display_order = 11,options = """["Jebsen & Jessen - New","Jebsen & Jessen - Existing Bottle","Customer"]""" },
                    new() { section_id = genSection.section_id,  field_key = "filling_content",      field_label = "Filling Content",                      field_type = "text",   is_required = false, display_order = 12 },
                    new() { section_id = genSection.section_id,  field_key = "capping_method",       field_label = "Capping Method",                       field_type = "select", is_required = false, display_order = 13,options = """["Auto","Manual"]""" },
                    new() { section_id = genSection.section_id,  field_key = "cap_seal",             field_label = "Cap Seal",                             field_type = "select", is_required = false, display_order = 14,options = """["N/A","Wadding","Seal","Gasket","Others"]""" },

                    // Seal Info
                    new() { section_id = sealSection.section_id, field_key = "customer_name",          field_label = "Customer Name",                       field_type = "text",   is_required = true,  display_order = 1 },
                    new() { section_id = sealSection.section_id, field_key = "apply_to_product",        field_label = "Apply to which product?",             field_type = "select", is_required = false, display_order = 2,options = """["New Cap","Existing Cap"]""" },
                    new() { section_id = sealSection.section_id, field_key = "estimated_required_date", field_label = "Estimated Required Date",             field_type = "date",   is_required = false, display_order = 3 },
                    new() { section_id = sealSection.section_id, field_key = "reason_of_change",        field_label = "Reason Of Change",                   field_type = "text",   is_required = false, display_order = 4 },
                    new() { section_id = sealSection.section_id, field_key = "qty_first_submission",    field_label = "Quantity Required For First Submission",field_type = "number",is_required = false, display_order = 5 },
                    new() { section_id = sealSection.section_id, field_key = "other_requirements",      field_label = "Others Requirements",                 field_type = "text",   is_required = false, display_order = 6 },
                };

                await context.NpiFormFields.AddRangeAsync(fields);
                await context.SaveChangesAsync();
            }

            // ── Role Permissions ──────────────────────────────────────────────
            if (!await context.RolePermissions.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.role_name == "Admin");
                var managerRole = await context.Roles.FirstAsync(r => r.role_name == "Manager");
                var memberRole = await context.Roles.FirstAsync(r => r.role_name == "Member");

                var resources = new[] { "projects", "tasks", "files", "approvals", "users", "departments" };
                var permissions = new List<RolePermissions>();

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

                await context.RolePermissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }
    }
}