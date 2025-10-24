using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using Microsoft.EntityFrameworkCore;

namespace HireFlow_API.Repositories
{

    public interface IInterviewScheduleRepository
    {
        Task<IEnumerable<InterviewScheduleDetail>> GetAllScheduledAsync();
        Task<IEnumerable<InterviewScheduleDetail>> GetUpcomingAsync();
        Task<IEnumerable<InterviewScheduleDetail>> GetCompletedAsync();
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

        public async Task<IEnumerable<InterviewScheduleDetail>> GetUpcomingAsync()
        {
            return await _context.InterviewScheduleDetails
                .Include(i => i.JobApplication)
                .Where(i => i.Status == "Scheduled" && i.ScheduledDate >= DateTime.Now)
                .OrderBy(i => i.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InterviewScheduleDetail>> GetCompletedAsync()
        {
            return await _context.InterviewScheduleDetails
                .Include(i => i.JobApplication)
                .Where(i => i.Status == "Completed" || i.Status == "Cancelled")
                .OrderByDescending(i => i.ScheduledDate)
                .ToListAsync();
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
