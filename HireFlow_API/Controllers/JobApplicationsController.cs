 
using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly ICandidateScoringService _candidateScorer;
        public JobApplicationsController(IJobApplicationService jobService , ICandidateScoringService candidateScoringService)
        {
            _service = jobService;
            _candidateScorer = candidateScoringService;
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
        public async Task<IActionResult> ScoreCandidate( )
        {
            try
            {
                await _candidateScorer.ScoreCandidatesAsync();

                return Ok("Candidates Scroed...");
            }
            catch (Exception ex)
            {
                return Ok("Errror while Candidates Scoring..." + ex);
            }
            

            
        }

        [HttpGet("GetCandidatesByJob/{jobId}")]
        public async Task<IActionResult> GetCandidatesByJob(Guid jobId)
        
        {
            var result = await _service.GetCandidatesForJobAsync(jobId);
            if (result == null || !result.Any())
                return NotFound("No candidates found for this job.");
            return Ok(result);
        }

        [HttpGet("GetCandidatesForDropdown")]
        public async Task<IActionResult> GetCandidatesForDropdown()
        {
            var candidates = await _service.GetCandidatesForDropdownAsync();
            return Ok(candidates);
        }


        [HttpGet("interviewers-dropdown")]
        public async Task<IActionResult> GetInterviewersForDropdown()
        {
            var interviewers = await _service.GetInterviewersForDropdownAsync();
            return Ok(interviewers);
        }

    }

    // ✅ DTO for request
    public class CandidateScoreRequest
    {
     
        public string JobDescription { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile Resume { get; set; }
    }

    public class InterviewerDropdownDto
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
    public class CandidateDropdownDto
    {
        public Guid ApplicationId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }
}
