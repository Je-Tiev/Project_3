using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Room
{
    public class RoomCreateDto
    {
        [Required(ErrorMessage = "Tên phòng họp không được để trống")]
        [StringLength(100, ErrorMessage = "Tên phòng họp không được vượt quá 100 ký tự")]
        public string RoomName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Location { get; set; }

        [Range(1, 1000, ErrorMessage = "Sức chứa phòng phải từ 1 đến 1000 người")]
        public int Capacity { get; set; } = 20;

        [StringLength(500)]
        public string? Equipment { get; set; }

        public string? Status { get; set; } = "Available";
    }
}
