using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class OnboardingStatus
    {
        [Key]
        public Guid StatusId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateId { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public CandidateDetail Candidate { get; set; }

        public bool IsPersonalInfo_Completed { get; set; }

        public bool IsDocuments_Uploaded { get; set; }

        public bool IsDocuments_Verified { get; set; }

        public bool IsOfferLetter_Accepted { get; set; }

        public bool IsWelcomeEmail_Sent { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

}
