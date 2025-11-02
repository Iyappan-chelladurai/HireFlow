using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HireFlow_API.Repositories
{

    public interface ICandidateDetailRepository
    {
        Task<string> CreateCandidateAsync(UserAccount user);

        Task<JobApplicationDTO?> GetApplicationByIdAsync(Guid applicationId);
        Task<bool> UpdateCandidateAsync(CandidateDetail updatedCandidate , bool isTrans = false);
        Task<List<CandidateCardDTO>> GetAllCandidateCardsAsync();
    }

    public class CandidateDetailRepository : ICandidateDetailRepository
    {

        private readonly ApplicationDbContext _context;

        public CandidateDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplicationDTO?> GetApplicationByIdAsync(Guid applicationId)
        {
            return await _context.JobApplications.Include(s=> s.Job)
                .AsNoTracking()
                .Where(j => j.ApplicationId == applicationId)
                .Select(j => new JobApplicationDTO
                {
                    ApplicationId = j.ApplicationId,
                    AppliedOn = j.AppliedOn,
                    ApplicationStatus = j.ApplicationStatus,
                    JobLocation = j.Job.Location,
                    JobTitle = j.Job.JobTitle,
                    CandidateName = j.Candidate.User.FullName,   // make sure UserAccount has FullName
                    CandidateEmail = j.Candidate.User.Email      // make sure UserAccount has Email
                })
                .FirstOrDefaultAsync();
        }

        public async Task<string> CreateCandidateAsync(UserAccount user)
        {
            try
            {
                var entity = new CandidateDetail
                {
                    CandidateId = Guid.NewGuid(),
                    UserId = user.Id,
                    EducationLevel = "",
                    NoticePeriodDays = 0,
                    ProfileCreatedOn = DateTime.UtcNow,
                };

                _context.CandidateDetails.Add(entity);
                await _context.SaveChangesAsync();

                return "1";
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message;
            }
        }
        public async Task<bool> UpdateCandidateAsync(CandidateDetail updatedCandidate , bool isTrnas = false)
        {
            if (updatedCandidate == null)
                throw new ArgumentNullException(nameof(updatedCandidate));
            try
            {
                // Fetch the existing entity
                var existingCandidate = await _context.CandidateDetails
                    .FirstOrDefaultAsync(c => c.CandidateId == updatedCandidate.CandidateId);

                if (existingCandidate == null)
                    return false; // Candidate not found

                // Update fields (only relevant fields, avoid overwriting ID/UserId)
                existingCandidate.EducationLevel = updatedCandidate.EducationLevel;
                existingCandidate.NoticePeriodDays = updatedCandidate.NoticePeriodDays;
                existingCandidate.CurrentJobTitle = updatedCandidate.CurrentJobTitle;
                existingCandidate.TotalExperienceYears = updatedCandidate.TotalExperienceYears;
                existingCandidate.ExpectedSalary = updatedCandidate.ExpectedSalary;
                existingCandidate.AvailableFrom = updatedCandidate.AvailableFrom;
                existingCandidate.PreferredLocation = updatedCandidate.PreferredLocation;

                _context.CandidateDetails.Update(existingCandidate);

                if (!isTrnas)
                {
                    await _context.SaveChangesAsync();
                }
                return true; // update successful
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                // _logger.LogError(ex, "Error updating candidate");
                return false; // indicate failure
            }
        }

        public async Task<List<CandidateCardDTO>> GetAllCandidateCardsAsync()
        {
            var candidates = await _context.CandidateDetails
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Include(c => c.JobApplications)
                .ToListAsync();

            var candidateCards = candidates.Select(c => new CandidateCardDTO
            {
                CandidateName = c.User.FullName ?? "Unknown",
                Position = c.CurrentJobTitle ?? "N/A",
                Status = c.JobApplications.FirstOrDefault().ApplicationStatus, // Replace with real status if needed
                MatchScore = 92,      // Replace with real score if needed
                ImageUrl = c.User.ProfileImagePath ?? "https://via.placeholder.com/100",
                Experience = $"{c.TotalExperienceYears ?? 0} years",
                Education = c.PreferredLocation ?? "N/A",
                AppliedDate = c.JobApplications.FirstOrDefault()?.AppliedOn ?? DateTime.UtcNow,
                Skills = new List<string>()//c.JobApplications.Select(d => d.).ToList() // or another property for skills
            }).ToList();

            return candidateCards;
        }
    }
}
