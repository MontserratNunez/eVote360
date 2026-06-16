using eVote360.Core.Domain.Entities;

namespace eVote360.Core.Domain.Interfaces
{
    public interface ICitizenRepository : IGenericRepository<Citizen>
    {
        Task<bool> ExistsByDocumentAsync(string documentNumber, int idToIgnore = 0);
        Task<bool> ExistsByEmailAsync(string email, int idToIgnore = 0);
    }
}