using WorkshopAPI.Models;

namespace WorkshopAPI.Data
{
    public class UserStore
    {
        public static List<User> Users = new()
    {
        new User
        {
            Id = 1,
            Username = "john_doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        },
        new User
        {
            Id = 2,
            Username = "jane_smith",
            Email = "jane@example.com",
            CreatedAt = DateTime.UtcNow
        }
    };
    }
}
