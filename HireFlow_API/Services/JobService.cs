using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using System.Security.Claims;

namespace HireFlow_API.Services
{
    public interface IJobService
    {
        Task<IEnumerable<JobDTO>> RetrieveAllJobsAsync();
        Task<JobDTO?> RetrieveJobByIdAsync(Guid jobId);
        Task<JobDTO> CreateNewJobAsync(CreateJobDTO newJobDto);
        Task<bool> UpdateJobDetailsAsync(Guid jobId, UpdateJobDTO updatedJobDto);
        Task<bool> DeleteJobAsync(Guid jobId);
    }

    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobService(IJobRepository jobRepository , IHttpContextAccessor httpContextAccessor)
        {
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
        }
 
        public async Task<IEnumerable<JobDTO>> RetrieveAllJobsAsync()
        {
            return await _jobRepository.RetrieveAllJobsAsync();
        }

        public async Task<JobDTO?> RetrieveJobByIdAsync(Guid jobId)
        {
            return await _jobRepository.RetrieveJobByIdAsync(jobId);
        }

        public async Task<JobDTO> CreateNewJobAsync(CreateJobDTO newJobDto)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdValue = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdValue))
                throw new UnauthorizedAccessException("User is not authenticated.");

            Guid postedBy = Guid.Parse(userIdValue); // Convert string → Guid

            var job = new Job
            {
                JobId = Guid.NewGuid(),
                JobTitle = newJobDto.JobTitle,
                JobDescription = newJobDto.JobDescription,
                Department = newJobDto.Department,
                Location = newJobDto.Location,
                Salary = newJobDto.Salary,
                EmploymentType = newJobDto.EmploymentType,
                Openings = newJobDto.Openings,
                PostedOn = DateTime.UtcNow,
                ClosingDate = newJobDto.ClosingDate,
                PostedBy = postedBy, 
                Skills = newJobDto.Skills,
                JobStatus = newJobDto.JobStatus
            };

            await _jobRepository.AddNewJobAsync(job);

            return new JobDTO
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,
                Department = job.Department,
                Location = job.Location,
                Salary = job.Salary,
                EmploymentType = job.EmploymentType,
                Openings = job.Openings,
                Skills = newJobDto.Skills,
                PostedOn = job.PostedOn,
                ClosingDate = job.ClosingDate,
                PostedBy = job.PostedBy,
                JobStatus = job.JobStatus
            };
        }


        public async Task<bool> UpdateJobDetailsAsync(Guid jobId, UpdateJobDTO updatedJobDto)
        {
            var job = new Job
            {
                JobId = jobId,
                JobTitle = updatedJobDto.JobTitle,
                JobDescription = updatedJobDto.JobDescription,
                Department = updatedJobDto.Department,
                Location = updatedJobDto.Location
            };

            return await _jobRepository.UpdateExistingJobAsync(job);
        }

        public async Task<bool> DeleteJobAsync(Guid jobId)
        {
            return await _jobRepository.RemoveJobByIdAsync(jobId);
        }
    }
}
