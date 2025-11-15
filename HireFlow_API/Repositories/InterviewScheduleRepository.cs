using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HireFlow_API.Repositories
{

    public interface IInterviewScheduleRepository
    {
        Task<IEnumerable<InterviewScheduleDetail>> GetAllScheduledAsync();
        Task<IEnumerable<UpcomingInterviewDto>> GetUpcomingAsync();
        Task<IEnumerable<CompleteInterviewDto>> GetCompletedAsync();
        Task<InterviewScheduleDetail?> GetByIdAsync(Guid id);
        Task AddAsync(InterviewScheduleDetail interview);
        Task UpdateAsync(InterviewScheduleDetail interview);
        Task SaveChangesAsync();
    }


    public class InterviewScheduleRepository : IInterviewScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InterviewScheduleDetail>> GetAllScheduledAsync()
        {
            return await _context.InterviewScheduleDetails
                .Include(i => i.JobApplication)
                .OrderByDescending(i => i.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<UpcomingInterviewDto>> GetUpcomingAsync()
        {
            return await _context.InterviewScheduleDetails
                                 .Include(i => i.JobApplication)
                                 .ThenInclude(j => j.Job)
                                 .Include(i => i.JobApplication.Candidate)
                                 .ThenInclude(c => c.User)
                                 .Where(i => i.Status == "Scheduled" && i.ScheduledDate >= DateTime.Now)
                                 .OrderBy(i => i.ScheduledDate)
                                 .AsNoTracking()
                                 .Select(a => new UpcomingInterviewDto
                                 {
                                     interviewId = a.InterviewId,
                                     Candidate = a.JobApplication.Candidate.User.FullName,
                                     Position = a.JobApplication.Job.JobTitle,
                                     Interviewer = a.InterviewerName,
                                     Date = a.ScheduledDate,
                                     Status = a.Status,
                                     Type = a.InterviewMode

                                 }).ToListAsync();

        }

        public async Task<IEnumerable<CompleteInterviewDto>> GetCompletedAsync()
        {

            return await _context.InterviewScheduleDetails
                            .Include(i => i.JobApplication)
                            .ThenInclude(j => j.Job)
                            .Include(i => i.JobApplication.Candidate)
                            .ThenInclude(c => c.User)
                            .Where(i => i.Status == "Completed" || i.Status == "Cancelled")
                            .OrderBy(i => i.ScheduledDate)
                            .AsNoTracking()
                            .Select(a => new CompleteInterviewDto
                            {
                                Candidate = a.JobApplication.Candidate.User.FullName,
                                Position = a.JobApplication.Job.JobTitle,
                                Date = a.ScheduledDate,
                                Result = a.Status,
                            }).ToListAsync();

        }

        public async Task<InterviewScheduleDetail?> GetByIdAsync(Guid id)
        {
            return await _context.InterviewScheduleDetails
                .Include(i => i.JobApplication)
                .FirstOrDefaultAsync(i => i.InterviewId == id);
        }

        public async Task AddAsync(InterviewScheduleDetail interview)
        {
            await _context.InterviewScheduleDetails.AddAsync(interview);
        }

        public async Task UpdateAsync(InterviewScheduleDetail interview)
        {
            _context.InterviewScheduleDetails.Update(interview);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
