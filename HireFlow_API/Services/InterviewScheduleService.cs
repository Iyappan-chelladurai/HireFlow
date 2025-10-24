using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;

namespace HireFlow_API.Services
{
    public interface IInterviewScheduleService
    {
        Task<IEnumerable<InterviewScheduleDetail>> GetUpcomingInterviewsAsync();
        Task<IEnumerable<InterviewScheduleDetail>> GetCompletedInterviewsAsync();
        Task<InterviewScheduleDetail?> GetInterviewByIdAsync(Guid id);
        Task<InterviewScheduleDetail> ScheduleInterviewAsync(ScheduleInterviewDto dto);
        Task<InterviewScheduleDetail?> RescheduleInterviewAsync(Guid id, DateTime newDate, string reason);
        Task<InterviewScheduleDetail?> UpdateStatusAsync(Guid id, string status, string? feedback = null, string? result = null);
    }

    public class InterviewScheduleService : IInterviewScheduleService
    {
        private readonly IInterviewScheduleRepository _repository;

        public InterviewScheduleService(IInterviewScheduleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InterviewScheduleDetail>> GetUpcomingInterviewsAsync()
            => await _repository.GetUpcomingAsync();

        public async Task<IEnumerable<InterviewScheduleDetail>> GetCompletedInterviewsAsync()
            => await _repository.GetCompletedAsync();

        public async Task<InterviewScheduleDetail?> GetInterviewByIdAsync(Guid id)
            => await _repository.GetByIdAsync(id);

        public async Task<InterviewScheduleDetail> ScheduleInterviewAsync(ScheduleInterviewDto dto)
        {
            var interview = new InterviewScheduleDetail
            {
                InterviewId = Guid.NewGuid(),
                ApplicationId = dto.ApplicationId,
                ScheduledDate = dto.ScheduledDate,
                InterviewerName = dto.InterviewerName,
                InterviewMode = dto.InterviewMode,
                MeetingLink = dto.MeetingLink,
                RoundNumber = dto.RoundNumber,
                Status = dto.Status,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            await _repository.AddAsync(interview);
            await _repository.SaveChangesAsync();

            return interview;
        }

        public async Task<InterviewScheduleDetail?> RescheduleInterviewAsync(Guid id, DateTime newDate, string reason)
        {
            var interview = await _repository.GetByIdAsync(id);
            if (interview == null) return null;

            interview.RescheduledDate = newDate;
            interview.RescheduleReason = reason;
            interview.Status = "Rescheduled";
            interview.UpdatedOn = DateTime.UtcNow;

            await _repository.UpdateAsync(interview);
            await _repository.SaveChangesAsync();
            return interview;
        }

        public async Task<InterviewScheduleDetail?> UpdateStatusAsync(Guid id, string status, string? feedback = null, string? result = null)
        {
            var interview = await _repository.GetByIdAsync(id);
            if (interview == null) return null;

            interview.Status = status;
            interview.InterviewFeedback = feedback ?? interview.InterviewFeedback;
            interview.InterviewResult = result ?? interview.InterviewResult;
            interview.UpdatedOn = DateTime.UtcNow;

            await _repository.UpdateAsync(interview);
            await _repository.SaveChangesAsync();
            return interview;
        }
    }
}
