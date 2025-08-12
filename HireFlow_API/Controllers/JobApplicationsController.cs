using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HireFlow_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {

        private readonly IJobApplicationService _service;

        public JobApplicationsController(IJobApplicationService jobService)
        {
            _service = jobService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobApplication>>> GetAllApplications()
        {
            var applications = await _service.RetrieveAllApplicationsAsync();
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplication(Guid id, JobApplicationDTO jobApplication)
        {
            var updated = await _service.UpdateExistingApplicationAsync(id, jobApplication);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelApplication(Guid id)
        {
            var deleted = await _service.CancelApplicationAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

    }
}
