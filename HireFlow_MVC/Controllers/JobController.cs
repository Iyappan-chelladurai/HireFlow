using HireFlow_MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace HireFlow_MVC.Controllers
{
    public class JobController : Controller
    {
        private readonly HttpClient _httpClient;

        public JobController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("HireFlowAPI");
        }

        [HttpGet]
        public IActionResult PostJob()
        {
            return View(new JobViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> PostJob(JobViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/Jobs/", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Job posted successfully!";
                return RedirectToAction("PostJob");
            }

            ModelState.AddModelError("", "Failed to post job.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AllJobs()
        {
            var response = await _httpClient.GetAsync("api/Jobs/");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to fetch jobs from API.";
                return View(new List<JobViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<JobViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(jobs);
        }

        [HttpGet]
        public async Task<IActionResult> JobDetails(Guid id)
        {
      
            var response = await _httpClient.GetAsync($"api/jobs/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var job = JsonSerializer.Deserialize<JobViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(job);
        }


    }
}

    

