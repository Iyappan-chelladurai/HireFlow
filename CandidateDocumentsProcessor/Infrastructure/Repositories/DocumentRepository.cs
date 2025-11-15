using CandidateDocumentsProcessor.Application.Interfaces;
using CandidateDocumentsProcessor.Domain.Entities;
using CandidateDocumentsProcessor.Infrastructure.Data;
 
using Microsoft.EntityFrameworkCore;

namespace DocumentVerificationService.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _ctx;

    public DocumentRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task AddAsync(UploadedDocument document) => await _ctx.UploadedDocuments.AddAsync(document);
    public async Task<UploadedDocument?> GetAsync(Guid id) => await _ctx.UploadedDocuments.FindAsync(id);
    public async Task<IEnumerable<UploadedDocument>> GetByCandidateAsync(Guid candidateId) =>
        await _ctx.UploadedDocuments.Where(d => d.CandidateId == candidateId).ToListAsync();
    public void Update(UploadedDocument document) => _ctx.UploadedDocuments.Update(document);
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
