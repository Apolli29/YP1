namespace YP1.Models
{
    public static class AppSession
    {
        public static UserModel CurrentUser { get; set; }
        public static bool DatabaseReady { get; set; }
        public static string DatabaseError { get; set; }
    }
}
