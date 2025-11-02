using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using Microsoft.EntityFrameworkCore;

namespace HireFlow_API.Repositories
{
   public  interface ICandidateDocumentsRepository
    {
        Task<IEnumerable<CandidateDocumentDetail>> GetAllAsync();
        Task<CandidateDocumentDetail> GetByIdAsync(Guid id);
        Task AddAsync(CandidateDocumentDetail entity, bool isTrans);
        Task UpdateAsync(CandidateDocumentDetail entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
    public class CandidateDocumentsRepository : ICandidateDocumentsRepository
    {
 
        private readonly ApplicationDbContext _context;

        public CandidateDocumentsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CandidateDocumentDetail>> GetAllAsync()
        {
            return await _context.CandidateDocumentDetails.ToListAsync();
        }

        public async Task<CandidateDocumentDetail> GetByIdAsync(Guid id)
        {
            return await _context.CandidateDocumentDetails.FindAsync(id);
        }

        public async Task AddAsync(CandidateDocumentDetail entity , bool isTrans = false)
        {
            _context.CandidateDocumentDetails.Add(entity);
            
            if (!isTrans)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(CandidateDocumentDetail entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.CandidateDocumentDetails.FindAsync(id);
            if (entity != null)
            {
                _context.CandidateDocumentDetails.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.CandidateDocumentDetails.AnyAsync(e => e.DocumentDetailId == id);
        }
    }


}
