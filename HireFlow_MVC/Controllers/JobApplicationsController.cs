using HireFlow_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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
        public async Task<IActionResult> ViewApplication(Guid id)
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

        // POST: /JobApplications/NewApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewApplication(JobApplicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/JobApplications", model);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(ListAllApplications));
            }
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
    }
}
