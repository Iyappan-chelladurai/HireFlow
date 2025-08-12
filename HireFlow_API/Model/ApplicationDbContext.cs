using HireFlow_API.Model.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HireFlow_API.Model
{
    public class ApplicationDbContext : IdentityDbContext<UserAccount, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        //public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<CandidateDetail> CandidateDetails { get; set; }

        public DbSet<DocumentType> DocumentTypes { get; set; }

        public DbSet<CandidateDocumentDetail> CandidateDocumentDetails { get; set; }

        public DbSet<JobApplication> JobApplications { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<OfferLetter> OfferLetters { get; set; }

        public DbSet<OnboardingStatus> OnboardingStatusTable { get; set; }


        public DbSet<InterviewScheduleDetail> InterviewScheduleDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<UserAccount>().ToTable("UserAccounts");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");


            modelBuilder.Entity<CandidateDocumentDetail>()
                        .HasOne(d => d.Candidate)
                        .WithMany(c => c.Documents)
                        .HasForeignKey(d => d.CandidateId)
                        .OnDelete(DeleteBehavior.Cascade); // Keep this

            modelBuilder.Entity<CandidateDocumentDetail>()
                        .HasOne(d => d.DocumentType)
                        .WithMany(t => t.CandidateDocuments)
                        .HasForeignKey(d => d.DocumentTypeId)
                        .OnDelete(DeleteBehavior.Restrict); // Disable cascade

            modelBuilder.Entity<JobApplication>()
                        .HasOne(a => a.Candidate)
                        .WithMany(c => c.JobApplications)
                        .HasForeignKey(a => a.CandidateId)
                        .OnDelete(DeleteBehavior.Cascade); // Allow cascade from Candidate → Application

            modelBuilder.Entity<JobApplication>()
                        .HasOne(a => a.Job)
                        .WithMany(j => j.Applications)
                        .HasForeignKey(a => a.JobId)
                        .OnDelete(DeleteBehavior.Restrict); // Prevent cascade from Job → Application

            modelBuilder.Entity<OfferLetter>()
                        .HasOne(o => o.Candidate)
                        .WithMany(c => c.OfferLetters)
                        .HasForeignKey(o => o.CandidateId)
                        .OnDelete(DeleteBehavior.Cascade); // Allow cascade from Candidate

            modelBuilder.Entity<OfferLetter>()
                        .HasOne(o => o.Job)
                        .WithMany(j => j.OfferLetters)
                        .HasForeignKey(o => o.JobId)
                        .OnDelete(DeleteBehavior.Restrict); // Prevent cascade from Job

        }
    }
}
