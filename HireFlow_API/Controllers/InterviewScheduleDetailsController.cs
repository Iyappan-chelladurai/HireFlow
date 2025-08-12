using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;

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
        public async Task<ActionResult<IEnumerable<InterviewScheduleDetail>>> GetInterviewScheduleDetails()
        {
            return await _context.InterviewScheduleDetails.ToListAsync();
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
        public async Task<ActionResult<InterviewScheduleDetail>> PostInterviewScheduleDetail(InterviewScheduleDetail interviewScheduleDetail)
        {
            _context.InterviewScheduleDetails.Add(interviewScheduleDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInterviewScheduleDetail", new { id = interviewScheduleDetail.InterviewId }, interviewScheduleDetail);
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
