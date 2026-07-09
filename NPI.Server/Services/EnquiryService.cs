using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public class EnquiryService : IEnquiryService
    {
        private readonly ApplicationDbContext _context;

        public EnquiryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Create ────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message, EnquiryResponseDto? Enquiry)>
            CreateEnquiryAsync(EnquiryCreateDto dto, int userId)
        {
            try
            {
                var customerId = await ResolveCustomerIdAsync(dto);
                if (customerId == 0)
                    return (false, "Customer not found or invalid data supplied.", null);

                var enquiry = new Enquiries
                {
                    enquiry_no = await GenerateEnquiryNoAsync(),
                    cust_id = customerId,
                    npi_category = dto.npi_category,
                    status = EnquiryStatus.Draft,
                    created_by = userId,
                    created_at = DateTime.Now
                };

                _context.Enquiries.Add(enquiry);
                await _context.SaveChangesAsync();

                await SaveFieldValuesAsync(enquiry.enquiry_id, dto.field_values);
                await UpsertCustomerRefAsync(enquiry.enquiry_id, dto.CustomerRef);

                var response = await GetEnquiryByIdAsync(enquiry.enquiry_id);
                return (true, "Enquiry created successfully.", response);
            }
            catch (Exception ex)
            {
                // ex.InnerException.Message usually contains the direct SQL Server syntax error
                var msg = ex.InnerException?.Message ?? ex.Message;
                return (false, $"Error creating enquiry: {msg}", null);
            }
        }

        // ── Update ────────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)>UpdateEnquiryAsync(int enquiryId, EnquiryCreateDto dto, int userId, string userRole)
        {
            try
            {
                var enquiry = await _context.Enquiries.FindAsync(enquiryId);
                if (enquiry == null)
                    return (false, "Enquiry not found.");

                var isPrivileged = RbacHelper.IsAdminOrManager(userRole);
                if (!isPrivileged && enquiry.created_by != userId)
                    return (false, "You are not authorised to edit this enquiry.");

                if (enquiry.status != EnquiryStatus.Draft)
                    return (false, "Only Draft enquiries can be updated.");

                var customerId = await ResolveCustomerIdAsync(dto, enquiry);
                if (customerId == 0)
                    return (false, "Customer not found.");

                enquiry.cust_id = customerId;
                enquiry.npi_category = dto.npi_category;
                enquiry.updated_at = DateTime.Now;
                enquiry.updated_by = userId;

                await _context.SaveChangesAsync();

                await SaveFieldValuesAsync(enquiryId, dto.field_values);
                await UpsertCustomerRefAsync(enquiryId, dto.CustomerRef);

                return (true, "Enquiry updated successfully.");
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                return (false, $"Error updating enquiry: {msg}");
            }
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public async Task<EnquiryResponseDto?> GetEnquiryByIdAsync(int enquiryId)
        {
            var enquiry = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.FieldValues)
                .Include(e => e.CustomerRef)
                .Include(e => e.Files)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

            return enquiry is null ? null : MapToResponseDto(enquiry);
        }

        public async Task<List<EnquiryResponseDto>> GetAllEnquiriesAsync()
        {
            var enquiries = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.FieldValues)
                .Include(e => e.CustomerRef)
                .Include(e => e.CreatedByUser)
                .OrderByDescending(e => e.created_at)
                .ToListAsync();

            return enquiries.Select(MapToResponseDto).ToList();
        }

        public async Task<List<EnquiryResponseDto>> GetEnquiriesByUserAsync(int userId)
        {
            var enquiries = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.FieldValues)
                .Include(e => e.CustomerRef)
                .Include(e => e.Files)
                .Include(e => e.CreatedByUser)
                .Where(e => e.created_by == userId)
                .OrderByDescending(e => e.created_at)
                .ToListAsync();

            return enquiries.Select(MapToResponseDto).ToList();
        }

        // ── Submit / Delete ───────────────────────────────────────────────────

        public async Task<(bool Success, string Message)>
            SubmitEnquiryAsync(int enquiryId, int userId)
        {
            try
            {
                var enquiry = await _context.Enquiries.FindAsync(enquiryId);
                if (enquiry == null) return (false, "Enquiry not found.");
                if (enquiry.created_by != userId) return (false, "Unauthorised.");
                if (enquiry.status != EnquiryStatus.Draft) return (false, "Only Draft enquiries can be submitted.");

                enquiry.status = EnquiryStatus.Submitted;
                enquiry.submitted_at = DateTime.Now;
                await _context.SaveChangesAsync();

                return (true, "Enquiry submitted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error submitting enquiry: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteEnquiryAsync(int enquiryId)
        {
            try
            {
                var enquiry = await _context.Enquiries
                    .Include(e => e.FieldValues)
                    .Include(e => e.CustomerRef)
                    .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

                if (enquiry == null) return (false, "Enquiry not found.");

                _context.Enquiries.Remove(enquiry);
                await _context.SaveChangesAsync();

                return (true, "Enquiry deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting enquiry: {ex.Message}");
            }
        }

        // ── Helpers: field value persistence ─────────────────────────────────

        private async Task SaveFieldValuesAsync(
            int enquiryId,
            Dictionary<string, Dictionary<string, string?>>? sections)
        {
            if (sections is null || sections.Count == 0) return;

            var sectionKeys = sections.Keys.ToList();

            // FIX: Retrieve fields for the enquiry without using `.Contains()` in SQL to prevent 
            // EF Core 8 translating it into an unsupported OPENJSON(... WITH ...) call on older SQL versions.
            var existing = await _context.EnquiryFieldValues
                .Where(v => v.enquiry_id == enquiryId)
                .ToListAsync();

            // Filter in-memory instead
            var toRemove = existing
                .Where(v => sectionKeys.Contains(v.section_key))
                .ToList();

            if (toRemove.Count > 0)
                _context.EnquiryFieldValues.RemoveRange(toRemove);

            var newRows = sections
                .SelectMany(sec => sec.Value
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                    .Select(kv => new EnquiryFieldValues
                    {
                        enquiry_id = enquiryId,
                        section_key = sec.Key ?? string.Empty,
                        field_key = kv.Key ?? string.Empty,
                        field_value = kv.Value,
                        updated_at = DateTime.Now
                    }))
                .GroupBy(x => new { x.enquiry_id, x.section_key, x.field_key })
                .Select(g => g.First())
                .ToList();

            if (newRows.Count > 0)
                await _context.EnquiryFieldValues.AddRangeAsync(newRows);

            await _context.SaveChangesAsync();
        }

        private async Task UpsertCustomerRefAsync(int enquiryId, CustomerRefDto? dto)
        {
            if (dto is null) return;

            var existing = await _context.EnquiryCustomerRef.FindAsync(enquiryId);
            if (existing is null)
            {
                _context.EnquiryCustomerRef.Add(new EnquiryCustomerRef
                {
                    enquiry_id = enquiryId,
                    mould_ownership = dto.mould_ownership
                });
            }
            else
            {
                existing.mould_ownership = dto.mould_ownership;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<int> ResolveCustomerIdAsync(
            EnquiryCreateDto dto,
            Enquiries? existingEnquiry = null)
        {
            if (dto.cust_id.HasValue)
            {
                return await _context.Customers.AnyAsync(c => c.cust_id == dto.cust_id.Value)
                    ? dto.cust_id.Value
                    : 0;
            }

            if (dto.new_customer is not null)
            {
                var existing = await _context.Customers
                    .FirstOrDefaultAsync(c =>
                        c.comp_name == dto.new_customer.comp_name && c.is_active);

                if (existing is not null) return existing.cust_id;

                var newCustomer = new Customers
                {
                    comp_name = dto.new_customer.comp_name,
                    cust_addr = dto.new_customer.cust_addr,
                    contact_name = dto.new_customer.contact_name,
                    contact_email = dto.new_customer.contact_email,
                    contact_phone = dto.new_customer.contact_phone,
                    created_at = DateTime.Now,
                    is_active = true
                };

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();
                return newCustomer.cust_id;
            }

            return existingEnquiry?.cust_id ?? 0;
        }

        // ── Mapping ───────────────────────────────────────────────────────────

        private static EnquiryResponseDto MapToResponseDto(Enquiries e)
        {
            var fieldValues = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            if (e.FieldValues != null)
            {
                foreach (var v in e.FieldValues)
                {
                    if (string.IsNullOrEmpty(v.section_key) || string.IsNullOrEmpty(v.field_key))
                        continue;

                    if (!fieldValues.ContainsKey(v.section_key))
                        fieldValues[v.section_key] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    fieldValues[v.section_key][v.field_key] = v.field_value ?? string.Empty;
                }
            }

            return new EnquiryResponseDto
            {
                enquiry_id = e.enquiry_id,
                enquiry_no = e.enquiry_no,
                proj_id = e.proj_id,
                cust_id = e.cust_id,
                customer_name = e.Customer?.comp_name,
                npi_category = e.npi_category,
                status = e.status,
                created_by = e.created_by,
                username = e.CreatedByUser?.username ?? "Unknown",
                created_at = e.created_at,
                updated_at = e.updated_at,
                updated_by = e.updated_by,
                submitted_at = e.submitted_at,
                field_values = fieldValues,
                CustomerRef = e.CustomerRef is not null
                    ? new CustomerRefResponseDto
                    {
                        enquiry_id = e.CustomerRef.enquiry_id,
                        mould_ownership = e.CustomerRef.mould_ownership
                    }
                    : null,
                Files = e.Files?.Select(f => new FileResponseDto
                {
                    file_id = f.file_id,
                    file_name = f.file_name,
                    file_path = f.file_path,
                    uploaded_at = f.created_at
                }).ToList()
            };
        }

        // ── Number generation ─────────────────────────────────────────────────

        public async Task<string> GenerateEnquiryNoAsync()
        {
            var year = DateTime.Now.Year;
            var prefix = $"ENQ-{year}-";

            var numbers = await _context.Enquiries
                .Where(e => e.enquiry_no.StartsWith(prefix))
                .Select(e => e.enquiry_no.Substring(prefix.Length))
                .ToListAsync();

            var last = numbers
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{(last + 1):D4}";
        }
    }
}