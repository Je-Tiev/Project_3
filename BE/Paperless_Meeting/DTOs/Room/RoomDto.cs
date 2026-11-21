using System;

namespace Paperless_Meeting.DTOs.Room
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public int Capacity { get; set; }
        public string? Equipment { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; }
        public string? CurrentUser { get; set; }
        public string? UpcomingSchedule { get; set; }
    }
}
