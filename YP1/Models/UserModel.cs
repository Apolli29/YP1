namespace YP1.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public bool IsFrozen { get; set; }
        public string FreezeReason { get; set; }

        public bool IsAdministrator
        {
            get { return RoleName == "administrator"; }
        }

        public bool IsAuthor
        {
            get { return RoleName == "author" || RoleName == "administrator"; }
        }
    }
}
