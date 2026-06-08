using eVote360.Core.Application.Interfaces;
using eVote360.Core.Application.ViewModels.Citizen;
using eVote360.Core.Domain.Entities;
using eVote360.Core.Domain.Interfaces;

namespace eVote360.Core.Application.Services
{
    public class CitizenService : ICitizenService
    {
        private readonly ICitizenRepository _citizenRepository;

        public CitizenService(ICitizenRepository citizenRepository)
        {
            _citizenRepository = citizenRepository;
        }

        public async Task<bool> ExistsByDocumentAsync(string document, int id = 0) =>
            await _citizenRepository.ExistsByDocumentAsync(document, id);

        public async Task<bool> ExistsByEmailAsync(string email, int id = 0) =>
            await _citizenRepository.ExistsByEmailAsync(email, id);

        public async Task<List<CitizenViewModel>> GetAllAsync()
        {
            var list = await _citizenRepository.GetAllList();
            return list.Select(c => new CitizenViewModel
            {
                Id = c.Id,
                Name = c.Name,
                LastName = c.LastName,
                Email = c.Email,
                DocumentNumber = c.DocumentNumber,
                Status = c.Status
            }).ToList();
        }

        public async Task<SaveCitizenViewModel?> GetByIdSaveViewModelAsync(int id)
        {
            var entity = await _citizenRepository.GetById(id);
            if (entity == null) return null;

            return new SaveCitizenViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                LastName = entity.LastName,
                Email = entity.Email,
                DocumentNumber = entity.DocumentNumber,
                Status = entity.Status
            };
        }

        public async Task<bool> AddAsync(SaveCitizenViewModel vm)
        {
            Citizen entity = new()
            {
                Name = vm.Name.Trim(),
                LastName = vm.LastName.Trim(),
                Email = vm.Email.Trim(),
                DocumentNumber = vm.DocumentNumber.Trim(),
                Status = vm.Status
            };
            await _citizenRepository.AddAsync(entity);
            return true;
        }

        public async Task<bool> UpdateAsync(SaveCitizenViewModel vm)
        {
            var entity = await _citizenRepository.GetById(vm.Id);
            if (entity == null) return false;

            entity.Name = vm.Name.Trim();
            entity.LastName = vm.LastName.Trim();
            entity.Email = vm.Email.Trim();
            entity.DocumentNumber = vm.DocumentNumber.Trim();
            entity.Status = vm.Status;

            await _citizenRepository.UpdateAsync(entity.Id, entity);
            return true;
        }

        public async Task<bool> ChangeStatusAsync(int id)
        {
            var entity = await _citizenRepository.GetById(id);
            if (entity == null) return false;

            entity.Status = !entity.Status;
            await _citizenRepository.UpdateAsync(entity.Id, entity);
            return true;
        }
    }
}