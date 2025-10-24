using HireFlow_API.Controllers;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.Extensions.Logging;

namespace HireFlow_API.Services
{
    public interface IJobApplicationService
    {
        Task<IEnumerable<JobApplicationDTO>> RetrieveAllApplicationsAsync(Guid JobId);
        Task<JobApplicationDTO?> RetrieveApplicationDetailsAsync(Guid applicationId);
        Task<JobApplicationDTO> SubmitApplicationAsync(JobApplicationDTO jobApplication);
        Task<bool> UpdateExistingApplicationAsync(Guid applicationId, JobApplicationDTO jobApplication);
        Task<bool> CancelApplicationAsync(Guid applicationId);
        Task<IEnumerable<JobApplicationResponseDTO>> GetApplicationsByCandidateIdAsync(Guid candidateId);
        Task<IEnumerable<CandidateDisplayDto>> GetCandidatesForJobAsync(Guid jobId);

        Task<IEnumerable<CandidateDropdownDto>> GetCandidatesForDropdownAsync();

        Task<IEnumerable<InterviewerDropdownDto>> GetInterviewersForDropdownAsync();
    


}

    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _repository;
        private readonly ICandidateDocumentsRepository _candidateDocumentsRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailRepository _emailRepository;
        private readonly ICandidateScoringService _candidateScorer;
        private readonly ICandidateDetailService _CandidateDetailService;
        private readonly ILogger<JobApplicationService> _logger;

        public JobApplicationService(
            IJobApplicationRepository repository,
            IConfiguration configuration,
            ICandidateDocumentsRepository candidateDocumentsRepository,
            IEmailRepository emailRepository,
            ICandidateScoringService candidateScoringService,
            ILogger<JobApplicationService> logger,
            ICandidateDetailService candidateDetailService)
        {
            _repository = repository;
            _configuration = configuration;
            _candidateDocumentsRepository = candidateDocumentsRepository;
            _emailRepository = emailRepository;
            _candidateScorer = candidateScoringService;
            _logger = logger;
            _CandidateDetailService = candidateDetailService;
        }

        public async Task<IEnumerable<JobApplicationDTO>> RetrieveAllApplicationsAsync(Guid JobId)
        {
            var applications = await _repository.GetAllApplicationsAsync(JobId);

            return applications.Select(a => new JobApplicationDTO
            {
                ApplicationId = a.ApplicationId,
                JobId = a.JobId,
                JobTitle = a.JobTitle,
                CandidateId = a.CandidateId,
                CandidateName = a.CandidateName,
                CandidateEmail = a.CandidateEmail,
                AppliedOn = a.AppliedOn,
                ApplicationStatus = a.ApplicationStatus,
                Skills = a.Skills,
                EducationLevel = a.EducationLevel,
                ExpectedSalary = a.ExpectedSalary,
                NoticePeriodDays = a.NoticePeriodDays,
                PreferredLocation = a.PreferredLocation,
                ResumePath = a.ResumePath,
                InterviewFeedback = a.InterviewFeedback,
                OfferSentOn = a.OfferSentOn,
                IsOfferAccepted = a.IsOfferAccepted,
                OnboardedOn = a.OnboardedOn
            });
        }


        public async Task<JobApplicationDTO?> RetrieveApplicationDetailsAsync(Guid applicationId)
        {
            return await _repository.GetApplicationByIdAsync(applicationId);
        }

