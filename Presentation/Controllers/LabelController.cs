using System;
using System.Threading.Tasks;
using MailHandlingServiceProvider.Business.DTOs;
using MailHandlingServiceProvider.Business.Responses;
using MailHandlingServiceProvider.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace MailHandlingServiceProvider.Presentation.Controllers
{
    [ApiController]
    [Route("api/labels")]
    // [Authorize]
    public class LabelsController(ILabelService labelService) : ControllerBase
    {
        private readonly ILabelService _labelService = labelService;

        [HttpGet]
        public async Task<ActionResult<EmailListResult<LabelDto>>> GetLabels()
        {
            var userId = GetCurrentUserId();
            var result = await _labelService.GetLabelsAsync(userId);
            
            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LabelResult<LabelDto>>> GetLabelById(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _labelService.GetLabelByIdAsync(id, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<LabelResult<LabelDto>>> CreateLabel([FromBody] CreateLabelDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var userId = GetCurrentUserId();
            var result = await _labelService.CreateLabelAsync(createDto, userId);
            
            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return CreatedAtAction(nameof(GetLabelById), new { id = result.Result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<LabelResult<LabelDto>>> UpdateLabel(Guid id, [FromBody] UpdateLabelDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var userId = GetCurrentUserId();
            var result = await _labelService.UpdateLabelAsync(id, updateDto, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLabel(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _labelService.DeleteLabelAsync(id, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return NoContent();
        }

        [HttpPost("emails/{emailId}/labels/{labelId}")]
        public async Task<ActionResult> AddLabelToEmail(Guid emailId, Guid labelId)
        {
            var userId = GetCurrentUserId();
            var result = await _labelService.AddLabelToEmailAsync(emailId, labelId, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return NoContent();
        }

        [HttpDelete("emails/{emailId}/labels/{labelId}")]
        public async Task<ActionResult> RemoveLabelFromEmail(Guid emailId, Guid labelId)
        {
            var userId = GetCurrentUserId();
            var result = await _labelService.RemoveLabelFromEmailAsync(emailId, labelId, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return NoContent();
        }

        [HttpGet("{id}/emails")]
        public async Task<ActionResult<EmailListResult<EmailDto>>> GetEmailsByLabelId(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _labelService.GetEmailsByLabelIdAsync(id, userId);
            
            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);
                
            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            // for testing, return a fixed GUID
            return Guid.Parse("00000000-0000-0000-0000-000000000002");
        
            // return Guid.Parse(User.FindFirst("sub")?.Value ?? "");
        }
    }
}