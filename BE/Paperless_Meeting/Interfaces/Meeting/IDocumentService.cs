using Paperless_Meeting.DTOs.Meeting;

namespace Paperless_Meeting.Interfaces.Meeting;

public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(int meetingId, DocumentUploadDto uploadDto, int userId);
    Task<bool> DeleteDocumentAsync(int documentId, int userId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByMeetingAsync(int meetingId);
    Task<DocumentDto?> GetDocumentByIdAsync(int documentId);
}
