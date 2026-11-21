using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Department
{
    public class DepartmentCreateDto
    {
        [Required(ErrorMessage = "Tên phòng ban không được để trống")]
        [StringLength(100, ErrorMessage = "Tên phòng ban không được vượt quá 100 ký tự")]
        public string DepartmentName { get; set; } = string.Empty;
    }
}
