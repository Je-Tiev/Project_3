using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Document;
using Paperless_Meeting.Repositories.Document;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Services.Document;

public class DocumentService : IDocumentService
{
    private readonly PaperlessMeetingDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        PaperlessMeetingDbContext context,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetUploadsBasePath()
    {
        var configPath = _configuration["Storage:UploadPath"];
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            return configPath;
        }
        return Path.Combine(_environment.ContentRootPath, "Uploads");
    }

    public string GetDocumentPhysicalPath(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        return Path.Combine(GetUploadsBasePath(), "documents", fileName);
    }

    public async Task<bool> CanUserAccessDocumentAsync(int documentId, int userId, string? userRole)
    {
        if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(userRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var document = await _context.Documents
            .Include(d => d.Meeting)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null) return false;

        // Người tải lên hoặc người tạo cuộc họp luôn có quyền truy cập
        if (document.UploadedBy == userId || document.Meeting.CreatedByUserId == userId)
        {
            return true;
        }

        // Kiểm tra xem người dùng có phải là người tham dự cuộc họp không
        var isParticipant = await _context.MeetingParticipants
            .AnyAsync(mp => mp.MeetingId == document.MeetingId && mp.UserId == userId);

        return isParticipant;
    }

    public async Task<DocumentDto> UploadDocumentAsync(int meetingId, DocumentUploadDto uploadDto, int userId)
    {
        var meeting = await _context.Meetings.FindAsync(meetingId);
        if (meeting == null) throw new KeyNotFoundException($"Meeting với ID {meetingId} không tồn tại");
        
        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId 
                                       && mp.UserId == userId 
                                       && (mp.RoleInMeeting == MeetingParticipant.MeetingRole.Host 
                                           || mp.RoleInMeeting == MeetingParticipant.MeetingRole.Member));

        if (participant == null) throw new UnauthorizedAccessException("Chỉ Host và Member mới có quyền upload tài liệu");

        if (uploadDto.File == null || uploadDto.File.Length == 0) throw new ArgumentException("File không hợp lệ");

        // Giới hạn dung lượng tối đa 500MB
        if (uploadDto.File.Length > 524288000)
        {
            throw new ArgumentException("Dung lượng tệp vượt quá giới hạn cho phép (tối đa 500MB)");
        }

        var originalFileName = Path.GetFileName(uploadDto.File.FileName);
        var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();

        // 1. Whitelist phần mở rộng cho phép
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt", ".txt", ".png", ".jpg", ".jpeg"
        };

        if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException($"Định dạng tệp '{fileExtension}' không được hỗ trợ. Chỉ chấp nhận tài liệu văn phòng, PDF hoặc hình ảnh.");
        }

        // 2. Validate Magic Bytes (File Signature)
        using (var stream = uploadDto.File.OpenReadStream())
        {
            if (!ValidateMagicBytes(stream, fileExtension))
            {
                throw new ArgumentException("Nội dung tệp không khớp với phần mở rộng hoặc tệp bị hỏng/chứa mã không an toàn.");
            }
        }

        var uploadsFolder = Path.Combine(GetUploadsBasePath(), "documents");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await uploadDto.File.CopyToAsync(fileStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu file lên server tại {FilePath}", filePath);
            throw new Exception("Không thể lưu file lên server", ex);
        }

        if (!Enum.TryParse<Models.Document.DocumentVisibility>(uploadDto.Visibility, true, out var visibility))
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

            _logger.LogInformation("Document {DocumentId} uploaded successfully by user {UserId}", document.DocumentId, userId);
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

        var ext = GetExtensionFromContentType(contentType);
        var uploadsFolder = Path.Combine(GetUploadsBasePath(), "documents");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"stream_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        if (!Enum.TryParse<Models.Document.DocumentVisibility>(visibility, true, out var docVisibility))
        {
            docVisibility = Models.Document.DocumentVisibility.Chung;
        }

        var document = new Models.Document
        {
            MeetingId = meetingId,
            FileName = fileName,
            FilePath = $"/uploads/documents/{fileName}",
            Visibility = docVisibility,
            UploadedBy = userId,
            UploadDate = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return MapToDto(document);
    }

    private static bool ValidateMagicBytes(Stream stream, string extension)
    {
        if (stream.Length < 4) return false;

        var header = new byte[8];
        var bytesRead = stream.Read(header, 0, header.Length);
        stream.Position = 0; // Đặt lại vị trí ban đầu của stream

        if (bytesRead < 4) return false;

        switch (extension.ToLowerInvariant())
        {
            case ".pdf":
                // PDF: %PDF (0x25, 0x50, 0x44, 0x46)
                return header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;

            case ".png":
                // PNG: 0x89, 0x50, 0x4E, 0x47
                return header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;

            case ".jpg":
            case ".jpeg":
                // JPEG: 0xFF, 0xD8, 0xFF
                return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

            case ".docx":
            case ".xlsx":
            case ".pptx":
                // OpenXML ZIP archives: PK (0x50, 0x4B, 0x03, 0x04)
                return header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04;

            case ".doc":
            case ".xls":
            case ".ppt":
                // Compound File Binary Format: 0xD0, 0xCF, 0x11, 0xE0
                return header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0;

            case ".txt":
                // Plaintext: không chứa byte NULL trong phần đầu
                for (int i = 0; i < bytesRead; i++)
                {
                    if (header[i] == 0x00) return false;
                }
                return true;

            default:
                return false;
        }
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

        var physicalPath = GetDocumentPhysicalPath(document.FilePath);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Document {DocumentId} deleted by user {UserId}", documentId, userId);
        return true;
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsByMeetingAsync(int meetingId)
    {
        var documents = await _context.Documents
            .Include(d => d.Meeting)
            .Include(d => d.Uploader)
            .Where(d => d.MeetingId == meetingId)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        return documents.Select(MapToDto);
    }

    public async Task<List<DocumentDto>> GetAllDocumentsAsync()
    {
        var documents = await _context.Documents
            .Include(d => d.Meeting)
            .Include(d => d.Uploader)
            .OrderByDescending(d => d.UploadDate)
            .AsNoTracking()
            .ToListAsync();

        return documents.Select(MapToDto).ToList();
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(int documentId)
    {
        var document = await _context.Documents
            .Include(d => d.Meeting)
            .Include(d => d.Uploader)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
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
            UploadDate = document.UploadDate,
            MeetingId = document.MeetingId,
            MeetingTitle = document.Meeting?.Title,
            UploaderName = document.Uploader?.FullName ?? document.Uploader?.Username
        };
    }
}
