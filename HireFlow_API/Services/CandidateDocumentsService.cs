using HireFlow_API.Model.DataModel;
using HireFlow_API.Repositories;

namespace HireFlow_API.Services
{

    interface ICandidateDocumentsService
    {
        Task<IEnumerable<CandidateDocumentDetail>> GetAllAsync();
        Task<CandidateDocumentDetail> GetByIdAsync(Guid id);
        Task AddAsync(CandidateDocumentDetail entity , bool isTrans = false);
        Task<bool> UpdateAsync(Guid id, CandidateDocumentDetail entity);
        Task<bool> DeleteAsync(Guid id);
    }

    class CandidateDocumentsService : ICandidateDocumentsService
    {
        private readonly ICandidateDocumentsRepository _repository;


        public CandidateDocumentsService(ICandidateDocumentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CandidateDocumentDetail>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<CandidateDocumentDetail> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(CandidateDocumentDetail entity, bool isTrans = false)
        {
            await _repository.AddAsync(entity, isTrans);
        }

        public async Task<bool> UpdateAsync(Guid id, CandidateDocumentDetail entity)
        {
            if (id != entity.DocumentDetailId)
                return false;

            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                return false;

            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
