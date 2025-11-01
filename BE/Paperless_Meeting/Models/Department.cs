using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.Models;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string DepartmentName { get; set; }
    
    public virtual ICollection<User> Members { get; set; }
}