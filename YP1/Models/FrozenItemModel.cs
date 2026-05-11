namespace YP1.Models
{
    public class FrozenItemModel
    {
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public string FreezeReason { get; set; }
    }
}
