namespace YP1.Models
{
    public class GenreModel
    {
        public int GenreId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
