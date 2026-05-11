using System.Collections.Generic;

namespace YP1.Models
{
    public class BookModel
    {
        public BookModel()
        {
            GenreIds = new List<int>();
        }

        public int BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string BookText { get; set; }
        public string CoverColor { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsFrozen { get; set; }
        public string FreezeReason { get; set; }
        public decimal AverageRating { get; set; }
        public string GenresText { get; set; }
        public string ListName { get; set; }
        public List<int> GenreIds { get; set; }
    }
}
