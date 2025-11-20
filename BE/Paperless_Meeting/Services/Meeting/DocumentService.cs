using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Interfaces.Meeting;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Services.Meeting
{
    public class DocumentService : IDocumentService
    {
        private readonly PaperlessMeetingDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DocumentService(PaperlessMeetingDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<DocumentDto> UploadDocumentAsync(int meetingId, DocumentUploadDto uploadDto, int userId)
        {
            // Kiểm tra cuộc họp tồn tại
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null)
                throw new ArgumentException("Cuộc họp không tồn tại.");

            // Tạo thư mục lưu trữ nếu chưa tồn tại
            // Dùng ContentRootPath/Uploads thay vì WebRootPath (WebRootPath có thể null)
            var uploadDir = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", "documents", meetingId.ToString());
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            // Sinh tên file duy nhất
            var fileName = $"{Guid.NewGuid()}_{uploadDto.FileName}";
            var filePath = Path.Combine(uploadDir, fileName);

            // Lưu file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await uploadDto.File.CopyToAsync(fileStream);
            }

            // Tạo record Document trong database
            var document = new Document
            {
                MeetingId = meetingId,
                FileName = uploadDto.FileName,
                FilePath = $"/uploads/documents/{meetingId}/{fileName}",
                Visibility = (Document.DocumentVisibility)Enum.Parse(typeof(Document.DocumentVisibility), uploadDto.Visibility),
                UploadedBy = userId,
                UploadDate = DateTime.UtcNow
            };

            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();

            return _mapper.Map<DocumentDto>(document);
        }

        public async Task<bool> DeleteDocumentAsync(int documentId, int userId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            // Chỉ người upload hoặc admin mới có thể xóa
            if (document.UploadedBy != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa tài liệu này.");

            // Xóa file từ disk
            // Từ FilePath như "/uploads/documents/1/uuid_file.pdf" → "Uploads/documents/1/uuid_file.pdf"
            var relativePath = document.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, relativePath);
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Xóa từ database
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByMeetingAsync(int meetingId)
        {
            var documents = await _context.Documents
                .Where(d => d.MeetingId == meetingId)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int documentId)
        {
            var document = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document == null) return null;

            return _mapper.Map<DocumentDto>(document);
        }
    }
}
