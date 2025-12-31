namespace Paperless_Meeting.DTOs.Auth;

public class UpdateUserDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
}
