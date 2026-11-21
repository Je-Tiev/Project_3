using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Paperless_Meeting.DTOs.Department;
using Paperless_Meeting.Repositories.Department;

namespace Paperless_Meeting.Controllers.Meeting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<DepartmentDto>>> GetAllDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartmentById(int id)
        {
            var dept = await _departmentService.GetDepartmentByIdAsync(id);
            if (dept == null)
            {
                return NotFound("Không tìm thấy phòng ban.");
            }
            return Ok(dept);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] DepartmentCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _departmentService.CreateDepartmentAsync(createDto);
            return CreatedAtAction(nameof(GetDepartmentById), new { id = result.DepartmentId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentCreateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _departmentService.UpdateDepartmentAsync(id, updateDto);
            if (!success)
            {
                return NotFound("Không tìm thấy phòng ban cần cập nhật.");
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var success = await _departmentService.DeleteDepartmentAsync(id);
            if (!success)
            {
                return BadRequest("Không thể xóa phòng ban đang có nhân sự hoặc không tồn tại.");
            }
            return NoContent();
        }
    }
}
