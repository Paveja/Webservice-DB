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
        private static List<User> _users = new List<User>();
        private static int _nextId = 1;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public UserController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        // GET: api/user
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

        // POST: api/user
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
                _users.Add(user);

                return Ok(new
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
    }
}
