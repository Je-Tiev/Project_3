using System.ComponentModel.DataAnnotations;

namespace Paperless_Meeting.DTOs.Poll;

public class PollDetailDto
    {
        public int PollId { get; set; }
        public int MeetingId { get; set; }
        public string Question { get; set; }
        public string Status { get; set; } // "Open" hoặc "Closed"
        public DateTime CreatedAt { get; set; }
        
        // Kết quả thống kê
        public int TotalVotes { get; set; }
        public int YesVotes { get; set; }
        public int NoVotes { get; set; }

        public bool UserHasVoted { get; set; } 
        public bool? UserChoice { get; set; }
        public List<VoteDetailDto> Votes { get; set; } = new List<VoteDetailDto>();
    }