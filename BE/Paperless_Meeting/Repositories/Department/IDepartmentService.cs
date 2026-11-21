using System.Collections.Generic;
using System.Threading.Tasks;
using Paperless_Meeting.DTOs.Department;

namespace Paperless_Meeting.Repositories.Department
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto createDto);
        Task<bool> UpdateDepartmentAsync(int id, DepartmentCreateDto updateDto);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}
