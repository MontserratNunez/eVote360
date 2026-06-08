using eVote360.Core.Application.ViewModels.Citizen;

namespace eVote360.Core.Application.Interfaces
{
    public interface ICitizenService
    {
        Task<SaveCitizenViewModel?> GetByIdSaveViewModelAsync(int id);
        Task<List<CitizenViewModel>> GetAllAsync();
        Task<bool> AddAsync(SaveCitizenViewModel vm);
        Task<bool> UpdateAsync(SaveCitizenViewModel vm);
        Task<bool> ChangeStatusAsync(int id);
        Task<bool> ExistsByDocumentAsync(string document, int id = 0);
        Task<bool> ExistsByEmailAsync(string email, int id = 0);
    }
}