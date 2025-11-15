using DocumentFormat.OpenXml.Office2010.Excel;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HireFlow_API.Controllers
{
 
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "HR")]
    public class InterviewScheduleController : ControllerBase
    {
        private readonly IInterviewScheduleService _service;

        private readonly HttpClient _httpClient;

        public InterviewScheduleController(IInterviewScheduleService service , HttpClient httpClient )
        {
            _service = service;
            _httpClient = httpClient;
           
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming()
        {

            var token = Request.Headers["Authorization"].ToString();

            var interviews = await _service.GetUpcomingInterviewsAsync();
            return Ok(interviews);
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompleted()
        {
            var interviews = await _service.GetCompletedInterviewsAsync();
            return Ok(interviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var interview = await _service.GetInterviewByIdAsync(id);
            if (interview == null) return NotFound();
            return Ok(interview);
        }
        [HttpPost("schedule")]
     
        public async Task<IActionResult> Schedule([FromBody] ScheduleInterviewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.ScheduleInterviewAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.InterviewId }, created);
        }
        [HttpPut("reschedule/{id}")]
        public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleDto dto)
        {
            var updated = await _service.RescheduleInterviewAsync(id, dto.NewDate, dto.Reason);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var updated = await _service.UpdateStatusAsync(id, dto.Status, dto.Feedback, dto.Result);
            if (updated == null) return NotFound();
            return Ok(updated);
        }


        [AllowAnonymous]
        [HttpPost("Interview/GenerateInterviewQA")]
        public async Task<IActionResult> GenerateInterviewQA([FromBody] Guid interviewId)
        {

            var InterviewQuestions = await _service.GenerateInterviewQAAsync(interviewId);
            if (InterviewQuestions == null) return NotFound();
            return Ok(InterviewQuestions);

        }
    }



public class ResumeRequest
{
        public Guid? interviewId { get; set; } 
        public string ResumeText { get; set; } = string.Empty;
}
    public class InterviewQuestion
    {
        public string Category { get; set; }
        public string Question { get; set; }
        public string Difficulty { get; set; }
        public List<string> Tags { get; set; } = new();
    }
    public class InterviewQuestionResponse
    {
        public List<InterviewQuestion> Questions { get; set; } = new();
        public string Error { get; set; }
        public string RawResponse { get; set; }
    }


    public class RescheduleDto
    {
        public DateTime NewDate { get; set; }
        public string Reason { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; }
        public string? Feedback { get; set; }
        public string? Result { get; set; }
    }

}
