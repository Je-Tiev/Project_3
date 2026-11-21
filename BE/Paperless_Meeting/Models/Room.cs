using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoomName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        public int Capacity { get; set; } = 20;

        [MaxLength(500)]
        public string? Equipment { get; set; }

        public RoomStatus Status { get; set; } = RoomStatus.Available;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    }

    public enum RoomStatus
    {
        Available = 0,
        InUse = 1,
        Maintenance = 2
    }
}
