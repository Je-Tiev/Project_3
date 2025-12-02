using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Document;
using Paperless_Meeting.Repositories.Document;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Services.Document;

public class DocumentService : IDocumentService
{
    private readonly PaperlessMeetingDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        PaperlessMeetingDbContext context,
        IWebHostEnvironment environment,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<DocumentDto> UploadDocumentAsync(int meetingId, DocumentUploadDto uploadDto, int userId)
    {
        // Kiểm tra meeting có tồn tại không
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) throw new KeyNotFoundException($"Meeting với ID {meetingId} không tồn tại");
        
        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId 
                                       && mp.UserId == userId 
                                       && (mp.RoleInMeeting == MeetingParticipant.MeetingRole.Host 
                                           || mp.RoleInMeeting == MeetingParticipant.MeetingRole.Member));

        if (participant == null) throw new UnauthorizedAccessException("Chỉ Host và Member mới có quyền upload tài liệu");

        if (uploadDto.File == null || uploadDto.File.Length == 0) throw new ArgumentException("File không hợp lệ");
        
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var originalFileName = Path.GetFileName(uploadDto.File.FileName);
        
        var fileExtension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var uniqueFileName = $"{fileNameWithoutExtension}_{Guid.NewGuid()}{fileExtension}";
        
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadDto.File.CopyToAsync(fileStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu file lên server");
            throw new Exception("Không thể lưu file lên server", ex);
        }

        if (!Enum.TryParse<Models.Document.DocumentVisibility>(uploadDto.Visibility, out var visibility))
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            throw new ArgumentException("Visibility không hợp lệ. Chỉ chấp nhận 'Chung' hoặc 'NoiBo'");
        }

        var document = new Models.Document
        {
            MeetingId = meetingId,
            FileName = originalFileName,
            FilePath = $"/uploads/documents/{uniqueFileName}",
            Visibility = visibility,
            UploadedBy = userId,
            UploadDate = DateTime.UtcNow
        };

        try
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Document {document.DocumentId} uploaded successfully by user {userId}");

            return MapToDto(document);
        }
        catch (Exception ex)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            _logger.LogError(ex, "Lỗi khi lưu thông tin document vào database");
            throw new Exception("Không thể lưu thông tin tài liệu vào database", ex);
        }
    }
    
    public async Task<List<DocumentDto>> UploadMultipleDocumentsAsync(int meetingId, DocumentMultipleUploadDto uploadDto, int userId)
    {
        if (uploadDto.Files == null || !uploadDto.Files.Any())
        {
            throw new ArgumentException("Danh sách file rỗng");
        }

        var results = new List<DocumentDto>();
    
        foreach (var file in uploadDto.Files)
        {
            var singleUploadDto = new DocumentUploadDto 
            { 
                File = file, 
                Visibility = uploadDto.Visibility 
            };
        
            var result = await UploadDocumentAsync(meetingId, singleUploadDto, userId);
            results.Add(result);
        }
    
        return results;
    }
    
    public async Task<DocumentDto> UploadDocumentStreamAsync(int meetingId, Stream fileStream, string contentType, string visibility, int userId)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) throw new KeyNotFoundException($"Meeting {meetingId} không tồn tại");
    
        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId 
                                       && mp.UserId == userId 
                                       && (mp.RoleInMeeting == MeetingParticipant.MeetingRole.Host 
                                           || mp.RoleInMeeting == MeetingParticipant.MeetingRole.Member));

        if (participant == null) throw new UnauthorizedAccessException("Chỉ Host và Member mới có quyền upload tài liệu");

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"stream_{Guid.NewGuid()}{GetExtensionFromContentType(contentType)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        var document = new Models.Document
        {
            MeetingId = meetingId,
            FileName = fileName,
            FilePath = $"/uploads/documents/{fileName}",
            Visibility = Enum.Parse<Models.Document.DocumentVisibility>(visibility),
            UploadedBy = userId,
            UploadDate = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return MapToDto(document);
    }

    private string GetExtensionFromContentType(string contentType)
    {
        return contentType switch
        {
            "application/pdf" => ".pdf",
            "application/msword" => ".doc",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            "application/vnd.ms-excel" => ".xls",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
            "application/vnd.ms-powerpoint" => ".ppt",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
            "text/plain" => ".txt",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            _ => ".bin"
        };
    }
    
    public async Task<bool> DeleteDocumentAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .Include(d => d.Meeting)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new KeyNotFoundException($"Document với ID {documentId} không tồn tại");
        }

        var isUploader = document.UploadedBy == userId;
        var isCreator = document.Meeting.CreatedByUserId == userId;

        if (!isUploader && !isCreator)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa tài liệu này");
        }

        var physicalPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Document {documentId} deleted by user {userId}");

        return true;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByMeetingAsync(int meetingId)
    {
        var documents = await _context.Documents
            .Where(d => d.MeetingId == meetingId)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(int documentId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        return document != null ? MapToDto(document) : null;
    }

    private DocumentDto MapToDto(Models.Document document)
    {
        return new DocumentDto
        {
            DocumentId = document.DocumentId,
            FileName = document.FileName,
            FilePath = document.FilePath,
            Visibility = document.Visibility.ToString(),
            UploadedBy = document.UploadedBy,
            UploadDate = document.UploadDate
        };
    }
}