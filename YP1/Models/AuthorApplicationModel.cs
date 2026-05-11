using System;

namespace YP1.Models
{
    public class AuthorApplicationModel
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Login { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
