using HireFlow_API.Model.DTOs;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HireFlow_API.Controllers
{
 
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "HR")]
    public class InterviewScheduleController : ControllerBase
    {
        private readonly IInterviewScheduleService _service;

        public InterviewScheduleController(IInterviewScheduleService service)
        {
            _service = service;
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
