using HireFlow_MVC.Models; // DTOs for MVC (same as API DTOs or mapped versions)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HireFlow_MVC.Controllers
{

    [Authorize(Roles = "HR")]
    public class InterviewsController : Controller
    {
        private readonly HttpClient _httpClient;

        public InterviewsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("HireFlowAPI");
        }

        // GET: /InterviewsMvc
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/interviews");
            if (!response.IsSuccessStatusCode)
                return View(new List<InterviewDetailsDTO>());

            var json = await response.Content.ReadAsStringAsync();
            var interviews = JsonSerializer.Deserialize<List<InterviewDetailsDTO>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(interviews);
        }

        // GET: /InterviewsMvc/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/interviews/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var interview = JsonSerializer.Deserialize<InterviewDetailsDTO>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(interview);
        }

        // GET: /InterviewsMvc/Create
        public IActionResult Create()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Room(string id)
        {
            return View(model: id);
        }

        // POST: /InterviewsMvc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InterviewDetailsDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/interviews", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error creating interview");
            return View(dto);
        }

        // GET: /InterviewsMvc/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/interviews/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var interview = JsonSerializer.Deserialize<InterviewDetailsDTO>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(interview);
        }

        // POST: /InterviewsMvc/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, InterviewDetailsDTO dto)
        {
            if (id != dto.InterviewId)
                return BadRequest();

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/interviews/{id}", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error updating interview");
            return View(dto);
        }

        // GET: /InterviewsMvc/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/interviews/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var interview = JsonSerializer.Deserialize<InterviewDetailsDTO>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(interview);
        }

        // POST: /InterviewsMvc/DeleteConfirmed/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/interviews/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error deleting interview");
            return RedirectToAction(nameof(Index));
        }
    }
}
