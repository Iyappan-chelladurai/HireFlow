using HireFlow_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HireFlow_MVC.Controllers
{
    public class JobApplicationsController : Controller
    {
        private readonly HttpClient _httpClient;

        public JobApplicationsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("HireFlowAPI");
        }


        // GET: /JobApplications
        [HttpGet]
        public  IActionResult ListAllApplications(Guid? JobId = null)
        {
            //var applications = await _httpClient.GetFromJsonAsync<List<JobApplicationViewModel>>($"api/JobApplications/{JobId}");
            //return View(applications);
            return View();
        }

        // GET: /JobApplications/ViewApplication/{id}
        public async Task<IActionResult> ViewApplication(Guid? id)
        {
            var application = await _httpClient.GetFromJsonAsync<JobApplicationViewModel>($"api/JobApplications/{id}");
            if (application == null)
                return NotFound();

            return View(application);
        }

        // GET: /JobApplications/NewApplication
        public IActionResult NewApplication()
        {
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> NewApplication([FromForm] JobApplicationViewModel model)
        {
            var candidateIdString = HttpContext.Session.GetString("CandidateId");
            if (!string.IsNullOrEmpty(candidateIdString) && Guid.TryParse(candidateIdString, out Guid candidateId))
                model.CandidateId = candidateId;
    
            // Create multipart form content
            var content = new MultipartFormDataContent();

            // Add text fields
            content.Add(new StringContent(model.CandidateId.ToString()), "CandidateId");
            content.Add(new StringContent(model.JobId.ToString()), "JobId");
            content.Add(new StringContent(model.CurrentJobTitle ?? ""), "CurrentJobTitle");
            content.Add(new StringContent(model.TotalExperienceYears?.ToString() ?? "0"), "TotalExperienceYears");
            content.Add(new StringContent(model.NoticePeriodDays.ToString()), "NoticePeriodDays");
            content.Add(new StringContent(model.EducationLevel ?? ""), "EducationLevel");
            content.Add(new StringContent(model.AvailableFrom ?? ""), "AvailableFrom");
            content.Add(new StringContent(model.ExpectedSalary ?? ""), "ExpectedSalary");
            content.Add(new StringContent(model.PreferredLocation ?? ""), "PreferredLocation");
            content.Add(new StringContent(model.Skills ?? ""), "Skills");

            // Add file (resume)
            if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                var fileContent = new StreamContent(model.ResumeFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ResumeFile.ContentType);
                content.Add(fileContent, "ResumeFile", model.ResumeFile.FileName);
            }
 

            var response = await _httpClient.PostAsync("api/JobApplications", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(ListAllApplications));

            TempData["ErrorMessage"] = "Failed to submit application. Please try again.";
            return View(model);
        }



        // GET: /JobApplications/EditApplication/{id}
        public async Task<IActionResult> EditApplication(Guid id)
        {
            var application = await _httpClient.GetFromJsonAsync<JobApplicationViewModel>($"api/JobApplications/{id}");
            if (application == null)
                return NotFound();

            return View(application);
        }

        // POST: /JobApplications/EditApplication/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditApplication(Guid id, JobApplicationViewModel model)
        {
            if (id != model.ApplicationId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"api/JobApplications/{id}", model);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(ListAllApplications));
            }
            return View(model);
        }

        // GET: /JobApplications/ConfirmDeleteApplication/{id}
        public async Task<IActionResult> ConfirmDeleteApplication(Guid id)
        {
            var application = await _httpClient.GetFromJsonAsync<JobApplicationViewModel>($"api/JobApplications/{id}");
            if (application == null)
                return NotFound();

            return View(application);
        }

        // POST: /JobApplications/DeleteApplication/{id}
        [HttpPost, ActionName("DeleteApplication")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/JobApplications/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(ListAllApplications));

            return BadRequest();
        }

        [Authorize(Roles = "Candidate")]
        public IActionResult ViewApplicationStatus()
        {
           
            return View();
        }


    }
}
