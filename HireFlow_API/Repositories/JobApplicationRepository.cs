using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;

namespace HireFlow_API.Repositories
{
    public interface IJobApplicationRepository
    {
       
            Task<IEnumerable<JobApplicationDTO>> GetAllApplicationsAsync(Guid JobId);
            Task<JobApplicationDTO?> GetApplicationByIdAsync(Guid applicationId);
            Task AddNewApplicationAsync(JobApplicationDTO jobApplicationDto);
            Task UpdateApplicationInfoAsync(JobApplicationDTO jobApplicationDto);
            Task DeleteApplicationByIdAsync(Guid applicationId);
            Task<bool> IsApplicationExistsAsync(Guid applicationId);

        Task<IEnumerable<JobApplicationResponseDTO>> GetByCandidateIdAsync(Guid candidateId);

    }

    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobApplicationDTO>> GetAllApplicationsAsync(Guid jobId)
        {
            var applications = await _context.JobApplications
                .Where(a => a.JobId == jobId)
                .ToListAsync();

            // Map to DTO
            var applicationDTOs = applications.Select(entity => new JobApplicationDTO
            {
                ApplicationId = entity.ApplicationId,
                CandidateId = entity.CandidateId,
                JobId = entity.JobId,
                ResumePath = entity.ResumePath,
                AppliedOn = entity.AppliedOn,
                ApplicationStatus = entity.ApplicationStatus,
                InterviewFeedback = entity.InterviewFeedback,
                OfferSentOn = entity.OfferSentOn,
                IsOfferAccepted = entity.IsOfferAccepted,
                OnboardedOn = entity.OnboardedOn
            }).ToList();

            return applicationDTOs;
        }


        public async Task<JobApplicationDTO?> GetApplicationByIdAsync(Guid applicationId)
        {
            var entity = await _context.JobApplications.FindAsync(applicationId);
            return entity != null ? MapEntityToDTO(entity) : null;
        }

        public async Task AddNewApplicationAsync(JobApplicationDTO jobApplicationDto)
        {

            var entity = new JobApplication
            {
                ApplicationId = Guid.NewGuid(),
                CandidateId = jobApplicationDto.CandidateId,
                JobId = jobApplicationDto.JobId,
                ResumePath = jobApplicationDto.ResumePath,
                AppliedOn = DateTime.UtcNow,
                ApplicationStatus = JobApplicationStatus.Applied.ToString(),
            };

          
            _context.JobApplications.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateApplicationInfoAsync(JobApplicationDTO jobApplicationDto)
        {
            var existingEntity = await _context.JobApplications.FindAsync(jobApplicationDto.ApplicationId);

            if (existingEntity != null)
            {
                // Only update mutable fields
                existingEntity.ApplicationStatus = jobApplicationDto.ApplicationStatus;
                existingEntity.InterviewFeedback = jobApplicationDto.InterviewFeedback;
                existingEntity.OfferSentOn = jobApplicationDto.OfferSentOn;
                existingEntity.IsOfferAccepted = jobApplicationDto.IsOfferAccepted;
                existingEntity.OnboardedOn = jobApplicationDto.OnboardedOn;

                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Job Application not found.");
            }
        }

        public async Task DeleteApplicationByIdAsync(Guid applicationId)
        {
            var entity = await _context.JobApplications.FindAsync(applicationId);
            if (entity != null)
            {
                _context.JobApplications.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsApplicationExistsAsync(Guid applicationId)
        {
            return await _context.JobApplications.AnyAsync(a => a.ApplicationId == applicationId);
        }

        
        private JobApplicationDTO MapEntityToDTO(JobApplication entity)
        {
            return new JobApplicationDTO
            {
                ApplicationId = entity.ApplicationId,
                CandidateId = entity.CandidateId,
                JobId = entity.JobId,
                ResumePath = entity.ResumePath,
                AppliedOn = entity.AppliedOn,
                ApplicationStatus = entity.ApplicationStatus,
                InterviewFeedback = entity.InterviewFeedback,
                OfferSentOn = entity.OfferSentOn,
                IsOfferAccepted = entity.IsOfferAccepted,
                OnboardedOn = entity.OnboardedOn
            };
        }

        private JobApplication MapDTOToEntity(JobApplicationDTO dto)
        {
            return new JobApplication
            {
                ApplicationId = dto.ApplicationId,
                CandidateId = dto.CandidateId,
                JobId = dto.JobId,
                ResumePath = dto.ResumePath,
                AppliedOn = dto.AppliedOn,
                ApplicationStatus = dto.ApplicationStatus,
                InterviewFeedback = dto.InterviewFeedback,
                OfferSentOn = dto.OfferSentOn,
                IsOfferAccepted = dto.IsOfferAccepted,
                OnboardedOn = dto.OnboardedOn
            };
        }

        public async Task<IEnumerable<JobApplicationResponseDTO>> GetByCandidateIdAsync(Guid candidateId)
        {

            var data = await _context.JobApplications
                .Where(app => app.CandidateId == candidateId)
                .Include(app => app.Job)
                 .Select(app => new JobApplicationResponseDTO
                 {
                     ApplicationId = app.ApplicationId,
                     CandidateId = app.CandidateId,
                     JobId = app.JobId,
                     ResumePath = app.ResumePath,
                     AppliedOn = app.AppliedOn,
                     ApplicationStatus = app.ApplicationStatus,
                     InterviewFeedback = app.InterviewFeedback,
                     OfferSentOn = app.OfferSentOn,
                     IsOfferAccepted = app.IsOfferAccepted,
                     OnboardedOn = app.OnboardedOn,
                     JobTitle = app.Job.JobTitle
                 }).ToListAsync();

            return data;
        }

    }


}
