using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface INpiFormConfigService
    {
        // ── Public endpoint ───────────────────────────────────────────────────
        Task<NpiFormConfigResponseDto> GetFormConfigAsync();

        // ── Categories ────────────────────────────────────────────────────────
        Task<List<NpiCategoryDto>> GetAllCategoriesAsync();
        Task<(bool Success, string Message, int Id)> CreateCategoryAsync(UpsertNpiCategoryDto dto);
        Task<(bool Success, string Message)> UpdateCategoryAsync(int id, UpsertNpiCategoryDto dto);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id);

        // ── Sections ──────────────────────────────────────────────────────────
        Task<List<NpiFormSectionDto>> GetAllSectionsAsync();
        Task<NpiFormSectionDto?> GetSectionByIdAsync(int id);
        Task<(bool Success, string Message, int Id)> CreateSectionAsync(CreateNpiFormSectionDto dto);
        Task<(bool Success, string Message)> UpdateSectionAsync(int id, UpdateNpiFormSectionDto dto);
        Task<(bool Success, string Message)> DeleteSectionAsync(int id);
        Task<(bool Success, string Message)> ToggleSectionStatusAsync(int id);
        Task<(bool Success, string Message)> ReorderSectionsAsync(List<int> orderedIds);

        // ── Fields ────────────────────────────────────────────────────────────
        Task<List<NpiFormFieldDto>> GetAllFieldsAsync();
        Task<(bool Success, string Message, int Id)> CreateFieldAsync(UpsertNpiFormFieldDto dto);
        Task<(bool Success, string Message)> UpdateFieldAsync(int id, UpsertNpiFormFieldDto dto);
        Task<(bool Success, string Message)> DeleteFieldAsync(int id);
    }
}