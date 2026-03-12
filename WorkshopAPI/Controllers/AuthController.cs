using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WorkshopAPI.Data;
using WorkshopAPI.Models;
using WorkshopAPI.Services;

namespace WorkshopAPI.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        //constructor for injection
        public AuthController(JwtService jwtService, ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public ActionResult<User> Register(UserRegisterDto dto)
        {
            // ตรวจสอบว่า username ซ้ำหรือไม่
            if (UserStore.GetUserByUsername(dto.Username) != null)
            {
                return BadRequest(new { error = "Username already exists" });
            }

            // Hash password
            var passwordHash = PasswordService.HashPassword(dto.Password);

            // สร้าง user ใหม่
            var newUser = new User
            {
                Id = UserStore.Users.Count + 1,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            UserStore.CreateUser(newUser);

            // Return user response (ไม่ส่ง password_hash - จะจัดการใน JSON serialization)
            // สร้าง User object ใหม่โดยไม่รวม password_hash
            var userResponse = new User
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                CreatedAt = newUser.CreatedAt,
                PasswordHash = string.Empty // ไม่ส่ง password_hash ใน response
            };

            return CreatedAtAction(
                nameof(GetProfile),
                new { },
                userResponse
            );
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public ActionResult<TokenResponse> Login(UserLoginDto dto)
        {
            // ค้นหา user จาก username
            var user = UserStore.GetUserByUsername(dto.Username);

            if (user == null)
            {
                return Unauthorized(new { error = "Invalid username or password" });
            }

            // ตรวจสอบ password
            if (!PasswordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { error = "Invalid username or password" });
            }

            // สร้าง JWT access token และ refresh token
            var accessTokenResult = _jwtService.GenerateAccessTokenWithJti(user.Id, user.Username);
            var refreshToken = _jwtService.GenerateRefreshToken(user.Id, user.Username, accessTokenResult.Jti);

            return Ok(new TokenResponse
            {
                AccessToken = accessTokenResult.Token,
                RefreshToken = refreshToken,
                TokenType = "bearer",
                ExpiresIn = _jwtService.GetAccessTokenExpirationSeconds()
            });
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        public ActionResult<TokenResponse> Refresh(RefreshTokenDto dto)
        {
            // ตรวจสอบว่า refresh token ถูก revoke หรือไม่
            if (UserStore.IsRefreshTokenRevoked(dto.RefreshToken))
            {
                return Unauthorized(new { error = "Refresh token has been revoked" });
            }

            // Validate refresh token
            var principal = _jwtService.ValidateToken(dto.RefreshToken);

            if (principal == null)
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }

            // ดึง user id จาก token
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usernameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;

            if (userIdClaim == null || usernameClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid token claims" });
            }

            // ตรวจสอบว่า user ยังมีอยู่
            var user = UserStore.GetUserById(userId);
            if (user == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            // Revoke access token เดิมที่ผูกกับ refresh token
            var oldAccessTokenJti = principal.FindFirst("ati")?.Value;
            if (!string.IsNullOrEmpty(oldAccessTokenJti))
            {
                UserStore.RevokeAccessTokenJti(oldAccessTokenJti);
            }

            // สร้าง access token และ refresh token ใหม่
            var newAccessTokenResult = _jwtService.GenerateAccessTokenWithJti(userId, usernameClaim);
            var newRefreshToken = _jwtService.GenerateRefreshToken(userId, usernameClaim, newAccessTokenResult.Jti);

            // Revoke refresh token เก่า
            UserStore.RevokeRefreshToken(dto.RefreshToken);

            return Ok(new TokenResponse
            {
                AccessToken = newAccessTokenResult.Token,
                RefreshToken = newRefreshToken,
                TokenType = "bearer",
                ExpiresIn = _jwtService.GetAccessTokenExpirationSeconds()
            });
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        [Authorize] // ต้องมี token ถึงจะ logout ได้
        public IActionResult Logout(RefreshTokenDto dto)
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                UserStore.RevokeAccessTokenJti(jti);
            }

            // ถ้ามี refresh token ให้ revoke
            if (dto != null && !string.IsNullOrEmpty(dto.RefreshToken))
            {
                UserStore.RevokeRefreshToken(dto.RefreshToken);
            }

            return Ok(new { message = "Logged out successfully" });
        }

        // GET /api/auth/profile
        [HttpGet("profile")]
        [Authorize] // ต้องมี token ถึงจะเข้าถึงได้
        public ActionResult<User> GetProfile()
        {
            // ดึง user id จาก token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid token" });
            }

            // ค้นหา user
            var user = UserStore.GetUserById(userId);

            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // สร้าง User object ใหม่โดยไม่รวม password_hash
            var userResponse = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                PasswordHash = string.Empty // ไม่ส่ง password_hash ใน response
            };

            return Ok(userResponse);
        }
    }
}
