using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using Microsoft.AspNetCore.Identity;

namespace HireFlow_API.Repositories
{

    public interface ICandidateDetailRepository
    {
        Task<string> CreateCandidateAsync(UserAccount user);
    }

    public class CandidateDetailRepository : ICandidateDetailRepository
    {

        private readonly ApplicationDbContext _context;

        public CandidateDetailRepository(ApplicationDbContext context)
        {
            _context = context;
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
