using HireFlow.Services;
using HireFlow_API.Controllers;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using Microsoft.CodeAnalysis.Elfie.Extensions;

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
    }

    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _repository;

        private readonly ICandidateDocumentsRepository _candidateDocumentsRepository;

        private readonly IConfiguration _configuration;
         
        private readonly IEmailRepository _emailRepository;

        private readonly CandidateScorerService _candidateScorer;




        public JobApplicationService(IJobApplicationRepository repository , IConfiguration configuration,
            ICandidateDocumentsRepository candidateDocumentsRepository,
            IEmailRepository emailRepository,
            CandidateScorerService candidateScorer
            )
        {
            _repository = repository;
            _configuration = configuration;
            _candidateDocumentsRepository = candidateDocumentsRepository;
            _emailRepository = emailRepository;
            _candidateScorer = candidateScorer;
        }


        public async Task<IEnumerable<JobApplicationDTO>> RetrieveAllApplicationsAsync(Guid JobId)
        {
            return await _repository.GetAllApplicationsAsync(JobId);
        }

        public async Task<JobApplicationDTO?> RetrieveApplicationDetailsAsync(Guid applicationId)
        {

            return await _repository.GetApplicationByIdAsync(applicationId);
        }

        public async Task<JobApplicationDTO> SubmitApplicationAsync(JobApplicationDTO jobApplication)
        {
            var ResumePath = _configuration["FilePath:ResumePath"];
            Directory.CreateDirectory(ResumePath);

            var filePath = Path.Combine(ResumePath, Guid.NewGuid() + Path.GetExtension(jobApplication.ResumeFile.FileName));

            // Optionally store the file path in DTO
            jobApplication.ResumePath = filePath;

           var AppId = await _repository.AddNewApplicationAsync(jobApplication);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await jobApplication.ResumeFile.CopyToAsync(stream);
            }
          

            CandidateDocumentDetail candidateDocumentDetail = new CandidateDocumentDetail()
            {

                DocumentDetailId = Guid.NewGuid(),
                CandidateId = jobApplication.CandidateId,
                FilePath = filePath,
                FileName = jobApplication.ResumeFile.FileName,
                FileExtension = Path.GetExtension(jobApplication.ResumeFile.FileName),
                DocumentTypeId = 3,
                UploadedOn = DateTime.Now,
                IsFraudDetected = false,
                IsVerified = false,
                FraudScore = 0,
                IsActive = false,
                FileSizeInMB = Convert.ToDecimal(jobApplication.ResumeFile.Length / 24) / 24,
            };

           await _candidateDocumentsRepository.AddAsync(candidateDocumentDetail);
           
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Email", "JobApplied.html");
            string htmlTemplate = System.IO.File.ReadAllText(templatePath);

            // Replace placeholders
            string body = htmlTemplate
                .Replace("{{CandidateName}}", "Iyappan C")
                 .Replace("{{JobTitle}}", ".NET Developer")
                  .Replace("{{CompanyName}}", "Infoplus Technologies");

            EmailRequestDTO emailRequest = new EmailRequestDTO()
            {
                FromEmailAddress = "hireflowofficial@gmail.com",
                ToEmailAddresses = new List<string>() { "iyappadhoni6@gmail.com" },
                EmailSubject = ".NET Developer Applied..",
                HtmlEmailBody = body
            };
          var CS =  await _candidateScorer.ScoreCandidateAsync(jobApplication.JobId, AppId);


            _emailRepository.SendEmail(emailRequest);

            return jobApplication;
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
    }
}
