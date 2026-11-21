using System;
using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Room
{
    public class RoomAvailabilityCheckDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int? ExcludeMeetingId { get; set; }
    }
}
