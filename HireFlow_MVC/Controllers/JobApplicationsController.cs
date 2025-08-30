using HireFlow_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
        public async Task<IActionResult> ListAllApplications()
        {
            var applications = await _httpClient.GetFromJsonAsync<List<JobApplicationViewModel>>("api/JobApplications");
            return View(applications);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewApplication([FromForm] JobApplicationViewModel model)
        {
            var candidateIdString = HttpContext.Session.GetString("CandidateId");
            if (!string.IsNullOrEmpty(candidateIdString) && Guid.TryParse(candidateIdString, out Guid candidateId))
                model.CandidateId = candidateId;

            var content = new MultipartFormDataContent();

            // Add text fields
            content.Add(new StringContent(model.CandidateId.ToString()), "CandidateId");
            content.Add(new StringContent(model.JobId.ToString()), "JobId");
            content.Add(new StringContent(model.CurrentJobTitle ?? ""), "CurrentJobTitle");
            content.Add(new StringContent(model.TotalExperienceYears?.ToString() ?? "0"), "TotalExperienceYears");

            // Add file
            if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                var stream = model.ResumeFile.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ResumeFile.ContentType);
                content.Add(fileContent, "ResumeFile", model.ResumeFile.FileName); // Must match API property name
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

        public async Task<IActionResult> ViewApplicationStatus()
        {
            var candidateIdString = HttpContext.Session.GetString("CandidateId");

            if (!string.IsNullOrEmpty(candidateIdString) && Guid.TryParse(candidateIdString, out Guid candidateId))
            {
                var response = await _httpClient.GetAsync($"api/JobApplications/candidate/{candidateId}");

                var content = await response.Content.ReadAsStringAsync();
                // Deserialize JSON -> List of ViewModels
                var applications = JsonConvert.DeserializeObject<List<JobApplicationViewModel>>(content);
                return View(applications);
            }
            else
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }
        }


    }
}
