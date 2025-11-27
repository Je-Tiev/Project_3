using Paperless_Meeting.DTOs.Document;

namespace Paperless_Meeting.Repositories.Document;

public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(int meetingId, DocumentUploadDto uploadDto, int userId);
    Task<bool> DeleteDocumentAsync(int documentId, int userId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByMeetingAsync(int meetingId);
    Task<DocumentDto?> GetDocumentByIdAsync(int documentId);
}
