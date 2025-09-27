using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using Microsoft.EntityFrameworkCore;

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

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobDTO>> RetrieveAllJobsAsync()
        {
   return await _context.Jobs
    .AsNoTracking()
    .Select(j => new JobDTO
    {
        JobId = j.JobId,
        JobTitle = j.JobTitle,
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
    })
    .ToListAsync();

        }


        public async Task<JobDTO?> RetrieveJobByIdAsync(Guid jobId)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Where(j => j.JobId == jobId)
                .Select(j => new JobDTO
                {
                    JobId = j.JobId,
                    JobTitle = j.JobTitle,
                    JobDescription = j.JobDescription,
                    Department = j.Department,
                    Location = j.Location
                })
                .FirstOrDefaultAsync();
        }

        public async Task AddNewJobAsync(Job job)
        {
            try
            {
                await _context.Jobs.AddAsync(job);
                await _context.SaveChangesAsync();
            }
            catch (Exception EX)
            { 
            
            }

        }

        public async Task<bool> UpdateExistingJobAsync(Job job)
        {
            var exists = await _context.Jobs.AnyAsync(e => e.JobId == job.JobId);
            if (!exists) return false;

            _context.Entry(job).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveJobByIdAsync(Guid jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckJobExistsAsync(Guid jobId)
        {
            return await _context.Jobs.AnyAsync(e => e.JobId == jobId);
        }
    }
}
