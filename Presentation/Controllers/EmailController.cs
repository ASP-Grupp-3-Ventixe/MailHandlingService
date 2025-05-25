using Application.DTOs;
using Application.Responses;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/emails")]
[Authorize]
public class EmailsController(IEmailService emailService) : ControllerBase
{
    private readonly IEmailService _emailService = emailService;
    
    [HttpPost]
    public async Task<ActionResult<EmailResult<EmailDto>>> CreateEmail(CreateEmailDto emailDto)
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"[CLAIM] {claim.Type}: {claim.Value}");
        }
        
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            Console.WriteLine($"[DEBUG] userId: {userId}");

            var result = await _emailService.CreateEmailAsync(emailDto, userId);

            if (!result.Succeeded)
                return StatusCode(result.StatusCode ?? 500, result);

            return CreatedAtAction(nameof(GetEmail), new { id = result.Result.Id }, result);
        }
        catch (Exception ex)
        {

            Console.WriteLine($"[ERROR] {ex}");
            throw; 
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<EmailListResult<EmailDto>>> GetEmails(
        [FromQuery] string folder = "inbox",
        [FromQuery] bool unreadOnly = false,
        [FromQuery] string? searchQuery = null)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.GetEmailsAsync(userId, folder, unreadOnly, searchQuery);
    
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
    
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<EmailNavigationResult<EmailDetailsDto>>> GetEmail(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.GetEmailByIdAsync(id, userId);

        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
            
        return Ok(result);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> SoftDeleteEmail(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.SoftDeleteEmailAsync(id, userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return NoContent();
    }
    
    [HttpDelete("{emailId}/permanent")]
    public async Task<ActionResult> HardDeleteEmail(Guid emailId)
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.HardDeleteEmailAsync(emailId, userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return Ok(result);
    }
    
    [HttpDelete("trash/empty")]
    public async Task<ActionResult> EmptyTrash()
    {
        var userId = GetCurrentUserId();
        var result = await _emailService.EmptyTrashAsync(userId);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode ?? 500, result);
        
        return Ok(result);
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
