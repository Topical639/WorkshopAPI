using Microsoft.AspNetCore.Mvc;
using WorkshopAPI.Data;
using WorkshopAPI.Models;

namespace WorkshopAPI.Controllers
{

    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        // GET /api/users
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAll()
        {
            //IEnumerable collection : list , array
            return Ok(UserStore.Users);
        }

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public ActionResult<User> GetById(int id)
        {
            var user = UserStore.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(user);
        }
        [HttpPost]
        public ActionResult<User> Create(User input)
        {
            // Validation: µÃÇ¨ÊÍº Username
            if (string.IsNullOrWhiteSpace(input.Username))
            {
                return BadRequest(new { error = "Username is required" });
            }

            // Validation: µÃÇ¨ÊÍº Email
            if (string.IsNullOrWhiteSpace(input.Email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            // Validation: µÃÇ¨ÊÍº Email format (áºº§èÒÂ)
            if (!input.Email.Contains("@"))
            {
                return BadRequest(new { error = "Invalid email format" });
            }

            // µÃÇ¨ÊÍºÇèÒ username «éÓËÃ×ÍäÁè
            if (UserStore.Users.Any(u => u.Username == input.Username))
            {
                return BadRequest(new { error = "Username already exists" });
            }

            // ÊÃéÒ§ ID ãËÁè (ËÒ¤èÒ max + 1)
            var newId = UserStore.Users.Max(u => u.Id) + 1;

            // ÊÃéÒ§ User ãËÁè
            var user = new User
            {
                Id = newId,
                Username = input.Username,
                Email = input.Email,
                CreatedAt = DateTime.UtcNow
            };

            // à¾ÔèÁà¢éÒ List
            UserStore.Users.Add(user);

            // Êè§ Response 201 Created ¾ÃéÍÁ Location header
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public ActionResult<User> Update(int id, User input)
        {
            // Validation: µÃÇ¨ÊÍº Username
            if (string.IsNullOrWhiteSpace(input.Username))
            {
                return BadRequest(new { error = "Username is required" });
            }

            // Validation: µÃÇ¨ÊÍº Email
            if (string.IsNullOrWhiteSpace(input.Email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            // Validation: µÃÇ¨ÊÍº Email format
            if (!input.Email.Contains("@"))
            {
                return BadRequest(new { error = "Invalid email format" });
            }

            // ËÒ¼ÙéãªéµÒÁ id
            var user = UserStore.Users.FirstOrDefault(u => u.Id == id);

            // ¶éÒäÁè¾º Êè§ 404
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // ÍÑ¾à´·¢éÍÁÙÅ
            user.Username = input.Username;
            user.Email = input.Email;

            return Ok(user);
        }

        // PATCH /api/users/{id}
        [HttpPatch("{id}")]
        public ActionResult<User> Patch(int id, UserUpdateDto input)
        {
            // ËÒ¼ÙéãªéµÒÁ id
            var user = UserStore.Users.FirstOrDefault(u => u.Id == id);

            // ¶éÒäÁè¾º Êè§ 404
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // ÍÑ¾à´·à©¾ÒÐ field ·ÕèÊè§ÁÒ
            if (input.Username != null)
            {
                if (string.IsNullOrWhiteSpace(input.Username))
                {
                    return BadRequest(new { error = "Username cannot be empty" });
                }
                user.Username = input.Username;
            }

            if (input.Email != null)
            {
                if (string.IsNullOrWhiteSpace(input.Email))
                {
                    return BadRequest(new { error = "Email cannot be empty" });
                }
                if (!input.Email.Contains("@"))
                {
                    return BadRequest(new { error = "Invalid email format" });
                }
                user.Email = input.Email;
            }

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // ËÒ¼ÙéãªéµÒÁ id
            var user = UserStore.Users.FirstOrDefault(u => u.Id == id);

            // ¶éÒäÁè¾º Êè§ 404
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Åº¼Ùéãªé
            UserStore.Users.Remove(user);

            // Êè§ Status Code 204 No Content
            return NoContent();
        }
    }
}
