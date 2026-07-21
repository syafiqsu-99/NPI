using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using NPI.Server.Data;
using NPI.Server.Helpers;
using NPI.Server.Models;
using PdfSharp.Fonts;

namespace NPI.Server.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateEnquiryPdfAsync(int enquiryId);
        Task<byte[]> GenerateProjectStatusReportAsync(int projectId);
    }

    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        static PdfService()
        {
            if (GlobalFontSettings.FontResolver == null)
                GlobalFontSettings.FontResolver = new CustomFontResolver();
        }

        public async Task<byte[]> GenerateEnquiryPdfAsync(int enquiryId)
        {
            // Load enquiry with new dynamic field values + preserved CustomerRef
            var enquiry = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.FieldValues)
                .Include(e => e.CustomerRef)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId)
                ?? throw new Exception("Enquiry not found.");

            // Load the full section/field metadata to resolve labels
            var sections = await _context.NpiFormSections
                .Include(s => s.Fields)
                .Where(s => s.is_active)
                .OrderBy(s => s.display_order)
                .ToListAsync();

            // Build lookup: section_key → { field_key → value }
            var valueMap = (enquiry.FieldValues ?? Enumerable.Empty<EnquiryFieldValues>())
                .GroupBy(v => v.section_key)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(v => v.field_key, v => v.field_value ?? "N/A"));

            var document = new Document();
            document.Info.Title = $"Enquiry {enquiry.enquiry_no}";

            DefineStyles(document);

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2.5);

            // Header
            AddEnquiryHeader(section, enquiry);

            // Customer information
            AddCustomerInformation(section, enquiry);

            // NPI Category
            AddNpiCategory(section, enquiry);

            // Dynamic sections — renders every section that has answered fields
            foreach (var sec in sections)
            {
                if (!valueMap.TryGetValue(sec.section_key, out var sectionValues))
                    continue;

                if (sectionValues.Count == 0) continue;

                AddDynamicSection(section, sec, sectionValues, document);
            }

            // Customer reference (always preserved separately)
            if (enquiry.CustomerRef is not null)
                AddCustomerReference(section, enquiry.CustomerRef);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }

        // ── Rendering helpers ─────────────────────────────────────────────────

        private static void AddEnquiryHeader(Section section, Enquiries enquiry)
        {
            var header = section.AddParagraph("Sales Enquiry");
            header.Format.Font.Size = 18;
            header.Format.Font.Bold = true;
            header.Format.Alignment = ParagraphAlignment.Center;

            var subheader = section.AddParagraph($"Enquiry No: {enquiry.enquiry_no}");
            subheader.Format.Font.Size = 14;
            subheader.Format.Alignment = ParagraphAlignment.Center;

            var statusPara = section.AddParagraph();
            statusPara.Format.Alignment = ParagraphAlignment.Center;
            statusPara.Format.SpaceAfter = "0.5cm";
            var statusText = statusPara.AddFormattedText($"Status: {enquiry.status}");
            statusText.Bold = true;
            statusText.Color = enquiry.status switch
            {
                "Submitted" => Colors.Blue,
                _ => Colors.Black
            };

            section.AddParagraph();
        }

        private static void AddCustomerInformation(Section section, Enquiries enquiry)
        {
            section.AddParagraph("Customer Information", "Heading1");
            var table = CreateTable(section);
            AddTableRow(table, "Company Name:", enquiry.Customer?.comp_name ?? "N/A");
            AddTableRow(table, "Contact Person:", enquiry.Customer?.contact_name ?? "N/A");
            AddTableRow(table, "Contact Email:", enquiry.Customer?.contact_email ?? "N/A");
            AddTableRow(table, "Contact Phone:", enquiry.Customer?.contact_phone ?? "N/A");
            AddTableRow(table, "Address:", enquiry.Customer?.cust_addr ?? "N/A");
            AddTableRow(table, "Enquiry Date:", enquiry.created_at.ToString("dd MMM yyyy HH:mm"));
            AddTableRow(table, "Created By:", enquiry.CreatedByUser?.username ?? "N/A");
        }

        private static void AddNpiCategory(Section section, Enquiries enquiry)
        {
            section.AddParagraph("NPI Category", "Heading1");
            var para = section.AddParagraph();
            para.Format.Font.Size = 11;
            para.Format.Font.Bold = true;
            para.AddText(enquiry.npi_category);
            para.Format.SpaceAfter = "0.5cm";
        }

        private static void AddDynamicSection(
            Section section,
            NpiFormSection sec,
            Dictionary<string, string?> values,
            Document document)
        {
            section.AddParagraph(sec.section_label, "Heading1");

            var table = CreateTable(section);

            var orderedFields = (sec.Fields ?? Enumerable.Empty<NpiFormField>())
                .Where(f => f.is_active)
                .OrderBy(f => f.display_order)
                .ToList();

            foreach (var field in orderedFields)
            {
                if (!values.TryGetValue(field.field_key, out var val)) continue;
                AddTableRow(table, $"{field.field_label}:", val ?? "N/A");
            }

            foreach (var kv in values)
            {
                if (orderedFields.Any(f => f.field_key == kv.Key)) continue;
                AddTableRow(table, $"{kv.Key}:", kv.Value ?? "N/A");
            }
        }

        private static void AddCustomerReference(Section section, EnquiryCustomerRef cref)
        {
            section.AddParagraph("Customer Reference", "Heading1");
            var table = CreateTable(section);
            AddTableRow(table, "Mould Ownership:", cref.mould_ownership ?? "N/A");
        }

        // ── Table utilities ───────────────────────────────────────────────────

        private static Table CreateTable(Section section)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;
            var col1 = table.AddColumn("7cm");
            col1.Format.Alignment = ParagraphAlignment.Left;
            var col2 = table.AddColumn("10cm");
            col2.Format.Alignment = ParagraphAlignment.Left;
            return table;
        }

        private static void AddTableRow(Table table, string label, string value)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(label);
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Shading.Color = Colors.LightGray;
            row.Cells[1].AddParagraph(value);
        }

        private static void DefineStyles(Document document)
        {
            var normal = document.Styles["Normal"];
            normal.Font.Name = "Arial";
            normal.Font.Size = 10;

            var h1 = document.Styles["Heading1"];
            h1.Font.Name = "Arial";
            h1.Font.Size = 13;
            h1.Font.Bold = true;
            h1.ParagraphFormat.SpaceBefore = "0.3cm";
            h1.ParagraphFormat.SpaceAfter = "0.2cm";

            var h2 = document.Styles["Heading2"];
            h2.Font.Name = "Arial";
            h2.Font.Size = 11;
            h2.Font.Bold = true;
            h2.ParagraphFormat.SpaceBefore = "0.2cm";
            h2.ParagraphFormat.SpaceAfter = "0.15cm";
        }

        public async Task<byte[]> GenerateProjectStatusReportAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Tasks).ThenInclude(t => t.Department)
                .Include(p => p.ProjectTeams).ThenInclude(pt => pt.User)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.proj_id == projectId)
                ?? throw new Exception("Project not found");

            var document = new Document();
            DefineStyles(document);

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            // ── Header bar ──────────────────────────────────────────────────────────
            AddProjectHeader(section, project);

            // ── KPI summary row (4 boxes) ────────────────────────────────────────────
            AddKpiRow(section, project);

            // ── Stage pipeline visual ────────────────────────────────────────────────
            AddStagePipeline(section, project);

            // ── Task table grouped by stage ──────────────────────────────────────────
            AddTaskTable(section, project);

            // ── Team members ─────────────────────────────────────────────────────────
            AddTeamSection(section, project);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }

        private void AddProjectHeader(Section section, Projects project)
        {
            var headerPara = section.AddParagraph();
            headerPara.Format.SpaceAfter = "0.3cm";
            var run = headerPara.AddFormattedText($"NPI Project Status Report", TextFormat.Bold);
            run.Size = 18;

            var meta = section.AddParagraph(
                $"{project.proj_no} | {project.proj_name} | Status: {project.status} | " +
                $"Generated: {DateTime.Now:dd MMM yyyy HH:mm}");
            meta.Format.Font.Size = 9;
            meta.Format.Font.Color = Colors.Gray;
            meta.Format.SpaceAfter = "0.5cm";

            section.AddParagraph(); // spacer
        }

        private void AddKpiRow(Section section, Projects project)
        {
            var tasks = project.Tasks.ToList();
            var total = tasks.Count;
            var completed = tasks.Count(t => t.status == "Completed");
            var overdue = tasks.Count(t =>
                t.status != "Completed" &&
                t.planned_end_date.HasValue &&
                t.planned_end_date.Value < DateOnly.FromDateTime(DateTime.Now));
            var progress = total > 0 ? (int)((completed / (double)total) * 100) : 0;

            section.AddParagraph("Key Performance Indicators", "Heading2");

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.LightGray;

            for (int i = 0; i < 4; i++)
            {
                var col = table.AddColumn("4.4cm");
                col.Format.Alignment = ParagraphAlignment.Center;
            }

            var row = table.AddRow();
            row.Height = "2cm";

            AddKpiCell(row.Cells[0], total.ToString(), "Total Tasks", Colors.SteelBlue);
            AddKpiCell(row.Cells[1], completed.ToString(), "Completed", Colors.Green);
            AddKpiCell(row.Cells[2], overdue.ToString(), "Overdue", Colors.OrangeRed);
            AddKpiCell(row.Cells[3], $"{progress}%", "Overall Progress", Colors.Purple);

            section.AddParagraph(); // spacer
        }

        private void AddKpiCell(Cell cell, string value, string label, Color color)
        {
            cell.Shading.Color = new Color(color.R, color.G, color.B, 20); // tinted bg
            var vPara = cell.AddParagraph(value);
            vPara.Format.Font.Bold = true;
            vPara.Format.Font.Size = 22;
            vPara.Format.Font.Color = color;
            vPara.Format.Alignment = ParagraphAlignment.Center;

            var lPara = cell.AddParagraph(label);
            lPara.Format.Font.Size = 9;
            lPara.Format.Font.Color = Colors.Gray;
            lPara.Format.Alignment = ParagraphAlignment.Center;
        }

        private void AddStagePipeline(Section section, Projects project)
        {
            section.AddParagraph("NPI Stage Progress", "Heading2");

            var stageIds = new[] { "0.0", "1.0", "2.0", "3.0", "4.0", "5.0" };
            var stageNames = NpiStages.Names;

            var table = section.AddTable();
            table.Borders.Width = 0;

            foreach (var _ in stageIds)
                table.AddColumn("2.5cm").Format.Alignment = ParagraphAlignment.Center;

            var row = table.AddRow();
            row.Height = "1.5cm";

            for (int i = 0; i < stageIds.Length; i++)
            {
                var sid = stageIds[i];
                if (!stageNames.ContainsKey(sid)) continue;

                var stageTasks = project.Tasks.Where(t => t.stage_id == sid).ToList();
                bool completed = stageTasks.Any() && stageTasks.All(t => t.status == "Completed");
                bool inProgress = stageTasks.Any(t => t.status == "In Progress");

                var cell = row.Cells[i];
                cell.Shading.Color = completed ? Colors.LightGreen
                                  : inProgress ? Colors.LightBlue
                                  : Colors.LightGray;

                var para = cell.AddParagraph(sid);
                para.Format.Font.Bold = true;
                para.Format.Font.Size = 10;
                para.Format.Alignment = ParagraphAlignment.Center;

                var name = cell.AddParagraph(stageNames[sid]);
                name.Format.Font.Size = 8;
                name.Format.Alignment = ParagraphAlignment.Center;

                if (stageTasks.Any())
                {
                    var done = stageTasks.Count(t => t.status == "Completed");
                    var count = cell.AddParagraph($"{done}/{stageTasks.Count}");
                    count.Format.Font.Size = 8;
                    count.Format.Font.Color = Colors.DimGray;
                    count.Format.Alignment = ParagraphAlignment.Center;
                }
            }

            section.AddParagraph(); // spacer
        }

        private void AddTaskTable(Section section, Projects project)
        {
            section.AddParagraph("Task Breakdown by Stage", "Heading2");

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.LightGray;

            table.AddColumn("1.5cm"); // code
            table.AddColumn("5.5cm"); // title
            table.AddColumn("2cm");   // dept
            table.AddColumn("2cm");   // status
            table.AddColumn("1.5cm"); // progress

            var header = table.AddRow();
            header.Shading.Color = Colors.SteelBlue;
            var headers = new[] { "Code", "Task", "Dept", "Status", "%" };
            for (int i = 0; i < headers.Length; i++)
            {
                var p = header.Cells[i].AddParagraph(headers[i]);
                p.Format.Font.Bold = true;
                p.Format.Font.Color = Colors.White;
                p.Format.Font.Size = 9;
            }

            var grouped = project.Tasks
                .Where(t => t.stage_id != null)
                .GroupBy(t => t.stage_id)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                // Stage separator row
                var stageRow = table.AddRow();
                stageRow.Shading.Color = Colors.LightSteelBlue;
                var sp = stageRow.Cells[0].AddParagraph($"Stage {group.Key}");
                sp.Format.Font.Bold = true;
                sp.Format.Font.Size = 9;
                stageRow.Cells[0].MergeRight = 4;

                foreach (var task in group.OrderBy(t => t.task_code))
                {
                    var tRow = table.AddRow();
                    tRow.Cells[0].AddParagraph(task.task_code ?? "").Format.Font.Size = 8;
                    tRow.Cells[1].AddParagraph(task.title).Format.Font.Size = 8;
                    tRow.Cells[2].AddParagraph(task.Department?.dept_name ?? "").Format.Font.Size = 8;
                    var statusPara = tRow.Cells[3].AddParagraph(task.status ?? "");
                    statusPara.Format.Font.Size = 8;
                    statusPara.Format.Font.Color = task.status == "Completed" ? Colors.Green
                                                 : task.status == "In Progress" ? Colors.SteelBlue
                                                 : Colors.Gray;
                    tRow.Cells[4].AddParagraph($"{(int)(task.per_complete ?? 0)}%").Format.Font.Size = 8;
                }
            }
        }

        private void AddTeamSection(Section section, Projects project)
        {
            if (!project.ProjectTeams.Any()) return;

            section.AddParagraph("Project Team", "Heading2");

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.LightGray;
            table.AddColumn("6cm");
            table.AddColumn("4cm");
            table.AddColumn("4cm");

            var h = table.AddRow();
            h.Shading.Color = Colors.SteelBlue;
            foreach (var (label, idx) in new[] { "Name", "Department", "Role" }.Select((l, i) => (l, i)))
            {
                var p = h.Cells[idx].AddParagraph(label);
                p.Format.Font.Bold = true;
                p.Format.Font.Color = Colors.White;
                p.Format.Font.Size = 9;
            }

            foreach (var member in project.ProjectTeams)
            {
                var r = table.AddRow();
                r.Cells[0].AddParagraph(member.User?.full_name ?? member.User?.username ?? "").Format.Font.Size = 9;
                r.Cells[1].AddParagraph(member.User?.Department?.dept_name ?? "").Format.Font.Size = 9;
                r.Cells[2].AddParagraph(member.role ?? "").Format.Font.Size = 9;
            }
        }
    }

    public class CustomFontResolver : IFontResolver
    {
        private static readonly Dictionary<string, string> FontMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Arial#Regular"] = "arial.ttf",
                ["Arial#Bold"] = "arialbd.ttf",
                ["Arial#Italic"] = "ariali.ttf",
                ["Arial#BoldItalic"] = "arialbi.ttf",
                ["Courier New#Regular"] = "arial.ttf",
                ["Courier New#Bold"] = "arialbd.ttf",
                ["Courier New#Italic"] = "ariali.ttf",
                ["Courier New#BoldItalic"] = "arialbi.ttf",
                ["Courier#Regular"] = "arial.ttf",
                ["Courier#Bold"] = "arialbd.ttf",
            };

        private readonly string _fontsDir;

        public CustomFontResolver()
        {
            _fontsDir = Path.Combine(
                AppContext.BaseDirectory, "Fonts");
        }

        public byte[] GetFont(string faceName)
        {
            if (FontMap.TryGetValue(faceName, out var fileName))
            {
                var path = Path.Combine(_fontsDir, fileName);
                if (File.Exists(path))
                    return File.ReadAllBytes(path);
            }

            var fallback = Path.Combine(_fontsDir, "arial.ttf");
            if (File.Exists(fallback))
                return File.ReadAllBytes(fallback);

            throw new InvalidOperationException(
                $"Font file not found for '{faceName}' in '{_fontsDir}'. " +
                "Ensure arial.ttf and arialbd.ttf exist in the Fonts directory.");
        }

        public FontResolverInfo ResolveTypeface(
            string familyName, bool isBold, bool isItalic)
        {
            var suffix = (isBold, isItalic) switch
            {
                (true, true) => "#BoldItalic",
                (true, false) => "#Bold",
                (false, true) => "#Italic",
                _ => "#Regular",
            };

            var key = $"Arial{suffix}";
            return new FontResolverInfo(key);
        }
    }
}
