using CandidateDocumentsProcessor.Application.Interfaces;
using CandidateDocumentsProcessor.Domain.Entities;

namespace CandidateDocumentsProcessor.Application.Services
{
    public class DocumentService
    {
        private readonly IDocumentRepository _repo;
        public DocumentService(IDocumentRepository repo)
        {
            _repo = repo;
        }

        public async Task<UploadedDocument> UploadAsync(Guid candidateId, string documentType,
            string fileName, string blobUrl, string contentType, long size)
        {
            var doc = new UploadedDocument
            {
                CandidateId = candidateId,
                DocumentType = documentType,
                FileName = fileName,
                BlobUrl = blobUrl,
                ContentType = contentType,
                FileSize = size
            };

            await _repo.AddAsync(doc);
            await _repo.SaveChangesAsync();
            return doc;
        }

        public async Task<IEnumerable<UploadedDocument>> GetDocumentsAsync(Guid candidateId)
            => await _repo.GetByCandidateAsync(candidateId);

        public async Task VerifyAsync(Guid id, VerificationStatus status, string? remarks)
        {
            var doc = await _repo.GetAsync(id) ?? throw new KeyNotFoundException("Document not found");
            doc.Status = status;
            doc.Remarks = remarks;
            _repo.Update(doc);
            await _repo.SaveChangesAsync();
        }
    }
}
