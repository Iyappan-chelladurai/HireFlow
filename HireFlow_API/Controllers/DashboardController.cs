using HireFlow_API.Model;
using HireFlow_API.Model.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireFlow_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetDashboardData()
        {
            // Replace this section with actual DB queries
            var stats = new DashboardStatsDto
            {
                OpenPositions = await _context.Jobs.Where(a=>a.ClosingDate > DateTime.Now).Select(j => j.Openings).FirstOrDefaultAsync(),
                Applications = await _context.JobApplications.Distinct().CountAsync(),
                Interviews = await _context.InterviewScheduleDetails.Where(a=>a.IsActive).CountAsync(),
                NewHires = await _context.JobApplications.CountAsync(a => a.ApplicationStatus == "Hired")
            };

            var recentApplications = await _context.JobApplications
                .OrderByDescending(a => a.AppliedOn)
                .Take(5)
                .Select(a => new RecentApplicationDto
                {
                    Candidate = a.Candidate.User.FullName,
                    Position = a.Job.JobTitle,
                    AppliedOn = a.AppliedOn,
                    Status = a.ApplicationStatus,

                    Score = _context.tbl_CandidatesJobScore.Where(a=>a.JobApplicationId == a.JobApplicationId)
                                    .OrderByDescending(a=>a.EvaluatedOn)
                                    .Select(a=>a.OverallFitScore).FirstOrDefault()
                }).Distinct()
                .ToListAsync();

            var upcomingInterviews = await _context.InterviewScheduleDetails
                                                   .Include(a=>a.JobApplication)
                                                    .Where(i => i.ScheduledDate >= DateTime.Today)
                                                    .OrderBy(i => i.ScheduledDate)
                                                    .Take(5)
                                                    .Select(i => new UpcomingInterviewDto
                                                    {
                                                        Date = i.ScheduledDate,
                                                        TimeRange = $"{i.ScheduledDate:hh\\:mm tt} - {(i.ScheduledDate).AddHours(1):hh\\:mm tt}",
                                                        Position = i.JobApplication.Job.JobTitle,
                                                        Candidate = _context.Users.Where(a=>a.Id == i.JobApplication.Candidate.User.Id).Select(a=>a.FullName).FirstOrDefault()
                                                    }).Distinct()
                .ToListAsync();

            var response = new DashboardResponseDto
            {
                Stats = stats,
                RecentApplications = recentApplications,
                UpcomingInterviews = upcomingInterviews
            };

            return Ok(response);
        }
    }

}
