using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Interfaces.Meeting;
using System.Security.Claims;

namespace Paperless_Meeting.Controllers.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Upload tài liệu cho một cuộc họp
        /// </summary>
        /// <param name="meetingId">ID của cuộc họp</param>
        /// <param name="uploadDto">Thông tin tài liệu cần upload</param>
        /// <returns>Thông tin tài liệu đã được tải lên</returns>
        [HttpPost("meeting/{meetingId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(int meetingId, [FromForm] DocumentUploadDto uploadDto)
        {
            try
            {
                if (uploadDto.File == null || uploadDto.File.Length == 0)
                    return BadRequest("Không có tệp được chọn.");

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var document = await _documentService.UploadDocumentAsync(meetingId, uploadDto, userId);
                
                return CreatedAtAction(nameof(GetDocument), new { documentId = document.DocumentId }, 
                    new { message = "Tải lên thành công.", document });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi tải lên: {ex.Message}");
            }
        }

        /// <summary>
        /// Upload nhiều tài liệu cho một cuộc họp
        /// </summary>
        /// <param name="meetingId">ID của cuộc họp</param>
        /// <param name="files">Danh sách file cần upload</param>
        /// <param name="visibility">Mức độ hiển thị: "Chung" hoặc "NoiBo"</param>
        /// <returns>Danh sách tài liệu đã được tải lên</returns>
        [HttpPost("meeting/{meetingId}/batch")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMultipleDocuments(
            int meetingId, 
            [FromForm] List<IFormFile> files,
            [FromForm] string visibility = "Chung")
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest("Không có tệp được chọn.");

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var uploadedDocuments = new List<object>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var uploadDto = new DocumentUploadDto
                        {
                            File = file,
                            FileName = file.FileName,
                            Visibility = visibility
                        };

                        try
                        {
                            var document = await _documentService.UploadDocumentAsync(meetingId, uploadDto, userId);
                            uploadedDocuments.Add(document);
                        }
                        catch (Exception ex)
                        {
                            uploadedDocuments.Add(new { fileName = file.FileName, error = ex.Message });
                        }
                    }
                }

                return Ok(new { 
                    message = "Quá trình tải lên hoàn tất.", 
                    uploadedCount = uploadedDocuments.Count, 
                    documents = uploadedDocuments 
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi tải lên: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tất cả tài liệu của một cuộc họp
        /// </summary>
        /// <param name="meetingId">ID của cuộc họp</param>
        /// <returns>Danh sách tài liệu</returns>
        [HttpGet("meeting/{meetingId}")]
        public async Task<IActionResult> GetDocumentsByMeeting(int meetingId)
        {
            var documents = await _documentService.GetDocumentsByMeetingAsync(meetingId);
            return Ok(documents);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một tài liệu
        /// </summary>
        /// <param name="documentId">ID của tài liệu</param>
        /// <returns>Thông tin tài liệu</returns>
        [HttpGet("{documentId}")]
        public async Task<IActionResult> GetDocument(int documentId)
        {
            var document = await _documentService.GetDocumentByIdAsync(documentId);
            if (document == null)
                return NotFound("Không tìm thấy tài liệu.");
            
            return Ok(document);
        }

        /// <summary>
        /// Xóa một tài liệu
        /// </summary>
        /// <param name="documentId">ID của tài liệu</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var success = await _documentService.DeleteDocumentAsync(documentId, userId);
                
                if (!success)
                    return NotFound("Không tìm thấy tài liệu.");
                
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xóa: {ex.Message}");
            }
        }
    }
}