using CandidateDocumentsProcessor.Domain.Entities;

namespace CandidateDocumentsProcessor.Application.Interfaces
{
    public class IDocumentRepository
    {
        Task AddAsync(UploadedDocument document);
        Task<UploadedDocument?> GetAsync(Guid id);
        Task<IEnumerable<UploadedDocument>> GetByCandidateAsync(Guid candidateId);
        void Update(UploadedDocument document);
        Task<int> SaveChangesAsync();
    }
}
