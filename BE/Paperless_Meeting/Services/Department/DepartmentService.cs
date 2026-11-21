using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Department;
using Paperless_Meeting.Repositories.Department;

namespace Paperless_Meeting.Services.Department
{
    public class DepartmentService : IDepartmentService
    {
        private readonly PaperlessMeetingDbContext _context;

        public DepartmentService(PaperlessMeetingDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            return await _context.Departments
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    MemberCount = d.Members.Count
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Members)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (dept == null) return null;

            return new DepartmentDto
            {
                DepartmentId = dept.DepartmentId,
                DepartmentName = dept.DepartmentName,
                MemberCount = dept.Members.Count
            };
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto createDto)
        {
            var dept = new Models.Department
            {
                DepartmentName = createDto.DepartmentName
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                DepartmentId = dept.DepartmentId,
                DepartmentName = dept.DepartmentName,
                MemberCount = 0
            };
        }

        public async Task<bool> UpdateDepartmentAsync(int id, DepartmentCreateDto updateDto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            dept.DepartmentName = updateDto.DepartmentName;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Members)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (dept == null) return false;

            // Không cho xóa nếu phòng ban đang có nhân sự
            if (dept.Members.Any()) return false;

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
