using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace HireFlow_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {

        private readonly IJobApplicationService _service;

        private readonly CandidateScorerService _candidateScorer;

        public JobApplicationsController(IJobApplicationService jobService)
        {
            _service = jobService;
            _candidateScorer = new CandidateScorerService();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobApplicationDTO>>> GetAllApplications(Guid JobId)
        {
            var applications = await _service.RetrieveAllApplicationsAsync(JobId);

            return Ok(applications);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobApplication>> GetApplicationDetails(Guid id)
        {
            var application = await _service.RetrieveApplicationDetailsAsync(id);

            if (application == null)
                return NotFound();

            return Ok(application);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<JobApplicationDTO>> SubmitNewApplication([FromForm] JobApplicationDTO jobApplication)
        {
            if (jobApplication.ResumeFile == null || jobApplication.ResumeFile.Length == 0)
                return BadRequest("Resume file is required.");

            var created = await _service.SubmitApplicationAsync(jobApplication);


            EmailRepository email = new EmailRepository();


            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Email", "JobApplied.html");
            string htmlTemplate = System.IO.File.ReadAllText(templatePath);

            // Replace placeholders
            string body = htmlTemplate
                .Replace("{{CandidateName}}", "Iyappan C")
                 .Replace("{{JobTitle}}", ".NET Developer")
                  .Replace("{{CompanyName}}", "HireFlow Technologies");


            EmailRequestDTO emailRequest = new EmailRequestDTO()
            {

                FromEmailAddress = "hireflowofficial@gmail.com",
                ToEmailAddresses = new List<string>() { "iyappadhoni6@gmail.com" },

                EmailSubject = ".NET Developer Applied..",
                HtmlEmailBody = body
            };

            email.SendEmail(emailRequest);



            return CreatedAtAction(nameof(GetApplicationDetails), new { id = created.ApplicationId }, created);



        }

    
        [HttpGet("candidate/{candidateId:guid}")]
        public async Task<IActionResult> GetApplicationsByCandidateId(Guid candidateId)
        {
            var applications = await _service.GetApplicationsByCandidateIdAsync(candidateId);

            if (applications == null || !applications.Any())
                return NotFound(new { Message = "No applications found for this candidate." });

            return Ok(applications);
        }
        /// <summary>
        /// Score a candidate against a Job Role + Job Description
        /// </summary>
        [HttpPost("score")]
        public async Task<IActionResult> ScoreCandidate([FromForm] CandidateScoreRequest request)
        {
            if (request.Resume == null || request.Resume.Length == 0)
                return BadRequest("Resume file is required");

            using var ms = new MemoryStream();
            await request.Resume.CopyToAsync(ms);

            var result = await _candidateScorer.ScoreCandidateAsync(
                ms.ToArray(),
                request.Resume.FileName,
              
                request.JobDescription
            );

            return Ok(result);
        }
    }

    // ✅ DTO for request
    public class CandidateScoreRequest
    {
     
        public string JobDescription { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile Resume { get; set; }
    }
}
