using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Paperless_Meeting.Data;
using Paperless_Meeting.DTOs.Department;
using Paperless_Meeting.Models;
using Paperless_Meeting.Services.Department;
using Xunit;

namespace Paperless_Meeting.Tests.UnitTests
{
    public class DepartmentServiceTests
    {
        private PaperlessMeetingDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaperlessMeetingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new PaperlessMeetingDbContext(options);
        }

        [Fact]
        public async Task GetAllDepartmentsAsync_ReturnsDepartmentsWithMemberCount()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Departments.AddRange(
                new Department { DepartmentId = 1, DepartmentName = "Phòng Kỹ thuật" },
                new Department { DepartmentId = 2, DepartmentName = "Phòng Trống" }
            );

            context.Users.Add(new User
            {
                UserId = 1,
                Username = "techlead",
                FullName = "Tech Lead",
                Email = "tech@hust.edu.vn",
                PasswordHash = "h",
                DepartmentId = 1,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();

            var service = new DepartmentService(context);

            // Act
            var depts = await service.GetAllDepartmentsAsync();

            // Assert
            depts.Should().HaveCount(2);
            var dept1 = depts.Find(d => d.DepartmentId == 1);
            dept1.Should().NotBeNull();
            dept1!.MemberCount.Should().Be(1);

            var dept2 = depts.Find(d => d.DepartmentId == 2);
            dept2.Should().NotBeNull();
            dept2!.MemberCount.Should().Be(0);
        }

        [Fact]
        public async Task CreateDepartmentAsync_ValidData_AddsDepartmentSuccessfully()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            var service = new DepartmentService(context);

            var createDto = new DepartmentCreateDto
            {
                DepartmentName = "Trung tâm Đổi mới Sáng tạo"
            };

            // Act
            var created = await service.CreateDepartmentAsync(createDto);

            // Assert
            created.Should().NotBeNull();
            created.DepartmentName.Should().Be("Trung tâm Đổi mới Sáng tạo");
            created.MemberCount.Should().Be(0);

            var saved = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Trung tâm Đổi mới Sáng tạo");
            saved.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteDepartmentAsync_DepartmentWithMembers_ReturnsFalse()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Departments.Add(new Department { DepartmentId = 3, DepartmentName = "Phòng Kế toán" });
            context.Users.Add(new User
            {
                UserId = 2,
                Username = "accountant",
                FullName = "Accountant",
                Email = "acc@hust.edu.vn",
                PasswordHash = "h",
                DepartmentId = 3,
                Role = User.UserRole.User
            });
            await context.SaveChangesAsync();

            var service = new DepartmentService(context);

            // Act: Cố gắng xóa phòng ban đang có nhân sự
            var success = await service.DeleteDepartmentAsync(3);

            // Assert
            success.Should().BeFalse();
            var dept = await context.Departments.FindAsync(3);
            dept.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteDepartmentAsync_EmptyDepartment_ReturnsTrueAndDeletes()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();
            context.Departments.Add(new Department { DepartmentId = 4, DepartmentName = "Phòng Ban Cũ" });
            await context.SaveChangesAsync();

            var service = new DepartmentService(context);

            // Act: Xóa phòng ban rỗng
            var success = await service.DeleteDepartmentAsync(4);

            // Assert
            success.Should().BeTrue();
            var dept = await context.Departments.FindAsync(4);
            dept.Should().BeNull();
        }
    }
}
