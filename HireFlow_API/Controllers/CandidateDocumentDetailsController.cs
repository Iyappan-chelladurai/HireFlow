using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Services;

namespace HireFlow_API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    class CandidateDocumentDetailsController : ControllerBase
    {
        private readonly ICandidateDocumentsService _service;

        public CandidateDocumentDetailsController(ICandidateDocumentsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CandidateDocumentDetail>>> GetCandidateDocumentDetails()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CandidateDocumentDetail>> GetCandidateDocumentDetail(Guid id)
        {
            var detail = await _service.GetByIdAsync(id);
            if (detail == null)
                return NotFound();

            return Ok(detail);
        }

        [HttpPost]
        public async Task<ActionResult> PostCandidateDocumentDetail(CandidateDocumentDetail candidateDocumentDetail)
        {
            await _service.AddAsync(candidateDocumentDetail);
            return CreatedAtAction(nameof(GetCandidateDocumentDetail), new { id = candidateDocumentDetail.DocumentDetailId }, candidateDocumentDetail);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCandidateDocumentDetail(Guid id, CandidateDocumentDetail candidateDocumentDetail)
        {
            var updated = await _service.UpdateAsync(id, candidateDocumentDetail);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandidateDocumentDetail(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }

}
