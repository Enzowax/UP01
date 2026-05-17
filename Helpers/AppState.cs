using ShutIKrol.Models;

namespace ShutIKrol.Helpers
{
    public static class AppState
    {
        public static User? CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.RoleId == 4;
        public static bool IsAuthor => CurrentUser?.RoleId == 5;
        public static bool IsReader => CurrentUser?.RoleId == 6;
        public static bool IsFrozen => CurrentUser?.IsFrozen == true;

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
