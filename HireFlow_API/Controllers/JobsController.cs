using HireFlow_API.Model.DTOs;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDTO>>> GetAllJobs()
        {
            var jobs = await _jobService.RetrieveAllJobsAsync();
            return Ok(jobs);
        }

        [HttpGet("{jobId}")]
        public async Task<ActionResult<JobDTO>> GetJobById(Guid jobId)
        {
            var job = await _jobService.RetrieveJobByIdAsync(jobId);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JobDTO>> CreateJob(CreateJobDTO newJob)
        {
            var createdJob = await _jobService.CreateNewJobAsync(newJob);
            return CreatedAtAction(nameof(GetJobById), new { jobId = createdJob.JobId }, createdJob);
        }

        [HttpPut("{jobId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateJob(Guid jobId, UpdateJobDTO updatedJob)
        {
            var updated = await _jobService.UpdateJobDetailsAsync(jobId, updatedJob);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{jobId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteJob(Guid jobId)
        {
            var deleted = await _jobService.DeleteJobAsync(jobId);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
