using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;

namespace HireFlow_API.Services
{
   public interface IEmailService
    {

        Task JobAppliedEmail(string CandidateName, string jobTitle, string location, string CandidateEmail = "Iyappadhoni6@gmail.com");
        Task InterviewScheduledEmail(ScheduleInterviewDto interviewDto, string CandidateName, string jobTitle, string CandidateEmail = "Iyappadhoni6@gmail.com");
    }
    public class EmailService : IEmailService
    {

        private readonly IEmailRepository _emailRepository;

        public EmailService (IEmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
        }

        public async Task JobAppliedEmail(string CandidateName, string jobTitle, string location , string CandidateEmail = "Iyappadhoni6@gmail.com")
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Email", "JobApplied.html");
            var htmlTemplate = await File.ReadAllTextAsync(templatePath);

            var body = htmlTemplate
                .Replace("{{Candidate}}", CandidateName)
                .Replace("{{Position}}", jobTitle)
                .Replace("{{Date Applied}}", DateTime.Now.ToString("dd-MM-yyyy"))
                .Replace("{{Location}}", location);

            var emailRequest = new EmailRequestDTO
            {
                FromEmailAddress = "hireflowofficial@gmail.com",
                ToEmailAddresses = new List<string> { CandidateEmail },
                EmailSubject = $"{jobTitle} Application Received",
                HtmlEmailBody = body
            };

            await _emailRepository.SendEmail(emailRequest);      
        }

        public async Task InterviewScheduledEmail(ScheduleInterviewDto interviewDto , string CandidateName, string jobTitle, string CandidateEmail = "Iyappadhoni6@gmail.com")
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Email", "Interview.html");
            var htmlTemplate = await File.ReadAllTextAsync(templatePath);

            string body = htmlTemplate
                                    .Replace("{{CandidateName}}", CandidateName)
                                    .Replace("{{JobTitle}}", jobTitle)
                                    .Replace("{{InterviewType}}", interviewDto.InterviewMode)
                                    .Replace("{{InterviewDate}}", interviewDto.ScheduledDate.ToString())
                                    .Replace("{{InterviewTime}}", interviewDto.ScheduledDate.ToString())
                                    .Replace("{{InterviewLocation}}", interviewDto.interviewLocation)
                                    .Replace("{{ContactPerson}}", interviewDto.InterviewerName)
                                    .Replace("{{CalendarInviteLink}}", "");
                                    //.Replace("{{PrivacyPolicyLink}}", privacyLink);

            var emailRequest = new EmailRequestDTO
            {
                FromEmailAddress = "hireflowofficial@gmail.com",
                ToEmailAddresses = new List<string> { CandidateEmail },
                EmailSubject = $"{jobTitle} Interview Scheduled",
                HtmlEmailBody = body
            };

            await _emailRepository.SendEmail(emailRequest);
        }


    }
}
