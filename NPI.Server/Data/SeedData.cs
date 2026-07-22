using Microsoft.EntityFrameworkCore;
using NPI.Server.Helpers;
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
                    new Departments { dept_name = "Sales",      dept_code = "SLS", description = "Sales & Customer Service", color_hex = "#2196F3", is_assignable = true},
                    new Departments { dept_name = "Technical",  dept_code = "TEC", description = "Technical & Engineering" , color_hex = "#4CAF50", is_assignable = true},
                    new Departments { dept_name = "Purchaser",  dept_code = "PUR", description = "Purchasing Department"   , color_hex = "#FF9800", is_assignable = true},
                    new Departments { dept_name = "Production", dept_code = "PRD", description = "Production Department"   , color_hex = "#F44336", is_assignable = true},
                    new Departments { dept_name = "QA",         dept_code = "QA",  description = "Quality Assurance"       , color_hex = "#9C27B0", is_assignable = true},
                    new Departments { dept_name = "Management", dept_code = "MGT", description = "Management"              , color_hex = "#607D8B", is_assignable = false}
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
                    password_hash = PasswordHelper.HashPassword("Admin@123"),
                    is_active = true,
                    created_at = DateTime.Now
                };
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }

            // ── NPI Categories ────────────────────────────────────────────────
            if (!await context.FormCategories.AnyAsync())
            {
                var cats = new[]
                {
                    new FormCategory { category_name = "New Design - Bottle/ Jar/ Tub/ Container", display_order = 1 },
                    new FormCategory { category_name = "Duplication - Bottle/ Jar/ Tub Container",  display_order = 2 },
                    new FormCategory { category_name = "New Design - Cap/ Lid",                      display_order = 3 },
                    new FormCategory { category_name = "Duplication - Cap/ Lid",                     display_order = 4 },
                    new FormCategory { category_name = "Label",                                      display_order = 5 },
                    new FormCategory { category_name = "Wadding/ Seal",                              display_order = 6 },
                    new FormCategory { category_name = "New Material/New Color to Existing Product", display_order = 7 },
                };
                await context.FormCategories.AddRangeAsync(cats);
                await context.SaveChangesAsync();
            }

            // ── NPI Form Sections ─────────────────────────────────────────────
            if (!await context.FormSections.AnyAsync())
            {
                var sections = new[]
                {
                    new FormSection
                    {
                        section_key      = "generalInfo",
                        section_label    = "General Information - Cap/ Lid",
                        trigger_keywords = "bottle,jar,tub,container,cap,lid,label,new material,new color",
                        display_order    = 1
                    },
                    new FormSection
                    {
                        section_key      = "sealInfo",
                        section_label    = "Seal/ Wadding/ Gasket",
                        trigger_keywords = "wadding,seal",
                        display_order    = 2
                    }
                };
                await context.FormSections.AddRangeAsync(sections);
                await context.SaveChangesAsync();
            }

            // ── NPI Form Fields ───────────────────────────────────────────────
            if (!await context.FormFields.AnyAsync())
            {
                var genSection = await context.FormSections.FirstAsync(s => s.section_key == "generalInfo");
                var sealSection = await context.FormSections.FirstAsync(s => s.section_key == "sealInfo");

                var fields = new List<FormField>
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

                await context.FormFields.AddRangeAsync(fields);
                await context.SaveChangesAsync();
            }

            // ── Task Templates ──────────────────────────────────────────
            if (!await context.TaskTemplates.AnyAsync())
            {
                var deptByCode = await context.Departments
                    .ToDictionaryAsync(d => d.dept_code!, d => d.dept_id);

                int DeptId(string code) => deptByCode.TryGetValue(code, out var id)
                    ? id
                    : throw new InvalidOperationException(
                        $"Seed failed: department code '{code}' not found. " +
                        "TaskTemplates requires a non-null dept_id.");

                var sls = DeptId("SLS");
                var tec = DeptId("TEC");
                var pur = DeptId("PUR");
                var prd = DeptId("PRD");
                var qa = DeptId("QA");

                var seededAt = DateTime.Now;

                var templates = new List<TaskTemplates>
                {
                    // ── 0.0 Enquiry ───────────────────────────────────────────
                    new() { stage_id = "0.0", task_code = "0.1",  title = "Sales Enquiry Form",                        dept_id = sls, default_duration = 1, has_link = true,  display_order = 1 },
                    new() { stage_id = "0.0", task_code = "0.2",  title = "Customer Info",                             dept_id = sls, default_duration = 1, has_link = true,  display_order = 2 },
                    new() { stage_id = "0.0", task_code = "0.3",  title = "Project Awarded",                           dept_id = tec, default_duration = 1, has_link = false, display_order = 3 },

                    // ── 1.0 Project Start ─────────────────────────────────────
                    new() { stage_id = "1.0", task_code = "1.1",  title = "Project awarded / Contract signing",        dept_id = sls, default_duration = 5, has_link = false, display_order = 1 },
                    new() { stage_id = "1.0", task_code = "1.2",  title = "Drawing preparation",                       dept_id = tec, default_duration = 5, has_link = true,  display_order = 2 },
                    new() { stage_id = "1.0", task_code = "1.3",  title = "Drawing submission to customer",            dept_id = sls, default_duration = 5, has_link = false, display_order = 3 },
                    new() { stage_id = "1.0", task_code = "1.4",  title = "DFM preparation",                           dept_id = tec, default_duration = 5, has_link = true,  display_order = 4 },
                    new() { stage_id = "1.0", task_code = "1.5",  title = "DFM submission to customer",                dept_id = tec, default_duration = 5, has_link = false, display_order = 5 },
                    new() { stage_id = "1.0", task_code = "1.6",  title = "Customer drawing approval",                 dept_id = sls, default_duration = 5, has_link = true,  display_order = 6 },
                    new() { stage_id = "1.0", task_code = "1.7",  title = "PO issuance from customer",                 dept_id = sls, default_duration = 5, has_link = false, display_order = 7 },
                    new() { stage_id = "1.0", task_code = "1.8",  title = "PO issuance to supplier",                   dept_id = pur, default_duration = 5, has_link = true,  display_order = 8 },

                    // ── 2.0 Pilot Mould Fabrication ───────────────────────────
                    new() { stage_id = "2.0", task_code = "2.1",  title = "Pilot mould fabrication",                   dept_id = tec, default_duration = 5, has_link = false, display_order = 1 },
                    new() { stage_id = "2.0", task_code = "2.2",  title = "Mould trial + samples shipment",            dept_id = tec, default_duration = 5, has_link = false, display_order = 2 },
                    new() { stage_id = "2.0", task_code = "2.3",  title = "Trial sample verification",                 dept_id = tec, default_duration = 5, has_link = true,  display_order = 3 },
                    new() { stage_id = "2.0", task_code = "2.4",  title = "Mold modification (if any)",                dept_id = tec, default_duration = 5, has_link = false, display_order = 4 },
                    new() { stage_id = "2.0", task_code = "2.5",  title = "Mould trial + samples shipment",            dept_id = tec, default_duration = 5, has_link = false, display_order = 5 },
                    new() { stage_id = "2.0", task_code = "2.6",  title = "Trial sample verification",                 dept_id = tec, default_duration = 5, has_link = true,  display_order = 6 },
                    new() { stage_id = "2.0", task_code = "2.7",  title = "Discussion with customer",                  dept_id = sls, default_duration = 5, has_link = false, display_order = 7 },
                    new() { stage_id = "2.0", task_code = "2.8",  title = "Samples submission to QA",                  dept_id = tec, default_duration = 5, has_link = false, display_order = 8 },
                    new() { stage_id = "2.0", task_code = "2.9",  title = "FAI",                                       dept_id = qa,  default_duration = 5, has_link = true,  display_order = 9 },
                    new() { stage_id = "2.0", task_code = "2.10", title = "Samples submission to customer",            dept_id = sls, default_duration = 5, has_link = false, display_order = 10 },
                    new() { stage_id = "2.0", task_code = "2.11", title = "Customer approval",                         dept_id = sls, default_duration = 5, has_link = true,  display_order = 11 },

                    // ── 3.0 New Machine Purchase ──────────────────────────────
                    new() { stage_id = "3.0", task_code = "3.1",  title = "Machine fabrication",                       dept_id = tec, default_duration = 5, has_link = false, display_order = 1 },
                    new() { stage_id = "3.0", task_code = "3.2",  title = "Packing of machine & mould",                dept_id = tec, default_duration = 5, has_link = false, display_order = 2 },
                    new() { stage_id = "3.0", task_code = "3.3",  title = "Machine delivery",                          dept_id = tec, default_duration = 5, has_link = false, display_order = 3 },
                    new() { stage_id = "3.0", task_code = "3.4",  title = "Machine installation, test and commissioning", dept_id = tec, default_duration = 5, has_link = false, display_order = 4 },

                    // ── 4.0 Production Mould Fabrication ──────────────────────
                    new() { stage_id = "4.0", task_code = "4.1",  title = "MP mould fabrication",                      dept_id = tec, default_duration = 5, has_link = false, display_order = 1 },
                    new() { stage_id = "4.0", task_code = "4.2",  title = "Mould trial + samples shipment",            dept_id = tec, default_duration = 5, has_link = false, display_order = 2 },
                    new() { stage_id = "4.0", task_code = "4.3",  title = "Trial sample verification",                 dept_id = tec, default_duration = 5, has_link = true,  display_order = 3 },
                    new() { stage_id = "4.0", task_code = "4.4",  title = "Mold modification (if any)",                dept_id = tec, default_duration = 5, has_link = false, display_order = 4 },
                    new() { stage_id = "4.0", task_code = "4.5",  title = "Mould trial + samples shipment",            dept_id = tec, default_duration = 5, has_link = false, display_order = 5 },
                    new() { stage_id = "4.0", task_code = "4.6",  title = "Trial sample verification",                 dept_id = tec, default_duration = 5, has_link = true,  display_order = 6 },
                    new() { stage_id = "4.0", task_code = "4.7",  title = "Discussion with customer",                  dept_id = sls, default_duration = 5, has_link = false, display_order = 7 },
                    new() { stage_id = "4.0", task_code = "4.8",  title = "Samples submission to QA",                  dept_id = tec, default_duration = 5, has_link = false, display_order = 8 },
                    new() { stage_id = "4.0", task_code = "4.9",  title = "FAI",                                       dept_id = qa,  default_duration = 5, has_link = true,  display_order = 9 },
                    new() { stage_id = "4.0", task_code = "4.10", title = "Samples submission to customer",            dept_id = sls, default_duration = 5, has_link = false, display_order = 10 },
                    new() { stage_id = "4.0", task_code = "4.11", title = "Customer approval",                         dept_id = sls, default_duration = 5, has_link = true,  display_order = 11 },

                    // ── 5.0 Trial Run at JJ ───────────────────────────────────
                    new() { stage_id = "5.0", task_code = "5.1",  title = "Planning for trial",                        dept_id = prd, default_duration = 5, has_link = true,  display_order = 1 },
                    new() { stage_id = "5.0", task_code = "5.2",  title = "Mould trial, T0",                           dept_id = prd, default_duration = 5, has_link = false, display_order = 2 },
                    new() { stage_id = "5.0", task_code = "5.3",  title = "Trial sample verification",                 dept_id = tec, default_duration = 5, has_link = true,  display_order = 3 },
                    new() { stage_id = "5.0", task_code = "5.4",  title = "Mold modification (if any)",                dept_id = tec, default_duration = 5, has_link = false, display_order = 4 },
                    new() { stage_id = "5.0", task_code = "5.5",  title = "Mould trial, T1",                           dept_id = tec, default_duration = 5, has_link = false, display_order = 5 },
                    new() { stage_id = "5.0", task_code = "5.6",  title = "Blue Card",                                 dept_id = tec, default_duration = 5, has_link = true, display_order = 6 },
                    new() { stage_id = "5.0", task_code = "5.7",  title = "Trial sample verification",                 dept_id = tec, default_duration = 5, has_link = true, display_order = 7 },
                    new() { stage_id = "5.0", task_code = "5.8",  title = "FAI",                                       dept_id = qa,  default_duration = 5, has_link = true,  display_order = 8 },
                    new() { stage_id = "5.0", task_code = "5.9",  title = "Samples submission to customer",            dept_id = sls, default_duration = 5, has_link = false, display_order = 9 },
                    new() { stage_id = "5.0", task_code = "5.10", title = "Customer approval",                         dept_id = sls, default_duration = 5, has_link = true, display_order = 10 },
                };

                foreach (var t in templates)
                {
                    t.is_active = true;
                    t.created_at = seededAt;
                }

                await context.TaskTemplates.AddRangeAsync(templates);
                await context.SaveChangesAsync();
            }
        }
    }
}