using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
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

        public async Task<(bool Success, string Message, EnquiryResponseDto Enquiry)> CreateEnquiryAsync(EnquiryCreateDto dto, int userId)
        {
            try
            {
                int customerId;

                if (dto.cust_id == null && dto.new_customer != null)
                {
                    var existingCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.comp_name == dto.new_customer.comp_name && c.is_active);

                    if (existingCustomer != null)
                    {
                        customerId = existingCustomer.cust_id;
                    }
                    else
                    {
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

                        customerId = newCustomer.cust_id;
                    }
                }
                else if (dto.cust_id.HasValue)
                {
                    customerId = dto.cust_id.Value;

                    if (!await _context.Customers.AnyAsync(c => c.cust_id == customerId))
                    {
                        return (false, "Customer not found", null);
                    }
                }
                else
                {
                    return (false, "Either cust_id or new_customer must be provided", null);
                }

                var enquiry = new Enquiries
                {
                    enquiry_no = await GenerateEnquiryNo(),
                    cust_id = customerId,
                    npi_category = dto.npi_category,
                    status = "Draft",
                    created_by = userId,
                    created_at = DateTime.Now
                };

                _context.Enquiries.Add(enquiry);
                await _context.SaveChangesAsync();

                if (dto.GeneralInfo != null)
                {
                    var generalInfo = new EnquiryGeneralInfo
                    {
                        enquiry_id = enquiry.enquiry_id,
                        company_name = dto.GeneralInfo.company_name,
                        estimated_qty_per_year = dto.GeneralInfo.estimated_qty_per_year,
                        estimated_required_date = dto.GeneralInfo.estimated_required_date,
                        color = dto.GeneralInfo.color,
                        material_used = dto.GeneralInfo.material_used,
                        weight_g = dto.GeneralInfo.weight_g,
                        neck_size_mm = dto.GeneralInfo.neck_size_mm,
                        shape = dto.GeneralInfo.shape,
                        hot_cold_filling = dto.GeneralInfo.hot_cold_filling,
                        qty_first_submission = dto.GeneralInfo.qty_first_submission,
                        cap_bottle_source = dto.GeneralInfo.cap_bottle_source,
                        filling_content = dto.GeneralInfo.filling_content,
                        capping_method = dto.GeneralInfo.capping_method,
                        cap_seal = dto.GeneralInfo.cap_seal
                    };
                    _context.EnquiryGeneralInfo.Add(generalInfo);
                }

                if (dto.SealInfo != null)
                {
                    var sealInfo = new EnquirySealInfo
                    {
                        enquiry_id = enquiry.enquiry_id,
                        customer_name = dto.SealInfo.customer_name,
                        apply_to_product = dto.SealInfo.apply_to_product,
                        estimated_required_date = dto.SealInfo.estimated_required_date,
                        reason_of_change = dto.SealInfo.reason_of_change,
                        qty_first_submission = dto.SealInfo.qty_first_submission,
                        other_requirements = dto.SealInfo.other_requirements
                    };
                    _context.EnquirySealInfo.Add(sealInfo);
                }

                if (dto.CustomerRef != null)
                {
                    var customerRef = new EnquiryCustomerRef
                    {
                        enquiry_id = enquiry.enquiry_id,
                        mould_ownership = dto.CustomerRef.mould_ownership
                    };
                    _context.EnquiryCustomerRef.Add(customerRef);
                }

                await _context.SaveChangesAsync();

                var responseDto = await GetEnquiryByIdAsync(enquiry.enquiry_id);
                return (true, "Enquiry created successfully", responseDto);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating enquiry: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateEnquiryAsync(int enquiryId, EnquiryCreateDto dto, int userId)
        {
            try
            {
                var enquiry = await _context.Enquiries
                    .Include(e => e.Customer)
                    .Include(e => e.GeneralInfo)
                    .Include(e => e.SealInfo)
                    .Include(e => e.CustomerRef)
                    .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

                if (enquiry == null)
                    return (false, "Enquiry not found");

                if (enquiry.created_by != userId)
                    return (false, "Unauthorized to update this enquiry");

                if (enquiry.status != "Draft")
                    return (false, "Can only update draft enquiries");

                if (dto.cust_id.HasValue)
                {
                    if (enquiry.cust_id != dto.cust_id.Value)
                    {
                        if (!await _context.Customers.AnyAsync(c => c.cust_id == dto.cust_id.Value))
                        {
                            return (false, "Selected customer not found");
                        }
                        enquiry.cust_id = dto.cust_id.Value;
                    }
                }
                else if (dto.new_customer != null)
                {
                    var existingCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.comp_name == dto.new_customer.comp_name && c.is_active);

                    if (existingCustomer != null)
                    {
                        enquiry.cust_id = existingCustomer.cust_id;
                    }
                    else
                    {
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

                        enquiry.cust_id = newCustomer.cust_id;
                    }
                }
                else
                {
                    return (false, "Either cust_id or new_customer must be provided");
                }

                enquiry.npi_category = dto.npi_category;
                enquiry.updated_at = DateTime.Now;
                enquiry.updated_by = userId;

                if (dto.GeneralInfo != null)
                {
                    if (enquiry.GeneralInfo == null)
                    {
                        enquiry.GeneralInfo = new EnquiryGeneralInfo { enquiry_id = enquiryId };
                        _context.EnquiryGeneralInfo.Add(enquiry.GeneralInfo);
                    }

                    enquiry.GeneralInfo.company_name = dto.GeneralInfo.company_name;
                    enquiry.GeneralInfo.estimated_qty_per_year = dto.GeneralInfo.estimated_qty_per_year;
                    enquiry.GeneralInfo.estimated_required_date = dto.GeneralInfo.estimated_required_date;
                    enquiry.GeneralInfo.color = dto.GeneralInfo.color;
                    enquiry.GeneralInfo.material_used = dto.GeneralInfo.material_used;
                    enquiry.GeneralInfo.weight_g = dto.GeneralInfo.weight_g;
                    enquiry.GeneralInfo.neck_size_mm = dto.GeneralInfo.neck_size_mm;
                    enquiry.GeneralInfo.shape = dto.GeneralInfo.shape;
                    enquiry.GeneralInfo.hot_cold_filling = dto.GeneralInfo.hot_cold_filling;
                    enquiry.GeneralInfo.qty_first_submission = dto.GeneralInfo.qty_first_submission;
                    enquiry.GeneralInfo.cap_bottle_source = dto.GeneralInfo.cap_bottle_source;
                    enquiry.GeneralInfo.filling_content = dto.GeneralInfo.filling_content;
                    enquiry.GeneralInfo.capping_method = dto.GeneralInfo.capping_method;
                    enquiry.GeneralInfo.cap_seal = dto.GeneralInfo.cap_seal;
                }

                if (dto.SealInfo != null)
                {
                    if (enquiry.SealInfo == null)
                    {
                        enquiry.SealInfo = new EnquirySealInfo { enquiry_id = enquiryId };
                        _context.EnquirySealInfo.Add(enquiry.SealInfo);
                    }

                    enquiry.SealInfo.customer_name = dto.SealInfo.customer_name;
                    enquiry.SealInfo.apply_to_product = dto.SealInfo.apply_to_product;
                    enquiry.SealInfo.estimated_required_date = dto.SealInfo.estimated_required_date;
                    enquiry.SealInfo.reason_of_change = dto.SealInfo.reason_of_change;
                    enquiry.SealInfo.qty_first_submission = dto.SealInfo.qty_first_submission;
                    enquiry.SealInfo.other_requirements = dto.SealInfo.other_requirements;
                }

                if (dto.CustomerRef != null)
                {
                    if (enquiry.CustomerRef == null)
                    {
                        enquiry.CustomerRef = new EnquiryCustomerRef { enquiry_id = enquiryId };
                        _context.EnquiryCustomerRef.Add(enquiry.CustomerRef);
                    }

                    enquiry.CustomerRef.mould_ownership = dto.CustomerRef.mould_ownership;
                }

                await _context.SaveChangesAsync();
                return (true, "Enquiry updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating enquiry: {ex.Message}");
            }
        }

        public async Task<EnquiryResponseDto> GetEnquiryByIdAsync(int enquiryId)
        {
            var enquiry = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.GeneralInfo)
                .Include(e => e.SealInfo)
                .Include(e => e.CustomerRef)
                .Include(e => e.Files)
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

            if (enquiry == null) return null;

            return MapToResponseDto(enquiry);
        }

        public async Task<List<EnquiryResponseDto>> GetAllEnquiriesAsync()
        {
            var enquiries = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.GeneralInfo)
                .Include(e => e.SealInfo)
                .Include(e => e.CustomerRef)
                .Include(e => e.Files)
                .Include(e => e.CreatedByUser)
                .OrderByDescending(e => e.created_at)
                .ToListAsync();

            return enquiries.Select(MapToResponseDto).ToList();
        }

        public async Task<List<EnquiryResponseDto>> GetEnquiriesByUserAsync(int userId)
        {
            var enquiries = await _context.Enquiries
                .Include(e => e.Customer)
                .Include(e => e.GeneralInfo)
                .Include(e => e.SealInfo)
                .Include(e => e.CustomerRef)
                .Include(e => e.Files)
                .Include(e => e.CreatedByUser)
                .Where(e => e.created_by == userId)
                .OrderByDescending(e => e.created_at)
                .ToListAsync();

            return enquiries.Select(MapToResponseDto).ToList();
        }

        public async Task<(bool Success, string Message)> SubmitEnquiryAsync(int enquiryId, int userId)
        {
            try
            {
                var enquiry = await _context.Enquiries.FindAsync(enquiryId);

                if (enquiry == null)
                    return (false, "Enquiry not found");

                if (enquiry.created_by != userId)
                    return (false, "Unauthorized to submit this enquiry");

                if (enquiry.status != "Draft")
                    return (false, "Only draft enquiries can be submitted");

                enquiry.status = "Submitted";
                enquiry.submitted_at = DateTime.Now;

                await _context.SaveChangesAsync();
                return (true, "Enquiry submitted successfully");
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
                    .Include(e => e.GeneralInfo)
                    .Include(e => e.SealInfo)
                    .Include(e => e.CustomerRef)
                    .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

                if (enquiry == null)
                    return (false, "Enquiry not found");

                _context.Enquiries.Remove(enquiry);
                await _context.SaveChangesAsync();

                return (true, "Enquiry deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting enquiry: {ex.Message}");
            }
        }

        public async Task<string> GenerateEnquiryNo()
        {
            var year = DateTime.Now.Year;
            var prefix = $"ENQ-{year}-";

            var lastNumber = await _context.Enquiries
                .Where(e => e.enquiry_no.StartsWith(prefix))
                .Select(e => e.enquiry_no.Substring(prefix.Length))
                .ToListAsync()
                .ContinueWith(t => t.Result
                    .Select(s => int.TryParse(s, out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max());

            return $"{prefix}{(lastNumber + 1):D4}";
        }

        private EnquiryResponseDto MapToResponseDto(Enquiries enquiry)
        {
            return new EnquiryResponseDto
            {
                enquiry_id = enquiry.enquiry_id,
                enquiry_no = enquiry.enquiry_no,
                proj_id = enquiry.proj_id,
                cust_id = enquiry.cust_id,
                customer_name = enquiry.Customer?.comp_name,
                npi_category = enquiry.npi_category,
                status = enquiry.status,
                created_by = enquiry.created_by,
                username = enquiry.CreatedByUser?.username ?? "Unknown",
                created_at = enquiry.created_at,
                updated_by = enquiry.updated_by,
                submitted_at = enquiry.submitted_at,
                GeneralInfo = enquiry.GeneralInfo != null ? new GeneralInfoResponseDto
                {
                    general_info_id = enquiry.GeneralInfo.enquiry_id,
                    company_name = enquiry.GeneralInfo.company_name,
                    estimated_qty_per_year = enquiry.GeneralInfo.estimated_qty_per_year,
                    estimated_required_date = enquiry.GeneralInfo.estimated_required_date,
                    color = enquiry.GeneralInfo.color,
                    material_used = enquiry.GeneralInfo.material_used,
                    weight_g = enquiry.GeneralInfo.weight_g,
                    neck_size_mm = enquiry.GeneralInfo.neck_size_mm,
                    shape = enquiry.GeneralInfo.shape,
                    hot_cold_filling = enquiry.GeneralInfo.hot_cold_filling,
                    qty_first_submission = enquiry.GeneralInfo.qty_first_submission,
                    cap_bottle_source = enquiry.GeneralInfo.cap_bottle_source,
                    filling_content = enquiry.GeneralInfo.filling_content,
                    capping_method = enquiry.GeneralInfo.capping_method,
                    cap_seal = enquiry.GeneralInfo.cap_seal
                } : null,
                SealInfo = enquiry.SealInfo != null ? new SealInfoResponseDto
                {
                    seal_info_id = enquiry.SealInfo.enquiry_id,
                    customer_name = enquiry.SealInfo.customer_name,
                    apply_to_product = enquiry.SealInfo.apply_to_product,
                    estimated_required_date = enquiry.SealInfo.estimated_required_date,
                    reason_of_change = enquiry.SealInfo.reason_of_change,
                    qty_first_submission = enquiry.SealInfo.qty_first_submission,
                    other_requirements = enquiry.SealInfo.other_requirements
                } : null,
                CustomerRef = enquiry.CustomerRef != null ? new CustomerRefResponseDto
                {
                    customer_ref_id = enquiry.CustomerRef.enquiry_id,
                    mould_ownership = enquiry.CustomerRef.mould_ownership
                } : null,
                Files = enquiry.Files?.Select(f => new FileResponseDto
                {
                    file_id = f.file_id,
                    file_name = f.file_name,
                    file_path = f.file_path,
                    uploaded_at = f.created_at
                }).ToList()
            };
        }
    }
}