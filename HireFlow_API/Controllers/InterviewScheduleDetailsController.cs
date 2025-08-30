using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
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
    public class InterviewScheduleDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InterviewScheduleDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/InterviewScheduleDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InterviewDetailsDTO>>> GetInterviewScheduleDetails()
        {
            return await _context.InterviewScheduleDetails.AsNoTracking()
                .Select(j => new InterviewDetailsDTO
                {
                    InterviewId = j.InterviewId,
                    ApplicationId = j.ApplicationId,
                    InterviewerName = j.InterviewerName,
                    ScheduledDate = j.ScheduledDate,
                    InterviewFeedback = j.InterviewFeedback,
                    InterviewMode = j.InterviewMode,
                    InterviewResult = j.InterviewResult,
                    IsActive = j.IsActive,
                    CreatedOn = j.CreatedOn,
                }).ToListAsync();
        }

        // GET: api/InterviewScheduleDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InterviewScheduleDetail>> GetInterviewScheduleDetail(Guid id)
        {
            var interviewScheduleDetail = await _context.InterviewScheduleDetails.FindAsync(id);

            if (interviewScheduleDetail == null)
            {
                return NotFound();
            }

            return interviewScheduleDetail;
        }

        // PUT: api/InterviewScheduleDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInterviewScheduleDetail(Guid id, InterviewScheduleDetail interviewScheduleDetail)
        {
            if (id != interviewScheduleDetail.InterviewId)
            {
                return BadRequest();
            }

            _context.Entry(interviewScheduleDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InterviewScheduleDetailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/InterviewScheduleDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InterviewDetailsDTO>> PostInterviewScheduleDetail(InterviewDetailsDTO interviewScheduleDetail)
        {

            var newinterviewScheduleDetail = new InterviewScheduleDetail()
            {
                ApplicationId = interviewScheduleDetail.ApplicationId,
                ScheduledDate = interviewScheduleDetail.ScheduledDate,
                InterviewerName = interviewScheduleDetail.InterviewerName,
                InterviewMode = interviewScheduleDetail.InterviewMode,
                InterviewResult = null,
                InterviewFeedback = null,
                IsActive = true,
                CreatedOn = DateTime.Now,
            };

            _context.InterviewScheduleDetails.Add(newinterviewScheduleDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInterviewScheduleDetail", new { id = newinterviewScheduleDetail.InterviewId }, newinterviewScheduleDetail);
        }

        // DELETE: api/InterviewScheduleDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInterviewScheduleDetail(Guid id)
        {
            var interviewScheduleDetail = await _context.InterviewScheduleDetails.FindAsync(id);
            if (interviewScheduleDetail == null)
            {
                return NotFound();
            }

            _context.InterviewScheduleDetails.Remove(interviewScheduleDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InterviewScheduleDetailExists(Guid id)
        {
            return _context.InterviewScheduleDetails.Any(e => e.InterviewId == id);
        }
    }
}
