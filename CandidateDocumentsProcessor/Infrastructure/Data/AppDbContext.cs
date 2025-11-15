using CandidateDocumentsProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CandidateDocumentsProcessor.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UploadedDocument> UploadedDocuments => Set<UploadedDocument>();
    }
}
