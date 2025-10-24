using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<JobService> _logger;

        public JobService(IJobRepository jobRepository,
                          IHttpContextAccessor httpContextAccessor,
                          ILogger<JobService> logger)
        {
            _jobRepository = jobRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

            Guid postedBy = Guid.Parse(userIdValue);

            var job = new Job
            {
                JobId = Guid.NewGuid(),
                JobTitle = newJobDto.JobTitle,
                JobSummary = newJobDto.JobSummary,
                JobDescription = newJobDto.JobDescription,
                Department = newJobDto.Department,
                Location = newJobDto.Location,
                Salary = newJobDto.Salary,
                EmploymentType = newJobDto.EmploymentType,
                Openings = newJobDto.Openings,
                PostedOn = DateTime.Now,
                ClosingDate = newJobDto.ClosingDate,
                PostedBy = postedBy,
                Skills = newJobDto.Skills,
                JobStatus = newJobDto.JobStatus
            };

            try
            {
                await _jobRepository.AddNewJobAsync(job);
                _logger.LogInformation("Job '{JobTitle}' created successfully by {UserId}", job.JobTitle, postedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating job '{JobTitle}' by {UserId}", job.JobTitle, postedBy);
                throw;
            }

            return new JobDTO
            {
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                JobSummary = job.JobSummary,
                JobDescription = job.JobDescription,
                Department = job.Department,
                Location = job.Location,
                Salary = job.Salary,
                EmploymentType = job.EmploymentType,
                Openings = job.Openings,
                Skills = job.Skills,
                PostedOn = job.PostedOn,
                ClosingDate = job.ClosingDate,
                PostedBy = job.PostedBy,
                JobStatus = job.JobStatus
            };
        }

        public async Task<bool> UpdateJobDetailsAsync(Guid jobId, UpdateJobDTO updatedJobDto)
        {
            var existingJob = await _jobRepository.RetrieveJobByIdAsync(jobId);

            if (existingJob == null)
            {
                _logger.LogWarning("Attempted to update non-existing job {JobId}", jobId);
                return false;
            }

            var job = new Job
            {
                JobId = jobId,
                JobTitle = updatedJobDto.JobTitle,
                JobSummary = updatedJobDto.JobSummary,
                JobDescription = updatedJobDto.JobDescription,
                Department = updatedJobDto.Department,
                Location = updatedJobDto.Location,
                Salary = updatedJobDto.Salary ?? existingJob.Salary,  // safe merge
                Skills = updatedJobDto.Skills ?? existingJob.Skills,
                EmploymentType = updatedJobDto.EmploymentType ?? existingJob.EmploymentType,
                Openings = updatedJobDto?.Openings ?? existingJob.Openings,
                ClosingDate = updatedJobDto.ClosingDate ?? existingJob.ClosingDate,
                JobStatus = updatedJobDto?.JobStatus ?? existingJob.JobStatus,
                PostedBy = existingJob.PostedBy,
                PostedOn = existingJob.PostedOn
            };

            var result = await _jobRepository.UpdateExistingJobAsync(job);

            if (result)
                _logger.LogInformation("Job {JobId} updated successfully", jobId);
            else
                _logger.LogWarning("Failed to update job {JobId}", jobId);

            return result;
        }

        public async Task<bool> DeleteJobAsync(Guid jobId)
        {
            var result = await _jobRepository.RemoveJobByIdAsync(jobId);

            if (result)
                _logger.LogInformation("Job {JobId} deleted successfully", jobId);
            else
                _logger.LogWarning("Attempted to delete non-existing job {JobId}", jobId);

            return result;
        }
    }
}
