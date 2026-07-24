using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IEnquiryService
    {
        Task<(bool Success, string Message, EnquiryResponseDto? Enquiry)> CreateEnquiryAsync(EnquiryCreateDto dto, int userId, string? ipAddress);
        Task<(bool Success, string Message)> UpdateEnquiryAsync(int enquiryId, EnquiryCreateDto dto, int userId, string userRole, bool isSales, string? ipAddress);
        Task<(bool Success, string Message)> SubmitEnquiryAsync(int enquiryId, int userId, string userRole, bool isSales, string? ipAddress);
        Task<(bool Success, string Message)> DeleteEnquiryAsync(int enquiryId, int userId, string userRole, string? ipAddress);
        Task<(bool Success, string Message)> ReviewEnquiryAsync(int enquiryId, string decision, string? remark, int userId, string userRole, string? ipAddress);
        Task<List<EnquiryReviewResponseDto>> GetReviewsAsync(int enquiryId);
        Task<List<EnquiryRevisionSnapshotDto>> GetRevisionSnapshotsAsync(int enquiryId);
        Task<EnquiryResponseDto?> GetEnquiryByIdAsync(int enquiryId);
        Task<List<EnquiryResponseDto>> GetAllEnquiriesAsync(bool canSeeDrafts);
        Task<List<EnquiryResponseDto>> GetEnquiriesByUserAsync(int userId);
        Task<string> GenerateEnquiryNoAsync();
    }

    public class EnquiryService : IEnquiryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notifications;

        public EnquiryService(
            ApplicationDbContext context,
            IAuditLogService audit,
            INotificationService notifications)
        {
            _context = context;
            _audit = audit;
            _notifications = notifications;
        }

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

        public async Task<List<EnquiryResponseDto>> GetAllEnquiriesAsync(bool canSeeDrafts)
        {
            var query = _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.FieldValues)
                .Include(e => e.CustomerRef)
                .Include(e => e.CreatedByUser)
                .AsQueryable();

            if (!canSeeDrafts)
                query = query.Where(e => e.status != EnquiryStatus.Draft);

            var enquiries = await query
                .OrderByDescending(e => e.created_at)
                .ToListAsync();

            var ids = enquiries.Select(e => e.enquiry_id).ToList();

            var latestReviews = await _context.EnquiryReviews
                .Where(r => ids.Contains(r.enquiry_id))
                .GroupBy(r => r.enquiry_id)
                .Select(g => g.OrderByDescending(r => r.created_at).First())
                .ToListAsync();

            var reviewMap = latestReviews.ToDictionary(r => r.enquiry_id);

            return enquiries.Select(e =>
            {
                var dto = MapToResponseDto(e);
                if (reviewMap.TryGetValue(e.enquiry_id, out var rv))
                {
                    dto.latest_review_decision = rv.decision;
                    dto.latest_review_remark = rv.remark;
                }
                return dto;
            }).ToList();
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

        public async Task<(bool Success, string Message, EnquiryResponseDto? Enquiry)> CreateEnquiryAsync(EnquiryCreateDto dto, int userId, string? ipAddress)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            (bool Success, string Message, int EnquiryId, object? Snapshot) outcome;

            try
            {
                outcome = await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();

                    await using var tx = await _context.Database.BeginTransactionAsync();

                    var customerId = await ResolveCustomerIdAsync(dto);
                    if (customerId == 0)
                    {
                        await tx.RollbackAsync();
                        return (false, "Customer not found or invalid data supplied.", 0, (object?)null);
                    }

                    var enquiry = new Enquiries
                    {
                        enquiry_no = await GenerateEnquiryNoAsync(),
                        cust_id = customerId,
                        form_category = dto.form_category,
                        status = EnquiryStatus.Draft,
                        created_by = userId,
                        created_at = DateTime.Now
                    };

                    _context.Enquiries.Add(enquiry);
                    await _context.SaveChangesAsync();

                    await SaveFieldValuesAsync(enquiry.enquiry_id, dto.field_values);
                    await UpsertCustomerRefAsync(enquiry.enquiry_id, dto.customer_ref);

                    await tx.CommitAsync();

                    return (true, "Enquiry created successfully.", enquiry.enquiry_id, (object?)new
                    {
                        enquiry.enquiry_no,
                        enquiry.cust_id,
                        enquiry.form_category,
                        enquiry.status
                    });
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync(userId, null, "CREATE_FAILED", "Enquiries", null,
                    null, new { error = ex.Message, inner = ex.InnerException?.Message }, ipAddress);

                return (false, "Could not create the enquiry. Please contact IT.", null);
            }

            if (!outcome.Success)
                return (false, outcome.Message, null);

            await _audit.LogAsync(userId, null, "CREATE", "Enquiries", outcome.EnquiryId,
                null, outcome.Snapshot, ipAddress);

            var response = await GetEnquiryByIdAsync(outcome.EnquiryId);
            return (true, outcome.Message, response);
        }

        public async Task<(bool Success, string Message)> UpdateEnquiryAsync(int enquiryId, EnquiryCreateDto dto, int userId, string userRole, bool isSales, string? ipAddress)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            (bool Success, string Message, bool Denied, object? Before, object? After) outcome;

            try
            {
                outcome = await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();

                    await using var tx = await _context.Database.BeginTransactionAsync();

                    var enquiry = await _context.Enquiries.FindAsync(enquiryId);
                    if (enquiry is null)
                    {
                        await tx.RollbackAsync();
                        return (false, "Enquiry not found.", false, (object?)null, (object?)null);
                    }

                    if (!CanEdit(enquiry, userId, userRole, isSales))
                    {
                        await tx.RollbackAsync();
                        return (false, "You are not authorised to edit this enquiry.", true, (object?)null, (object?)null);
                    }

                    if (enquiry.status != EnquiryStatus.Draft &&
                        enquiry.status != EnquiryStatus.NeedsRework)
                    {
                        await tx.RollbackAsync();
                        return (false, "Only Draft or Needs-Rework enquiries can be updated.", false, (object?)null, (object?)null);
                    }

                    var customerId = await ResolveCustomerIdAsync(dto, enquiry);
                    if (customerId == 0)
                    {
                        await tx.RollbackAsync();
                        return (false, "Customer not found.", false, (object?)null, (object?)null);
                    }

                    var before = (object?)new { enquiry.cust_id, enquiry.form_category };

                    enquiry.cust_id = customerId;
                    enquiry.form_category = dto.form_category;
                    enquiry.updated_at = DateTime.Now;
                    enquiry.updated_by = userId;

                    await _context.SaveChangesAsync();
                    await SaveFieldValuesAsync(enquiryId, dto.field_values);
                    await UpsertCustomerRefAsync(enquiryId, dto.customer_ref);

                    await tx.CommitAsync();

                    return (true, "Enquiry updated successfully.", false, before,
                        (object?)new { enquiry.cust_id, enquiry.form_category });
                });
            }
            catch (Exception ex)
            {
                await _audit.LogAsync(userId, null, "UPDATE_FAILED", "Enquiries", enquiryId,
                    null, new { error = ex.Message, inner = ex.InnerException?.Message }, ipAddress);

                return (false, "Could not update the enquiry. Please contact IT.");
            }

            if (outcome.Denied)
            {
                await _audit.LogAsync(userId, null, "UPDATE_DENIED", "Enquiries", enquiryId,
                    null, new { reason = "Not owner and not Admin/Manager" }, ipAddress);
            }
            else if (outcome.Success)
            {
                await _audit.LogAsync(userId, null, "UPDATE", "Enquiries", enquiryId,
                    outcome.Before, outcome.After, ipAddress);
            }

            return (outcome.Success, outcome.Message);
        }

        public async Task<(bool Success, string Message)> SubmitEnquiryAsync(int enquiryId, int userId, string userRole, bool isSales, string? ipAddress)
        {
            try
            {
                var enquiry = await _context.Enquiries.FindAsync(enquiryId);
                if (enquiry is null) return (false, "Enquiry not found.");

                if (!CanEdit(enquiry, userId, userRole, isSales))
                {
                    await _audit.LogAsync(userId, null, "SUBMIT_DENIED", "Enquiries", enquiryId,
                        null, new { reason = "Not owner, Sales, Admin, or Manager" }, ipAddress);
                    return (false, "You are not authorised to submit this enquiry.");
                }

                if (enquiry.status != EnquiryStatus.Draft &&
                    enquiry.status != EnquiryStatus.NeedsRework)
                    return (false, "Only Draft or Needs-Rework enquiries can be submitted.");

                var previousStatus = enquiry.status;

                if (previousStatus == EnquiryStatus.NeedsRework)
                    enquiry.revision_no += 1;

                enquiry.status = EnquiryStatus.Submitted;
                enquiry.submitted_at = DateTime.Now;
                await _context.SaveChangesAsync();

                await SaveRevisionSnapshotAsync(enquiry, userId);

                await _audit.LogAsync(userId, null, "SUBMIT", "Enquiries", enquiryId,
                    new { status = previousStatus, revision = enquiry.revision_no },
                    new { status = EnquiryStatus.Submitted, enquiry.submitted_at, revision = enquiry.revision_no },
                    ipAddress);

                await _notifications.OnEnquirySubmittedAsync(
                    enquiry.enquiry_id, enquiry.enquiry_no, userId);

                return (true, "Enquiry submitted successfully.");
            }
            catch (Exception ex)
            {
                await _audit.LogAsync(userId, null, "SUBMIT_FAILED", "Enquiries", enquiryId,
                    null, new { error = ex.Message }, ipAddress);

                return (false, "Could not submit the enquiry. Please contact IT.");
            }
        }

        public async Task<(bool Success, string Message)> DeleteEnquiryAsync(int enquiryId, int userId, string userRole, string? ipAddress)
        {
            try
            {
                var enquiry = await _context.Enquiries
                    .Include(e => e.FieldValues)
                    .Include(e => e.CustomerRef)
                    .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

                if (enquiry is null) return (false, "Enquiry not found.");

                if (!CanMutate(enquiry, userId, userRole))
                {
                    await _audit.LogAsync(userId, null, "DELETE_DENIED", "Enquiries", enquiryId,
                        null, new { reason = "Not owner and not Admin/Manager" }, ipAddress);
                    return (false, "You are not authorised to delete this enquiry.");
                }

                if (enquiry.proj_id is not null)
                    return (false, "This enquiry has been launched as a project and cannot be deleted.");

                var snapshot = new
                {
                    enquiry.enquiry_no,
                    enquiry.cust_id,
                    enquiry.form_category,
                    enquiry.status,
                    enquiry.created_by
                };

                var files = await _context.Files
                    .Where(f => f.enquiry_id == enquiryId)
                    .ToListAsync();

                var reviews = await _context.EnquiryReviews
                    .Where(r => r.enquiry_id == enquiryId)
                    .ToListAsync();

                var snapshots = await _context.EnquiryRevisionSnapshots
                    .Where(s => s.enquiry_id == enquiryId)
                    .ToListAsync();

                var paths = files
                    .Where(f => !string.IsNullOrWhiteSpace(f.file_path))
                    .Select(f => f.file_path!)
                    .ToList();

                if (files.Count > 0) _context.Files.RemoveRange(files);
                if (reviews.Count > 0) _context.EnquiryReviews.RemoveRange(reviews);
                if (snapshots.Count > 0) _context.EnquiryRevisionSnapshots.RemoveRange(snapshots);

                _context.Enquiries.Remove(enquiry);
                await _context.SaveChangesAsync();

                foreach (var path in paths)
                {
                    try
                    {
                        if (File.Exists(path)) File.Delete(path);
                    }
                    catch { }
                }

                await _audit.LogAsync(userId, null, "DELETE", "Enquiries", enquiryId,
                    snapshot, new { deleted_files = files.Count }, ipAddress);

                return (true, "Enquiry deleted successfully.");
            }
            catch (Exception ex)
            {
                await _audit.LogAsync(userId, null, "DELETE_FAILED", "Enquiries", enquiryId,
                    null, new { error = ex.Message, inner = ex.InnerException?.Message }, ipAddress);

                return (false, "Could not delete the enquiry. Please contact IT.");
            }
        }

        public async Task<(bool Success, string Message)> ReviewEnquiryAsync(
            int enquiryId, string decision, string? remark, int userId, string userRole, string? ipAddress)
        {
            if (!RbacHelper.IsAdminOrManager(userRole))
            {
                await _audit.LogAsync(userId, null, "REVIEW_DENIED", "Enquiries", enquiryId,
                    null, new { reason = "Not Admin/Manager" }, ipAddress);
                return (false, "You are not authorised to review enquiries.");
            }

            var enquiry = await _context.Enquiries.FindAsync(enquiryId);
            if (enquiry is null) return (false, "Enquiry not found.");

            if (enquiry.status != EnquiryStatus.Submitted)
                return (false, "Only submitted enquiries can be reviewed.");

            string newStatus = decision switch
            {
                EnquiryReviewDecision.NeedsRework => EnquiryStatus.NeedsRework,
                EnquiryReviewDecision.NotFeasible => EnquiryStatus.NotFeasible,
                _ => string.Empty
            };
            if (newStatus.Length == 0)
                return (false, "Invalid review decision.");

            var previousStatus = enquiry.status;

            _context.EnquiryReviews.Add(new EnquiryReviews
            {
                enquiry_id = enquiryId,
                revision_no = enquiry.revision_no,
                reviewed_by = userId,
                decision = decision,
                remark = remark?.Trim(),
                created_at = DateTime.Now
            });

            enquiry.status = newStatus;
            enquiry.updated_at = DateTime.Now;
            enquiry.updated_by = userId;
            await _context.SaveChangesAsync();

            await _notifications.OnEnquiryReviewedAsync(
                enquiryId, enquiry.created_by, enquiry.enquiry_no, decision, remark);

            await _audit.LogAsync(userId, null, "REVIEW", "Enquiries", enquiryId,
                new { status = previousStatus },
                new { status = newStatus, decision, enquiry.revision_no }, ipAddress);

            return (true, $"Enquiry marked '{newStatus}'.");
        }

        public async Task<List<EnquiryReviewResponseDto>> GetReviewsAsync(int enquiryId)
        {
            return await _context.EnquiryReviews
                .Where(r => r.enquiry_id == enquiryId)
                .OrderByDescending(r => r.created_at)
                .Select(r => new EnquiryReviewResponseDto
                {
                    review_id = r.review_id,
                    enquiry_id = r.enquiry_id,
                    revision_no = r.revision_no,
                    reviewed_by = r.reviewed_by,
                    reviewer_name = r.ReviewedByUser!.username,
                    decision = r.decision,
                    remark = r.remark,
                    created_at = r.created_at
                })
                .ToListAsync();
        }

        public async Task<List<EnquiryRevisionSnapshotDto>> GetRevisionSnapshotsAsync(int enquiryId)
        {
            var rows = await _context.EnquiryRevisionSnapshots
                .Where(s => s.enquiry_id == enquiryId)
                .OrderBy(s => s.revision_no)
                .Select(s => new
                {
                    s.snapshot_id,
                    s.revision_no,
                    s.cust_id,
                    s.form_category,
                    s.field_values_json,
                    s.submitted_by,
                    name = s.SubmittedByUser!.username,
                    s.created_at
                })
                .ToListAsync();

            return rows.Select(s => new EnquiryRevisionSnapshotDto
            {
                snapshot_id = s.snapshot_id,
                revision_no = s.revision_no,
                cust_id = s.cust_id,
                form_category = s.form_category,
                field_values = System.Text.Json.JsonSerializer
                    .Deserialize<Dictionary<string, Dictionary<string, string?>>>(s.field_values_json)
                    ?? new(),
                submitted_by = s.submitted_by,
                submitted_by_name = s.name,
                created_at = s.created_at
            }).ToList();
        }

        private async Task SaveRevisionSnapshotAsync(Enquiries enquiry, int userId)
        {
            var values = await _context.EnquiryFieldValues
                .Where(v => v.enquiry_id == enquiry.enquiry_id)
                .ToListAsync();

            var grouped = values
                .GroupBy(v => v.section_key)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(v => v.field_key, v => v.field_value));

            _context.EnquiryRevisionSnapshots.Add(new EnquiryRevisionSnapshots
            {
                enquiry_id = enquiry.enquiry_id,
                revision_no = enquiry.revision_no,
                cust_id = enquiry.cust_id,
                form_category = enquiry.form_category,
                field_values_json = System.Text.Json.JsonSerializer.Serialize(grouped),
                submitted_by = userId,
                created_at = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        private async Task SaveFieldValuesAsync(int enquiryId, Dictionary<string, Dictionary<string, string?>>? sections)
        {
            if (sections is null || sections.Count == 0) return;

            var sectionKeys = sections.Keys.ToList();

            var toRemove = await _context.EnquiryFieldValues
                .Where(v => v.enquiry_id == enquiryId
                         && EF.Constant(sectionKeys).Contains(v.section_key))
                .ToListAsync();

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

        private async Task<int> ResolveCustomerIdAsync(EnquiryCreateDto dto, Enquiries? existingEnquiry = null)
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
                    created_at = DateTime.Now,
                    is_active = true
                };

                _context.Customers.Add(newCustomer);
                await _context.SaveChangesAsync();
                return newCustomer.cust_id;
            }

            return existingEnquiry?.cust_id ?? 0;
        }

        private static bool CanMutate(Enquiries enquiry, int userId, string userRole) =>
            RbacHelper.IsAdminOrManager(userRole) || enquiry.created_by == userId;

        private static bool CanEdit(Enquiries enquiry, int userId, string userRole, bool isSales) =>
            RbacHelper.IsAdminOrManager(userRole) || isSales || enquiry.created_by == userId;

        private static EnquiryResponseDto MapToResponseDto(Enquiries e)
        {
            var fieldValues = new Dictionary<string, Dictionary<string, string?>>(StringComparer.OrdinalIgnoreCase);

            if (e.FieldValues != null)
            {
                foreach (var v in e.FieldValues)
                {
                    if (string.IsNullOrEmpty(v.section_key) || string.IsNullOrEmpty(v.field_key))
                        continue;

                    if (!fieldValues.ContainsKey(v.section_key))
                        fieldValues[v.section_key] = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

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
                form_category = e.form_category,
                status = e.status,
                created_by = e.created_by,
                username = e.CreatedByUser?.username ?? "Unknown",
                created_at = e.created_at,
                updated_at = e.updated_at,
                updated_by = e.updated_by,
                submitted_at = e.submitted_at,
                revision_no = e.revision_no,
                field_values = fieldValues,
                customer_ref = e.CustomerRef is not null
                    ? new CustomerRefResponseDto
                    {
                        enquiry_id = e.CustomerRef.enquiry_id,
                        mould_ownership = e.CustomerRef.mould_ownership
                    }
                    : null,
                files = e.Files?
                    .Where(f => f.status != FileStatus.Deleted)
                    .Select(f => new FileResponseDto
                    {
                        file_id = f.file_id,
                        file_name = f.file_name,
                        file_path = f.file_path,
                        file_size = f.file_size,
                        description = f.description,
                        uploaded_at = f.created_at
                    }).ToList()
            };
        }

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