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
            return await _context.JobApplications
                .AsNoTracking()
                .Where(j => j.ApplicationId == applicationId)
                .Select(j => new JobApplicationDTO
                {
                    ApplicationId = j.ApplicationId,
                    AppliedOn = j.AppliedOn,
                    ApplicationStatus = j.ApplicationStatus,
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
                    ResumePath = "",
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
    }
}
