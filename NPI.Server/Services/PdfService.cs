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
