namespace Paperless_Meeting.DTOs.Poll
{
    public class VoteDetailDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } // Tên người vote
        public bool Choice { get; set; }     // True = Đồng ý, False = Không
        public DateTime VotedAt { get; set; }
    }
}