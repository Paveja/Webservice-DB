using Microsoft.AspNetCore.Mvc;
using dotnet06.Models;
using System.Net.Http;
using System.Text;

namespace dotnet06.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private static List<User> _users = InitializeMockData();
        private static int _nextId = 6;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public UserController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Initialize mock data for testing
        /// </summary>
        private static List<User> InitializeMockData()
        {
            return new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "john_doe",
                    Email = "john@example.com",
                    Password = "hashed_password_1",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    Username = "jane_smith",
                    Email = "jane@example.com",
                    Password = "hashed_password_2",
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    IsActive = true
                },
                new User
                {
                    Id = 3,
                    Username = "bob_wilson",
                    Email = "bob@example.com",
                    Password = "hashed_password_3",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    IsActive = true
                },
                new User
                {
                    Id = 4,
                    Username = "alice_johnson",
                    Email = "alice@example.com",
                    Password = "hashed_password_4",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    IsActive = false
                },
                new User
                {
                    Id = 5,
                    Username = "charlie_brown",
                    Email = "charlie@example.com",
                    Password = "hashed_password_5",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    IsActive = true
                }
            };
        }

        // GET: api/user
        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<object>> GetUsers()
        {
            try
            {
                // Get the THIRD_PARTY_URL environment variable
                var thirdPartyUrl = _configuration["THIRD_PARTY_URL"];
                if (string.IsNullOrEmpty(thirdPartyUrl))
                {
                    return StatusCode(500, new
                    {
                        StatusCode = 500,
                        Message = "THIRD_PARTY_URL environment variable is not set."
                    });
                }

                // Call third-party API
                var content = new StringContent("Hello World! testing third party", Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync(thirdPartyUrl, content);
                response.EnsureSuccessStatusCode();
                var thirdPartyResponse = await response.Content.ReadAsStringAsync();

                if (!_users.Any())
                {
                    return Ok(new
                    {
                        StatusCode = 200,
                        Message = "No users found",
                        Data = new List<User>(),
                        ThirdPartyResponse = thirdPartyResponse
                    });
                }

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Users retrieved successfully",
                    Data = _users,
                    ThirdPartyResponse = thirdPartyResponse
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error calling third-party API",
                    Error = ex.Message
                });
            }
        }

        // GET: api/user/{id}
        /// <summary>
        /// Get a user by ID
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<object> GetUserById(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = $"User with ID {id} not found",
                    Data = new { }
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "User retrieved successfully",
                Data = user
            });
        }

        // POST: api/user
        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateUser(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Username and Email are required",
                        Data = new { }
                    });
                }

                // Get the THIRD_PARTY_URL environment variable
                var thirdPartyUrl = _configuration["THIRD_PARTY_URL"];
                if (string.IsNullOrEmpty(thirdPartyUrl))
                {
                    return StatusCode(500, new
                    {
                        StatusCode = 500,
                        Message = "THIRD_PARTY_URL environment variable is not set."
                    });
                }

                // Call third-party API
                var content = new StringContent("User creation notification", Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync(thirdPartyUrl, content);
                response.EnsureSuccessStatusCode();
                var thirdPartyResponse = await response.Content.ReadAsStringAsync();

                user.Id = _nextId++;
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                _users.Add(user);

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new
                {
                    StatusCode = 201,
                    Message = "User created successfully",
                    Data = user,
                    ThirdPartyResponse = thirdPartyResponse
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Error calling third-party API",
                    Error = ex.Message
                });
            }
        }

        // PUT: api/user/{id}
        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult<object> UpdateUser(int id, User updatedUser)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = $"User with ID {id} not found",
                    Data = new { }
                });
            }

            if (string.IsNullOrEmpty(updatedUser.Username) || string.IsNullOrEmpty(updatedUser.Email))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Username and Email are required",
                    Data = new { }
                });
            }

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.Password = updatedUser.Password ?? user.Password;
            user.IsActive = updatedUser.IsActive;

            return Ok(new
            {
                StatusCode = 200,
                Message = "User updated successfully",
                Data = user
            });
        }

        // DELETE: api/user/{id}
        /// <summary>
        /// Delete a user by ID
        /// </summary>
        [HttpDelete("{id}")]
        public ActionResult<object> DeleteUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = $"User with ID {id} not found",
                    Data = new { }
                });
            }

            _users.Remove(user);

            return Ok(new
            {
                StatusCode = 200,
                Message = "User deleted successfully",
                Data = user
            });
        }
    }
}
