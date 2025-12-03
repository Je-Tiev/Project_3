using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Document;
using Paperless_Meeting.Repositories.Document;
using System.Security.Claims;

namespace Paperless_Meeting.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IDocumentService documentService,
        ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Upload tài liệu cho cuộc họp
    /// </summary>
    /// <param name="meetingId">ID của cuộc họp</param>
    /// <param name="uploadDto">Thông tin file upload</param>
    /// <returns>Thông tin tài liệu đã upload</returns>
    [HttpPost("meeting/{meetingId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(int meetingId, [FromForm] DocumentUploadDto uploadDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var document = await _documentService.UploadDocumentAsync(meetingId, uploadDto, userId);
            
            return CreatedAtAction(
                nameof(GetDocumentById),
                new { id = document.DocumentId },
                document);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload document");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi upload tài liệu" });
        }
    }

    [HttpPost("meeting/{meetingId}/multiple")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMultipleDocuments(
        int meetingId, 
        [FromForm] DocumentMultipleUploadDto uploadDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var documents = await _documentService.UploadMultipleDocumentsAsync(meetingId, uploadDto, userId);
        
            return Ok(new { 
                message = $"Upload thành công {documents.Count} file", 
                data = documents 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload multiple documents");
            return StatusCode(500, new { message = ex.Message });
        }
    }
    
    [HttpPost("meeting/{meetingId}/stream")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 524288000)] // 500MB
    public async Task<IActionResult> UploadDocumentStream(int meetingId, [FromQuery] string visibility)
    {
        try
        {
            var userId = GetCurrentUserId();
            var document = await _documentService.UploadDocumentStreamAsync(meetingId, Request.Body, Request.ContentType, visibility, userId);
        
            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi streaming upload");
            return StatusCode(500, new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Xóa tài liệu
    /// </summary>
    /// <param name="id">ID của tài liệu</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _documentService.DeleteDocumentAsync(id, userId);
            
            return Ok(new { message = "Xóa tài liệu thành công" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa document");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa tài liệu" });
        }
    }

    /// <summary>
    /// Lấy danh sách tài liệu của một cuộc họp
    /// </summary>
    /// <param name="meetingId">ID của cuộc họp</param>
    /// <returns>Danh sách tài liệu</returns>
    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetDocumentsByMeeting(int meetingId)
    {
        try
        {
            var documents = await _documentService.GetDocumentsByMeetingAsync(meetingId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách documents");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách tài liệu" });
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết một tài liệu
    /// </summary>
    /// <param name="id">ID của tài liệu</param>
    /// <returns>Thông tin tài liệu</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocumentById(int id)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            
            if (document == null)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu" });
            }
            
            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin document");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin tài liệu" });
        }
    }

    /// <summary>
    /// Download tài liệu
    /// </summary>
    /// <param name="id">ID của tài liệu</param>
    /// <returns>File tài liệu</returns>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            
            if (document == null)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu" });
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart('/'));
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "File không tồn tại trên server" });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = GetContentType(document.FileName);
            
            return File(memory, contentType, document.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi download document");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải tài liệu" });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Không thể xác định user");
        }
        return userId;
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}