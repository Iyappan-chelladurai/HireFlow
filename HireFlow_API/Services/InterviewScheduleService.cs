using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;

namespace HireFlow_API.Services
{
    public interface IInterviewScheduleService
    {
        Task<IEnumerable<UpcomingInterviewDto>> GetUpcomingInterviewsAsync();
        Task<IEnumerable<CompleteInterviewDto>> GetCompletedInterviewsAsync();
        Task<InterviewScheduleDetail?> GetInterviewByIdAsync(Guid id);
        Task<InterviewScheduleDetail> ScheduleInterviewAsync(ScheduleInterviewDto dto);
        Task<InterviewScheduleDetail?> RescheduleInterviewAsync(Guid id, DateTime newDate, string reason);
        Task<InterviewScheduleDetail?> UpdateStatusAsync(Guid id, string status, string? feedback = null, string? result = null);
    }

    public class InterviewScheduleService : IInterviewScheduleService
    {
        private readonly IInterviewScheduleRepository _repository;

        private readonly IEmailService _emailService;

        private readonly IJobApplicationService _jobApplicationService;

        public InterviewScheduleService(IInterviewScheduleRepository repository, IEmailService emailService,
                                                                IJobApplicationService jobApplicationService)
        {
            _repository = repository;
            _emailService = emailService;
            _jobApplicationService = jobApplicationService;
        }

        public async Task<IEnumerable<UpcomingInterviewDto>> GetUpcomingInterviewsAsync()
            => await _repository.GetUpcomingAsync();

        public async Task<IEnumerable<CompleteInterviewDto>> GetCompletedInterviewsAsync()
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
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };




            await _repository.AddAsync(interview);
            await _repository.SaveChangesAsync();


            var candidate = await _jobApplicationService.GetCandidatesbyApplicationIdAsync(dto.ApplicationId);

            _emailService.InterviewScheduledEmail(dto, candidate.CandidateName, candidate.JobTitle);

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
