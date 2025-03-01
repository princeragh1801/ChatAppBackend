
using Microsoft.AspNetCore.Mvc;
using ChatApp.Dtos;
using ChatApp.Interfaces;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserInfoDto>>>> GetALl()
        {
            try
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if(userId == null)
                {
                    return BadRequest("User not found");
                }
                var response = await _userService.GetAllAsync(Guid.Parse(userId));
                return Ok(new ApiResponse<List<UserInfoDto>>(response));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserInfoDto>>> Get(Guid id)
        {
            try
            {
                var response = await _userService.GetAsync(id);
                if(response == null)
                {
                    return NotFound(new ApiResponse<UserInfoDto>("User not found"));
                }
                return Ok(new ApiResponse<UserInfoDto>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<bool>>> Add(UserDto dto)
        {
            try
            {
                var response = await _userService.AddAsync(dto);
                if (!response)
                {
                    return Ok(new ApiResponse<UserInfoDto>("User already exist try to login"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost("List")]
        public async Task<ActionResult<ApiResponse<bool>>> AddMultiple(List<UserDto> dto)
        {
            try
            {
                var response = await _userService.AddMultipleAsync(dto);
                if (!response)
                {
                    return NotFound(new ApiResponse<UserInfoDto>("User already exist try to login"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse<LoginInfo>>> Login(LoginDto dto)
        {
            try
            {
                var response = await _userService.LoginUser(dto);
                return Ok(new ApiResponse<LoginInfo>(response));
            }
            catch (Exception ex)
            {
                if (ex.Message == ResponseCodes.UserNotFound || ex.Message == ResponseCodes.InvalidPassword)
                {
                    return Ok(new ApiResponse<LoginInfo>(ex.Message));
                }
                throw;
            }
        }

        [HttpPost("Logout")]
        public ActionResult<ApiResponse<bool>> Logout()
        {
            try
            {
                return Ok(new ApiResponse<bool>(true));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
