using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;

namespace HireFlow_API.Services
{
    public interface IJobApplicationService
    {
        Task<IEnumerable<JobApplicationDTO>> RetrieveAllApplicationsAsync();
        Task<JobApplicationDTO?> RetrieveApplicationDetailsAsync(Guid applicationId);
        Task<JobApplicationDTO> SubmitApplicationAsync(JobApplicationDTO jobApplication);
        Task<bool> UpdateExistingApplicationAsync(Guid applicationId, JobApplicationDTO jobApplication);
        Task<bool> CancelApplicationAsync(Guid applicationId);
    }

    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _repository;

        public JobApplicationService(IJobApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<JobApplicationDTO>> RetrieveAllApplicationsAsync()
        {
            return await _repository.GetAllApplicationsAsync();
        }

        public async Task<JobApplicationDTO?> RetrieveApplicationDetailsAsync(Guid applicationId)
        {
            return await _repository.GetApplicationByIdAsync(applicationId);
        }

        public async Task<JobApplicationDTO> SubmitApplicationAsync(JobApplicationDTO jobApplication)
        {
            
            var uploadsFolder = Path.Combine("wwwroot", "resumes"); // Example: save resume to server or cloud storage
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, Guid.NewGuid() + Path.GetExtension(jobApplication.ResumeFile.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await jobApplication.ResumeFile.CopyToAsync(stream);
            }

            // Optionally store the file path in DTO
            jobApplication.ResumePath = filePath;

            await _repository.AddNewApplicationAsync(jobApplication);
            return jobApplication;
        }

        public async Task<bool> UpdateExistingApplicationAsync(Guid applicationId, JobApplicationDTO jobApplication)
        {
            if (applicationId != jobApplication.ApplicationId)
                return false;

            if (!await _repository.IsApplicationExistsAsync(applicationId))
                return false;

            await _repository.UpdateApplicationInfoAsync(jobApplication);
            return true;
        }

        public async Task<bool> CancelApplicationAsync(Guid applicationId)
        {
            if (!await _repository.IsApplicationExistsAsync(applicationId))
                return false;

            await _repository.DeleteApplicationByIdAsync(applicationId);
            return true;
        }
    }
}
