namespace RoomMate_Finder_Frontend.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public Guid ReviewerId { get; set; }
        public required string ReviewerFullName { get; set; }
        public int Rating { get; set; }
        public required string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

