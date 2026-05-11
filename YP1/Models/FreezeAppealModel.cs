using System;

namespace YP1.Models
{
    public class FreezeAppealModel
    {
        public int AppealId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public string AppealText { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
