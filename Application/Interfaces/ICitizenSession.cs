
using eVote360.Core.Application.ViewModels.Citizen;

namespace eVote360.Core.Application.Interfaces
{
    public interface ICitizenSession
    {
        CitizenSession Get();
        void Set(CitizenSession session);
        void Remove();
    }
}
