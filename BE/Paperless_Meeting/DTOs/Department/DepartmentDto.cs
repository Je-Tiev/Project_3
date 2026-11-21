using System.Collections.Generic;

namespace Paperless_Meeting.DTOs.Department
{
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
    }
}
