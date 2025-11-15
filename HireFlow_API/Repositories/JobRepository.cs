using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HireFlow_API.Repositories
{
    public interface IJobRepository
    {
        Task<IEnumerable<JobDTO>> RetrieveAllJobsAsync();
        Task<JobDTO?> RetrieveJobByIdAsync(Guid jobId);
        Task AddNewJobAsync(Job job);
        Task<bool> UpdateExistingJobAsync(Job job);
        Task<bool> RemoveJobByIdAsync(Guid jobId);
        Task<bool> CheckJobExistsAsync(Guid jobId);
    }

    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(ApplicationDbContext context, ILogger<JobRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<JobDTO>> RetrieveAllJobsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all jobs with filtering criteria...");

                // Define a constant or enum for JobStatus
                const int ClosedJobStatus = 2;

                // Fetch jobs with candidate counts in a single query
                var jobs = await _context.Jobs
                    .Where(j => (j.JobStatus == ClosedJobStatus && j.ClosingDate <= DateTime.Now.AddDays(-1)) || j.JobStatus != ClosedJobStatus)
                    .AsNoTracking()
                    .Select(j => new
                    {
                        Job = j,
                        CandidateCount = _context.JobApplications.Count(a => a.JobId == j.JobId)
                    })
                    .OrderByDescending(j => j.Job.ClosingDate)
                    .ToListAsync();

                // Map to DTOs
                return jobs.Select(j => new JobDTO
                {
                    JobId = j.Job.JobId,
                    JobTitle = j.Job.JobTitle,
                    JobSummary = j.Job.JobSummary,
                    JobDescription = j.Job.JobDescription,
                    Department = j.Job.Department,
                    Location = j.Job.Location,
                    Salary = j.Job.Salary,
                    Skills = j.Job.Skills,
                    EmploymentType = j.Job.EmploymentType,
                    Openings = j.Job.Openings,
                    PostedOn = j.Job.PostedOn,
                    ClosingDate = j.Job.ClosingDate,
                    PostedBy = j.Job.PostedBy,
                    JobStatus = j.Job.JobStatus,
                    CandidateCount = j.CandidateCount
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all jobs");
                throw; // Rethrow the exception to propagate the error
            }
        }

        public async Task<JobDTO?> RetrieveJobByIdAsync(Guid jobId)
        {
            try
            {
                _logger.LogInformation("Fetching job with Id {JobId}", jobId);

                _context.Dispose();
                using (var context = _context)
                {
                    return await context.Jobs
                   .AsNoTracking()
                   .Where(j => j.JobId == jobId)
                   .Select(j => new JobDTO
                   {
                       JobId = j.JobId,
                       JobTitle = j.JobTitle,
                       JobSummary = j.JobSummary,
                       JobDescription = j.JobDescription,
                       Department = j.Department,
                       Location = j.Location
                   })
                   .FirstOrDefaultAsync();
                }

               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching job with Id {JobId}", jobId);
                return null;
            }
        }

        public async Task AddNewJobAsync(Job job)
        {
            if (job == null)
            {
                _logger.LogWarning("Attempted to add a null job.");
                throw new ArgumentNullException(nameof(job), "Job cannot be null.");
            }

            try
            {
                _logger.LogInformation("Adding new job with Id {JobId}", job.JobId);

                await _context.Jobs.AddAsync(job);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding new job with Id {JobId}", job.JobId);
                throw; // Rethrow to propagate the error
            }
        }

        public async Task<bool> UpdateExistingJobAsync(Job job)
        {
            if (job == null)
            {
                _logger.LogWarning("Attempted to update a null job.");
                throw new ArgumentNullException(nameof(job), "Job cannot be null.");
            }

            try
            {
                if (!await _context.Jobs.AsNoTracking().AnyAsync(e => e.JobId == job.JobId))
                {
                    _logger.LogWarning("Job with Id {JobId} not found for update", job.JobId);
                    return false;
                }

                _logger.LogInformation("Updating job with Id {JobId}", job.JobId);

                _context.Jobs.Update(job);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating job {@Job}", job);
                return false;
            }
        }

        public async Task<bool> RemoveJobByIdAsync(Guid jobId)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(jobId);
                if (job == null)
                {
                    _logger.LogWarning("Job with Id {JobId} not found for deletion", jobId);
                    return false;
                }

                _logger.LogInformation("Deleting job with Id {JobId}", jobId);

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while removing job with Id {JobId}", jobId);
                return false;
            }
        }

        public async Task<bool> CheckJobExistsAsync(Guid jobId)
        {
            try
            {
                _logger.LogInformation("Checking if job with Id {JobId} exists", jobId);
                return await _context.Jobs.AsNoTracking().AnyAsync(e => e.JobId == jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking existence of job {JobId}", jobId);
                return false;
            }
        }
    }
}
