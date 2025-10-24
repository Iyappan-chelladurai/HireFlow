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
                _logger.LogInformation("Fetching all jobs...");

                return await _context.Jobs
                    .AsNoTracking()
                    .Select(j => new JobDTO
                    {
                        JobId = j.JobId,
                        JobTitle = j.JobTitle,
                        JobSummary = j.JobSummary,
                        JobDescription = j.JobDescription,
                        Department = j.Department,
                        Location = j.Location,
                        Salary = j.Salary,
                        Skills = j.Skills,
                        EmploymentType = j.EmploymentType,
                        Openings = j.Openings,
                        PostedOn = j.PostedOn,
                        ClosingDate = j.ClosingDate,
                        PostedBy = j.PostedBy,
                        JobStatus = j.JobStatus,
                        CandidateCount = _context.JobApplications.Count(a => a.JobId == j.JobId)
                    }).OrderByDescending(A=>A.PostedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching all jobs");
                return Enumerable.Empty<JobDTO>();
            }
        }

        public async Task<JobDTO?> RetrieveJobByIdAsync(Guid jobId)
        {
            try
            {
                _logger.LogInformation("Fetching job with Id {JobId}", jobId);

                return await _context.Jobs
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching job with Id {JobId}", jobId);
                return null;
            }
        }

        public async Task AddNewJobAsync(Job job)
        {
            try
            {
                _logger.LogInformation("Adding new job {@Job}", job);

                await _context.Jobs.AddAsync(job);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding new job {@Job}", job);
                throw; // rethrow so higher layers know it failed
            }
        }

        public async Task<bool> UpdateExistingJobAsync(Job job)
        {
            try
            {
                if (!await _context.Jobs.AnyAsync(e => e.JobId == job.JobId))
                {
                    _logger.LogWarning("Job with Id {JobId} not found for update", job.JobId);
                    return false;
                }

                _logger.LogInformation("Updating job with Id {JobId}", job.JobId);

                _context.Entry(job).State = EntityState.Modified;
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
                return await _context.Jobs.AnyAsync(e => e.JobId == jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking existence of job {JobId}", jobId);
                return false;
            }
        }
    }
}
