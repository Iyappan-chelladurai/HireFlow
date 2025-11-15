namespace CandidateDocumentsProcessor.Domain.Entities
{
    public enum VerificationStatus { Pending, Verified, Rejected }
    public class UploadedDocument
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CandidateId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string BlobUrl { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public string? Remarks { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
