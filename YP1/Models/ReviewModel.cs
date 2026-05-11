using System;

namespace YP1.Models
{
    public class ReviewModel
    {
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public bool IsFrozen { get; set; }
        public string FreezeReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
