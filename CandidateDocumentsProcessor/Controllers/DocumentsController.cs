using CandidateDocumentsProcessor.Application.Services;
using CandidateDocumentsProcessor.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CandidateDocumentsProcessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT protected
    public class DocumentsController : ControllerBase
    {
        private readonly DocumentService _service;

        public DocumentsController(DocumentService service) => _service = service;

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] Guid candidateId, [FromForm] string documentType, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploads, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            var blobUrl = $"/uploads/{fileName}";
            var doc = await _service.UploadAsync(candidateId, documentType, file.FileName, blobUrl, file.ContentType ?? "application/octet-stream", file.Length);

            return Ok(doc);
        }

        [HttpGet("candidate/{candidateId}")]
        public async Task<IActionResult> GetByCandidate(Guid candidateId)
        {
            var docs = await _service.GetDocumentsAsync(candidateId);
            return Ok(docs);
        }

        [HttpPost("verify/{id}")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> Verify(Guid id, [FromQuery] VerificationStatus status, [FromQuery] string? remarks)
        {
            await _service.VerifyAsync(id, status, remarks);
            return Ok(new { message = "Verification updated." });
        }
    }
}
