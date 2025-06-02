using Application.DTOs;
using Application.Responses;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Presentation.Controllers;

[ApiController]
[Route("api/emails")]
// [Authorize] // Uncomment to require authentication for all email operations
public class EmailsController(IEmailService emailService, ILogger<EmailsController> logger) : ControllerBase
{
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<EmailsController> _logger = logger;
    
    [HttpPost]
    [SwaggerOperation(Summary = "Creates a new email")]
    [SwaggerResponse(201, "Email created successfully")]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerRequestExample(typeof(CreateEmailDto), typeof(CreateEmailDtoExample))]
    public async Task<ActionResult<EmailResult<EmailDto>>> CreateEmail(CreateEmailDto emailDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Message = "Invalid data.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        try
        {
            var userId = GetCurrentUserId();
            var result = await _emailService.CreateEmailAsync(emailDto, userId);

            if (!result.Succeeded)
             return StatusCode(result.StatusCode ?? 500, new { Message = "Failed to create email.", result.Error }); 

            return CreatedAtAction(nameof(GetEmail), new { id = result.Result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating email.");
            return StatusCode(500, new EmailResult<EmailDto> { Succeeded = false, StatusCode = 500, Error = "Unexpected error." });
        }
    }
    
    [HttpGet]
    [SwaggerOperation(Summary = "Gets all emails for the current user")]
    [SwaggerResponse(200, "List of emails returned", typeof(EmailListResult<EmailDto>))]
    [SwaggerResponse(500, "Unexpected error")]
    public async Task<ActionResult<EmailListResult<EmailDto>>> GetEmails(
        [FromQuery] string folder = "inbox",
        [FromQuery] bool unreadOnly = false,
        [FromQuery] string? searchQuery = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _emailService.GetEmailsAsync(userId, folder, unreadOnly, searchQuery);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, new { Message = "Failed to fetch emails.", result.Error });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching emails.");
            return StatusCode(500, new EmailListResult<EmailDto> { Succeeded = false, StatusCode = 500, Error = "Unexpected error." });
        }
    }
    
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Gets a specific email by id")]
    [SwaggerResponse(200, "Email details returned", typeof(EmailNavigationResult<EmailDetailsDto>))]
    [SwaggerResponse(403, "You do not have permission to access this email")]
    [SwaggerResponse(404, "Email not found")]
    public async Task<ActionResult<EmailNavigationResult<EmailDetailsDto>>> GetEmail(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _emailService.GetEmailByIdAsync(id, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, new { Message = "Failed to fetch email by id.", result.Error });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching email by id.");
            return StatusCode(500, new EmailNavigationResult<EmailDetailsDto> { Succeeded = false, StatusCode = 500, Error = "Unexpected error." });
        }
    }
    
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Soft-deletes (moves to trash) an email")]
    [SwaggerResponse(204, "Email moved to trash")]
    [SwaggerResponse(403, "You don't have permission to move this email.")]
    [SwaggerResponse(404, "Email not found")]
    public async Task<ActionResult> SoftDeleteEmail(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _emailService.SoftDeleteEmailAsync(id, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, new { Message = "Failed to soft-delete email.", result.Error });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error soft-deleting email.");
            return StatusCode(500, new { Message = "Unexpected error." });
        }
    }
    
    [HttpDelete("{emailId}/permanent")]
    [SwaggerOperation(Summary = "Permanently deletes an email")]
    [SwaggerResponse(200, "Email permanently deleted")]
    [SwaggerResponse(403, "You don't have permission to delete this email.")]
    [SwaggerResponse(404, "Email not found")]
    public async Task<ActionResult> HardDeleteEmail(Guid emailId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _emailService.HardDeleteEmailAsync(emailId, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, new { Message = "Failed to permanently delete email.", result.Error });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error permanently deleting email.");
            return StatusCode(500, new { Message = "Unexpected error." });
        }
    }
    
    [HttpDelete("trash/empty")]
    [SwaggerOperation(Summary = "Empties the trash folder for the current user")]
    [SwaggerResponse(200, "Trash emptied")]
    [SwaggerResponse(500, "Unexpected error")]
    public async Task<ActionResult> EmptyTrash()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _emailService.EmptyTrashAsync(userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, new { Message = "Failed to empty trash.", result.Error });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error emptying trash.");
            return StatusCode(500, new { Message = "Unexpected error." });
        }
    }
    
    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.MarkAsReadAsync(id, userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return NoContent();
    }

    [HttpPut("{id}/unread")]
    public async Task<ActionResult> MarkAsUnread(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.MarkAsUnreadAsync(id, userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return NoContent();
    }

    [HttpPost("{id}/reply")]
    public async Task<ActionResult<EmailResult<EmailDto>>> ReplyToEmail(Guid id, CreateReplyDto replyDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var userId = GetCurrentUserId();
        var result = await _emailService.ReplyToEmailAsync(id, replyDto);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
            
        return CreatedAtAction(nameof(GetEmail), new { id = result.Result.Id }, result);
    }

    [HttpPost("{id}/forward")]
    public async Task<ActionResult<EmailResult<EmailDto>>> ForwardEmail(Guid id,  CreateForwardDto forwardDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var userId = GetCurrentUserId();
        var result = await _emailService.ForwardEmailAsync(id, forwardDto);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
            
        return CreatedAtAction(nameof(GetEmail), new { id = result.Result.Id }, result);
    }

    [HttpPut("{id}/folder")]
    public async Task<ActionResult> MoveToFolder(Guid id, [FromBody] MoveToFolderRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var userId = GetCurrentUserId();
        var result = await _emailService.MoveToFolderAsync(id, userId, request.FolderName);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return NoContent();
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<CountResult>> GetUnreadCount([FromQuery] string folder = "inbox")
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.GetUnreadCountAsync(userId, folder);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
            
        return Ok(result);
    }

    [HttpGet("starred")]
    public async Task<ActionResult<EmailListResult<EmailDto>>> GetStarredEmails()
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.GetStarredEmailsAsync(userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
            
        return Ok(result);
    }

    [HttpPost("{id}/star")]
    public async Task<ActionResult> StarEmail(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.StarEmailAsync(id, userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return NoContent();
    }

    [HttpDelete("{id}/star")]
    public async Task<ActionResult> UnstarEmail(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.UnstarEmailAsync(id, userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return NoContent();
    }
    

    private Guid GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("nameid")?.Value;
        return Guid.TryParse(userIdClaim, out var guid) ? guid : Guid.Empty;
    }
}
public class MoveToFolderRequest
{
    public string FolderName { get; set; } = null!;
}
