using Amazon.Comprehend.Model;
using HireFlow_API.Controllers;
using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace HireFlow_API.Repositories
{
    public interface IJobApplicationRepository
    {

        Task<IEnumerable<JobApplicationDTO>> GetAllApplicationsAsync(Guid JobId);
        Task<JobApplicationDTO?> GetApplicationByIdAsync(Guid applicationId);
        Task<Guid> AddNewApplicationAsync(JobApplicationDTO jobApplicationDto);
        Task UpdateApplicationInfoAsync(JobApplicationDTO jobApplicationDto);
        Task DeleteApplicationByIdAsync(Guid applicationId);
        Task<bool> IsApplicationExistsAsync(Guid applicationId);

        Task<IList<JobApplicationResponseDTO>> GetByCandidateIdAsync(Guid candidateId);

        Task<IList<CandidateDisplayDto>> GetCandidatesForJobAsync(Guid jobId);

        Task<IList<CandidateDropdownDto>> GetCandidatesForDropdownAsync();

        Task<IList<InterviewerDropdownDto>> GetInterviewersForDropdownAsync();

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

        public async Task<Guid> AddNewApplicationAsync(JobApplicationDTO jobApplicationDto)
        {

            var entity = new JobApplication
            {
                ApplicationId = Guid.NewGuid(),
                CandidateId = jobApplicationDto.CandidateId,
                JobId = jobApplicationDto.JobId,
                ResumePath = jobApplicationDto.ResumePath,
                AppliedOn = DateTime.Now,
                ApplicationStatus = JobApplicationStatus.Applied.ToString(),
            };


            _context.JobApplications.Add(entity);
            await _context.SaveChangesAsync();

            return entity.ApplicationId;
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

        public async Task<IList<JobApplicationResponseDTO>> GetByCandidateIdAsync(Guid candidateId)
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
                     JobTitle = app.Job.JobTitle,
                     Department = app.Job.Department,
                     Location = app.Job.Location,
                     CurrentJobTitle = app.Candidate.CurrentJobTitle,

                 }).ToListAsync();

            return data;
        }
        public async Task<IList<CandidateDisplayDto>> GetCandidatesForJobAsync(Guid jobId)
        {
            IList < CandidateDisplayDto > candidates = new List<CandidateDisplayDto>();
            try
            {
               candidates = await (
                                              from app in _context.JobApplications
                                              join c in _context.CandidateDetails on app.CandidateId equals c.CandidateId
                                              join u in _context.Users on c.UserId equals u.Id
                                              join j in _context.Jobs on app.JobId equals j.JobId
                                              join cs in _context.tbl_CandidatesJobScore on app.ApplicationId equals cs.JobApplicationId into csGroup
                                              from cs in csGroup.DefaultIfEmpty() // Left join, in case score is missing
                                              where (jobId == Guid.Empty ? 1 == 1 : app.JobId == jobId)
                                              select new CandidateDisplayDto
                                              {

                                                  ApplicationId = app.ApplicationId,
                                                  CandidateName = u.FullName,
                                                  CandidateEmail = u.Email,
                                                  ApplicationStatus = app.ApplicationStatus,
                                                  JobTitle = j.JobTitle,
                                                  EducationLevel = c.EducationLevel ?? "",
                                                  AppliedOn = app.AppliedOn,
                                                  Skills = c.Skills ?? "",
                                                  MatchScore = cs.OverallFitScore,
                                                  ExpectedSalary = c.ExpectedSalary ?? 0m,
                                                  Department = j.Department,
                                                  Location = j.Location

                                              }
                                          ).ToListAsync();

            }
            catch (Exception ex)
            {
                throw ex;

            }

            return candidates;
        }

        public async Task<IList<CandidateDropdownDto>> GetCandidatesForDropdownAsync()
        {
            return await _context.JobApplications
                .Include(a => a.Candidate.User)
                .Include(a => a.Job)
                .Select(a => new CandidateDropdownDto
                {
                    ApplicationId = a.ApplicationId,
                    CandidateName = a.Candidate.User.FullName,
                    JobTitle = a.Job.JobTitle
                }).Distinct().ToListAsync();
        }
        public async Task<IList<InterviewerDropdownDto>> GetInterviewersForDropdownAsync()
        {
            var result = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == "HR" || role.Name == "Interviewer"
                select new InterviewerDropdownDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Role = role.Name
                }
            ).ToListAsync();

            return result;
        }


    }

    public class CandidateDisplayDto
    {
        public Guid ApplicationId { get; set; }

        // Candidate Info
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string EducationLevel { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty; // can be CSV or JSON depending on frontend

        // Job Info
        public string JobTitle { get; set; } = string.Empty;

        // Application Info
        public string ApplicationStatus { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
        public DateTime AppliedOn { get; set; }
        public decimal? ExpectedSalary { get; set; }

        // Candidate Score
        public int? MatchScore { get; set; }

        public string? Location { get; set; }
    }
}
