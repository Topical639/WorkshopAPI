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
                PasswordHash = "",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                Username = "jane_smith",
                Email = "jane@example.com",
                PasswordHash = "",
                CreatedAt = DateTime.UtcNow
            }
        };
        // Token blacklist สำหรับเก็บ refresh tokens ที่ถูก revoke
        public static HashSet<string> RevokedRefreshTokens = new();
        // Token blacklist สำหรับเก็บ access token jti ที่ถูก revoke
        public static HashSet<string> RevokedAccessTokenJtis = new();

        public static User? GetUserByUsername(string username)
        {
            return Users.FirstOrDefault(u => u.Username == username);
        }

        public static User? GetUserById(int id)
        {
            return Users.FirstOrDefault(u => u.Id == id);
        }

        public static User CreateUser(User user)
        {
            Users.Add(user);
            return user;
        }

        public static void RevokeRefreshToken(string refreshToken)
        {
            RevokedRefreshTokens.Add(refreshToken);
        }

        public static bool IsRefreshTokenRevoked(string refreshToken)
        {
            return RevokedRefreshTokens.Contains(refreshToken);
        }

        public static void RevokeAccessTokenJti(string jti)
        {
            RevokedAccessTokenJtis.Add(jti);
        }

        public static bool IsAccessTokenJtiRevoked(string jti)
        {
            return RevokedAccessTokenJtis.Contains(jti);
        }
    }

}
