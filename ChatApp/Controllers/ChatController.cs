using ChatApp.Cache;
using ChatApp.Dtos;
using ChatApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ChatApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IMemoryCache _memoryCache;
        public ChatController(IChatService chatService, IMemoryCache memoryCache)
        {
            _chatService = chatService;
            _memoryCache = memoryCache;
        }
        [Authorize]
        [HttpGet("User")]
        public async Task<ActionResult<ApiResponse<List<UserInfoDto>>>> GetALl()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ApiResponse<UserInfoDto>("User not found"));
            }
            var cacheKey = $"{CacheKeys.UserChats}:{userId}";
            if(_memoryCache.TryGetValue(cacheKey, out List<UserInfoDto> info))
            {
                return Ok(new ApiResponse<List<UserInfoDto>>(info));
            }
            else
            {
                try
                {

                    var response = await _chatService.GetAllAsync(Guid.Parse(userId));
                    _memoryCache.Set(cacheKey, response, DateTime.Now.AddMinutes(1));
                    return Ok(new ApiResponse<List<ChatDto>>(response));
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<bool>>> Add(UpsertChatDto dto)
        {
            try
            {
                var userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var response = await _chatService.AddAsync(dto, userId);
                if (!response)
                {
                    return NotFound(new ApiResponse<UserInfoDto>("Some error occured"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet("{chatId}")]
        public async Task<ActionResult<ApiResponse<List<MessageInfo>>>> GetChat(Guid chatId)
        {
            var cacheKey = $"Chat:{chatId}";
            var chats = new List<MessageInfo>();
            if (_memoryCache.TryGetValue(cacheKey, out chats))
            {
                return Ok(new ApiResponse<List<MessageInfo>>(chats));
            }
            try
            {
                chats = await _chatService.GetChats(chatId);
                _memoryCache.Set(cacheKey, chats, DateTime.Now.AddMinutes(1));
                return Ok(new ApiResponse<List<MessageInfo>>(chats));
            }
            catch (Exception)
            {
                throw;
            }
            
        }
        [HttpPost("AddUserChat/{receiverId}")]
        public async Task<ActionResult<ApiResponse<bool>>> AddUserChat(Guid receiverId)
        {
            try
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse<bool>("User not found"));
                }
                var response = await _chatService.AddUserChatAsync(Guid.Parse(userId), receiverId);
                if (!response)
                {
                    return Ok(new ApiResponse<bool>("Chat exists"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GroupInfo/{chatId}")]
        public async Task<ActionResult<ApiResponse<ChatDto>>> GetGroupInfo(Guid chatId)
        {
            try
            {
                var response = await _chatService.GetGroupInfo(chatId);
                if(response == null)
                {
                    return Ok(new ApiResponse<ChatDto>("No info found"));
                }
                return Ok(new ApiResponse<ChatDto>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete("Group/{chatId}/{userId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveUser(Guid chatId, Guid userId)
        {
            try
            {
                var response = await _chatService.RemoveUser(chatId, userId);
                if (!response)
                {
                    return Ok(new ApiResponse<bool>("No info found"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("Group/{chatId}/{userId}")]
        public async Task<ActionResult<ApiResponse<bool>>> AddUser(Guid chatId, Guid userId)
        {
            try
            {
                var response = await _chatService.AddUser(chatId, userId);
                if (!response)
                {
                    return Ok(new ApiResponse<bool>("User already in the group"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete("{chatId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteChat(Guid chatId)
        {
            try
            {
                var response = await _chatService.RemoveChat(chatId);
                if (!response)
                {
                    return Ok(new ApiResponse<bool>("No info found"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("Group/{chatId}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateGroupName(Guid chatId, UpdateGroupInfo groupInfo)
        {
            if(string.IsNullOrEmpty(groupInfo.Name))
            {
                return BadRequest("Invalid input");
            }
            try
            {
                var response = await _chatService.UpdateGroupInfo(chatId, groupInfo);
                if (!response)
                {
                    return Ok(new ApiResponse<bool>("No info found"));
                }
                return Ok(new ApiResponse<bool>(response));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
