using Paperless_Meeting.DTOs.Document;

namespace Paperless_Meeting.Repositories.Document;

public interface IDocumentService
{
    Task<List<DocumentDto>> UploadMultipleDocumentsAsync(int meetingId, DocumentMultipleUploadDto uploadDto, int userId);    Task<DocumentDto> UploadDocumentAsync(int meetingId, DocumentUploadDto uploadDto, int userId);
    Task<DocumentDto> UploadDocumentStreamAsync(int meetingId, Stream fileStream, string contentType, string visibility, int userId);
    Task<bool> DeleteDocumentAsync(int documentId, int userId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByMeetingAsync(int meetingId);
    Task<DocumentDto?> GetDocumentByIdAsync(int documentId);
    
}
