using HireFlow_MVC.Models;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "HR")]
        [HttpGet]
        public IActionResult PostJob()
        {
            return View(new JobViewModel());
        }
        [Authorize(Roles = "HR")]
        [HttpPost]
        public async Task<JsonResult> PostJob([FromBody] JobViewModel model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid job data." });

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Jobs/", model);

                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Job posted successfully!" });

                var errorContent = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = $"Failed to post job: {errorContent}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Exception: {ex.Message}" });
            }
        }


        [Authorize(Roles = "Candidate")]
        [HttpGet]
        public async Task<IActionResult> AllJobs()
        {

            var response = await _httpClient.GetAsync("api/Jobs/");

            if (!response.IsSuccessStatusCode)
            {
                // Return empty list with 500 status
                return StatusCode(500, new { message = "Failed to fetch jobs from API." });
            }

            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<JobViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(jobs);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            var response = await _httpClient.GetAsync("api/Jobs/");

            if (!response.IsSuccessStatusCode)
            {
                // Return empty list with 500 status
                return StatusCode(500, new { message = "Failed to fetch jobs from API." });
            }

            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<JobViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Return JSON instead of a View
            return Json(jobs);
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

    

