using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using NPI.Server.Data;
using NPI.Server.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Fonts;

namespace NPI.Server.Services
{
    public class PdfService : IPdfService
    {
        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateEnquiryPdfAsync(int enquiryId)
        {
            var enquiry = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.GeneralInfo)
                .Include(e => e.SealInfo)
                .Include(e => e.CustomerRef)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

            if (enquiry == null)
                throw new Exception("Enquiry not found");

            // Create a new document
            var document = new Document();
            document.Info.Title = $"Enquiry {enquiry.enquiry_no}";
            document.Info.Author = "NPI System";

            // Define styles
            DefineStyles(document);

            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new CustomFontResolver();
            }

            // Add section
            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2.5);

            // Add header
            AddHeader(section, enquiry);

            // Add customer information
            AddCustomerInformation(section, enquiry);

            // Add NPI category
            AddNpiCategory(section, enquiry);

            // Add general information
            if (enquiry.GeneralInfo != null)
            {
                AddGeneralInformation(section, enquiry.GeneralInfo);
            }

            // Add seal information
            if (enquiry.SealInfo != null)
            {
                AddSealInformation(section, enquiry.SealInfo);
            }

            // Add customer reference
            if (enquiry.CustomerRef != null)
            {
                AddCustomerReference(section, enquiry.CustomerRef);
            }

            // Render document to PDF
            var renderer = new PdfDocumentRenderer
            {
                Document = document,
                PdfDocument =
                {
                    PageLayout = PdfPageLayout.SinglePage,
                    ViewerPreferences =
                    {
                        FitWindow = true
                    }
                }
            };

            renderer.RenderDocument();

            // Save to memory stream
            using (MemoryStream stream = new MemoryStream())
            {
                renderer.PdfDocument.Save(stream, false);
                return stream.ToArray();
            }
        }

        private void DefineStyles(Document document)
        {
            var normal = document.Styles["Normal"];
            normal.Font.Name = "Arial";
            normal.Font.Size = 10;

            var heading1 = document.Styles["Heading1"];
            heading1.Font.Name = "Arial";
            heading1.Font.Size = 14;
            heading1.Font.Bold = true;

            var heading2 = document.Styles["Heading2"];
            heading2.Font.Name = "Arial";
            heading2.Font.Size = 12;
            heading2.Font.Bold = true;
        }

        private void AddHeader(Section section, Enquiries enquiry)
        {
            Paragraph header = section.AddParagraph();
            header.Format.Font.Size = 18;
            header.Format.Font.Bold = true;
            header.Format.Alignment = ParagraphAlignment.Center;
            header.AddText("Sales Enquiry");

            Paragraph subheader = section.AddParagraph();
            subheader.Format.Font.Size = 14;
            subheader.Format.Alignment = ParagraphAlignment.Center;
            subheader.AddText($"Enquiry No: {enquiry.enquiry_no}");

            // Status
            Paragraph statusPara = section.AddParagraph();
            statusPara.Format.Alignment = ParagraphAlignment.Center;
            statusPara.Format.SpaceAfter = "0.5cm";
            var statusText = statusPara.AddFormattedText($"Status: {enquiry.status}");
            statusText.Bold = true;

            if (enquiry.status == "Approved")
                statusText.Color = Colors.Green;
            else if (enquiry.status == "Submitted")
                statusText.Color = Colors.Blue;
            else if (enquiry.status == "Rejected")
                statusText.Color = Colors.Red;

            section.AddParagraph(); // Spacing
        }

        private void AddCustomerInformation(Section section, Enquiries enquiry)
        {
            section.AddParagraph("Customer Information", "Heading1");

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            Column column = table.AddColumn("7cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            AddTableRow(table, "Company Name:", enquiry.Customer?.comp_name ?? "N/A");
            AddTableRow(table, "Contact Person:", enquiry.Customer?.contact_name ?? "N/A");
            AddTableRow(table, "Contact Email:", enquiry.Customer?.contact_email ?? "N/A");
            AddTableRow(table, "Contact Phone:", enquiry.Customer?.contact_phone ?? "N/A");
            AddTableRow(table, "Address:", enquiry.Customer?.cust_addr ?? "N/A");
            AddTableRow(table, "Enquiry Date:", enquiry.created_at.ToString("dd MMM yyyy HH:mm"));
            AddTableRow(table, "Created By:", enquiry.CreatedByUser?.username ?? "N/A");
        }

        private void AddNpiCategory(Section section, Enquiries enquiry)
        {
            section.AddParagraph("NPI Category", "Heading1");

            Paragraph para = section.AddParagraph();
            para.Format.Font.Size = 11;
            para.Format.Font.Bold = true;
            para.AddText(enquiry.npi_category);
            para.Format.SpaceAfter = "0.5cm";
        }

        private void AddGeneralInformation(Section section, EnquiryGeneralInfo info)
        {
            section.AddParagraph("General Information - Cap/Lid", "Heading1");

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            Column column = table.AddColumn("7cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            AddTableRow(table, "Company Name:", info.company_name ?? "N/A");
            AddTableRow(table, "Estimated Qty/Year:", info.estimated_qty_per_year?.ToString() ?? "N/A");
            AddTableRow(table, "Required Date:", info.estimated_required_date?.ToString("dd MMM yyyy") ?? "N/A");
            AddTableRow(table, "Color:", info.color ?? "N/A");
            AddTableRow(table, "Material Used:", info.material_used ?? "N/A");
            AddTableRow(table, "Weight (g):", info.weight_g?.ToString() ?? "N/A");
            AddTableRow(table, "Neck Size (mm):", info.neck_size_mm ?? "N/A");
            AddTableRow(table, "Shape:", info.shape ?? "N/A");
            AddTableRow(table, "Hot/Cold Filling:", info.hot_cold_filling ?? "N/A");
            AddTableRow(table, "Qty First Submission:", info.qty_first_submission?.ToString() ?? "N/A");
            AddTableRow(table, "Cap/Bottle Source:", info.cap_bottle_source ?? "N/A");
            AddTableRow(table, "Filling Content:", info.filling_content ?? "N/A");
            AddTableRow(table, "Capping Method:", info.capping_method ?? "N/A");
            AddTableRow(table, "Cap Seal:", info.cap_seal ?? "N/A");
        }

        private void AddSealInformation(Section section, EnquirySealInfo info)
        {
            section.AddParagraph("Seal/Wadding/Gasket Information", "Heading1");

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            Column column = table.AddColumn("7cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            AddTableRow(table, "Customer Name:", info.customer_name ?? "N/A");
            AddTableRow(table, "Apply to Product:", info.apply_to_product ?? "N/A");
            AddTableRow(table, "Required Date:", info.estimated_required_date?.ToString("dd MMM yyyy") ?? "N/A");
            AddTableRow(table, "Reason of Change:", info.reason_of_change ?? "N/A");
            AddTableRow(table, "Qty First Submission:", info.qty_first_submission?.ToString() ?? "N/A");
            AddTableRow(table, "Other Requirements:", info.other_requirements ?? "N/A");
        }

        private void AddCustomerReference(Section section, EnquiryCustomerRef info)
        {
            section.AddParagraph("Customer Reference", "Heading1");

            Table table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = Colors.Gray;

            Column column = table.AddColumn("7cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("10cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            AddTableRow(table, "Mould Ownership:", info.mould_ownership ?? "N/A");
        }

        private void AddTableRow(Table table, string label, string value)
        {
            Row row = table.AddRow();
            row.Cells[0].AddParagraph(label);
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Shading.Color = Colors.LightGray;
            row.Cells[1].AddParagraph(value);
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

            if (GlobalFontSettings.FontResolver == null)
                GlobalFontSettings.FontResolver = new CustomFontResolver();

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
            var stageNames = new Dictionary<string, string>
            {
                ["0.0"] = "Enquiry",
                ["1.0"] = "Project Start",
                ["2.0"] = "Pilot Mould",
                ["3.0"] = "New Machine",
                ["4.0"] = "Prod Mould",
                ["5.0"] = "Trial JJ"
            };

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
        public byte[] GetFont(string faceName)
        {
            var fontFile = faceName switch
            {
                "Arial#Regular" => "arial.ttf",
                "Arial#Bold" => "arialbd.ttf",
                _ => throw new InvalidOperationException($"Unknown font: {faceName}")
            };

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", fontFile);
            return File.ReadAllBytes(path);
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold)
                    return new FontResolverInfo("Arial#Bold");

                return new FontResolverInfo("Arial#Regular");
            }

            return new FontResolverInfo("Arial#Regular");
        }
    }
}