        public async Task<JobApplicationDTO> SubmitApplicationAsync(JobApplicationDTO jobApplication)
        {
            if (jobApplication.ResumeFile == null || jobApplication.ResumeFile.Length == 0)
                throw new ArgumentException("Resume file is required.");

            // Validate file type (allow only PDF or DOCX)
            var allowedExtensions = new[] { ".pdf", ".docx" };
            var fileExtension = Path.GetExtension(jobApplication.ResumeFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Only PDF or DOCX files are allowed.");

            // Validate file size (max 5 MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (jobApplication.ResumeFile.Length > maxFileSize)
                throw new ArgumentException("File size exceeds 5 MB.");

            try
            {
                // Ensure directory exists
                var resumePath = _configuration["FilePath:ResumePath"];
                Directory.CreateDirectory(resumePath);

                // Generate unique file path
                var filePath = Path.Combine(resumePath, Guid.NewGuid() + fileExtension);
                jobApplication.ResumePath = filePath;

                // Save file asynchronously
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await jobApplication.ResumeFile.CopyToAsync(stream);
                }

                // Add job application to repository
                var appId = await _repository.AddNewApplicationAsync(jobApplication);


                var candidateDetails = new CandidateDetail
                {
                    CurrentJobTitle = jobApplication.CurrentJobTitle,
                    TotalExperienceYears = jobApplication.TotalExperienceYears,
                    NoticePeriodDays = jobApplication.NoticePeriodDays,
                    EducationLevel = jobApplication.EducationLevel,
                   AvailableFrom = jobApplication.AvailableFrom,
                    PreferredLocation = jobApplication.PreferredLocation,
                   ExpectedSalary = jobApplication.ExpectedSalary,
                    Skills = jobApplication.Skills,

                };

                await _CandidateDetailService.UpdateCandidateAsync(candidateDetails);



                 // Save candidate document details
                 var candidateDocumentDetail = new CandidateDocumentDetail
                {
                    DocumentDetailId = Guid.NewGuid(),
                    CandidateId = jobApplication.CandidateId,
                    FilePath = filePath,
                    FileName = jobApplication.ResumeFile.FileName,
                    FileExtension = fileExtension,
                    DocumentTypeId = 3,
                    UploadedOn = DateTime.Now,
                    IsFraudDetected = false,
                    IsVerified = false,
                    FraudScore = 0,
                    IsActive = true,
                    FileSizeInMB = Math.Round((decimal)jobApplication.ResumeFile.Length / (1024 * 1024), 2)
                };

                await _candidateDocumentsRepository.AddAsync(candidateDocumentDetail);

              var Candidate = await _CandidateDetailService.GetApplicationByIdAsync(appId);


                // Read email template
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Email", "JobApplied.html");
                var htmlTemplate = await File.ReadAllTextAsync(templatePath);

                // Replace placeholders dynamically
                var candidateName = Candidate.CandidateName ?? "Candidate";
                var jobTitle = Candidate.JobTitle ?? "Applied Job";

                var body = htmlTemplate
                    .Replace("{{CandidateName}}", candidateName)
                    .Replace("{{JobTitle}}", jobTitle)
                    .Replace("{{CompanyName}}", "");

                // Send email asynchronously
                var emailRequest = new EmailRequestDTO
                {
                    FromEmailAddress = "hireflowofficial@gmail.com",
                    ToEmailAddresses = new List<string> { "Iyappadhoni6@gmail.com" ?? "no-reply@example.com" },
                    EmailSubject = $"{jobTitle} Application Received",
                    HtmlEmailBody = body
                };

                await _emailRepository.SendEmail(emailRequest);


                jobApplication.ApplicationId = appId;

                // Score candidate
                var score = await _candidateScorer.ScoreCandidateAsync(jobApplication);

                _logger.LogInformation("Candidate {CandidateId} submitted application {ApplicationId} successfully.", jobApplication.CandidateId, appId);

                return jobApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting application for CandidateId {CandidateId}", jobApplication.CandidateId);
                throw;
            }
        }

        public async Task<bool> UpdateExistingApplicationAsync(Guid applicationId, JobApplicationDTO jobApplication)
        {
            if (applicationId != jobApplication.ApplicationId)
                return false;

            if (!await _repository.IsApplicationExistsAsync(applicationId))
                return false;

            await _repository.UpdateApplicationInfoAsync(jobApplication);
            return true;
        }

        public async Task<bool> CancelApplicationAsync(Guid applicationId)
        {
            if (!await _repository.IsApplicationExistsAsync(applicationId))
                return false;

            await _repository.DeleteApplicationByIdAsync(applicationId);
            return true;
        }

        public async Task<IEnumerable<JobApplicationResponseDTO>> GetApplicationsByCandidateIdAsync(Guid candidateId)
        {
            return await _repository.GetByCandidateIdAsync(candidateId);
        }

        public async Task<IEnumerable<CandidateDisplayDto>> GetCandidatesForJobAsync(Guid jobId)
        {
            var candidates = await _repository.GetCandidatesForJobAsync(jobId);

            // Example: Sort by applied date (newest first)
            return candidates.OrderByDescending(c => c.MatchScore).ToList();
        }


        public async Task<IEnumerable<CandidateDropdownDto>> GetCandidatesForDropdownAsync()
        {
            return await _repository.GetCandidatesForDropdownAsync();
        }
        public async Task<IEnumerable<InterviewerDropdownDto>> GetInterviewersForDropdownAsync()
        {
            return await _repository.GetInterviewersForDropdownAsync();
        }
    }
}
