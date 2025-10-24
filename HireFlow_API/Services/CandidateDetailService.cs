using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;

namespace HireFlow_API.Services
{
    public interface ICandidateDetailService
    {
        Task<JobApplicationDTO?> GetApplicationByIdAsync(Guid applicationId);
        Task<string> CreateCandidateAsync(UserAccount user);

        Task<bool> UpdateCandidateAsync(CandidateDetail updatedCandidate);
    }

    public class CandidateDetailService : ICandidateDetailService
    {
        private readonly ICandidateDetailRepository _candidateRepository;

        public CandidateDetailService(ICandidateDetailRepository candidateRepository)
        {
            _candidateRepository = candidateRepository;
        }

        /// <summary>
        /// Fetch Job Application details by Id (safe DTO response)
        /// </summary>
        public async Task<JobApplicationDTO?> GetApplicationByIdAsync(Guid applicationId)
        {
            var application = await _candidateRepository.GetApplicationByIdAsync(applicationId);

            if (application == null)
            {
                // You can either return null, or throw an exception depending on your design
                return null;
            }

            return application;
        }

        /// <summary>
        /// Creates a new candidate entry linked with UserAccount
        /// </summary>
        public async Task<string> CreateCandidateAsync(UserAccount user)
        {
            if (user == null || user.Id == Guid.Empty)
                return "Invalid User";

            var result = await _candidateRepository.CreateCandidateAsync(user);

            return result;
        }

        public async Task<List<CandidateCardDTO>> GetCandidateCardsAsync()
        {
            var candidates = await _candidateRepository.GetAllCandidateCardsAsync();
            return candidates;
        }

        public async Task<bool>  UpdateCandidateAsync(CandidateDetail updatedCandidate)
        {
            var candidates = await _candidateRepository.UpdateCandidateAsync(updatedCandidate);
            return candidates;
        }


    }
}
