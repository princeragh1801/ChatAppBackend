using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ChatApp.Dtos;
using ChatApp.Interfaces;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IMemoryCache _memoryCache;

        public MessagesController(IMessageService messageService, IMemoryCache memoryCache)
        {
            _messageService = messageService;   
            _memoryCache = memoryCache;
        }

        [HttpPost("{chatId}")]
        public async Task<ApiResponse<Guid?>> AddMessage(Guid chatId, [FromForm] UpsertMessageDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _messageService.AddAsync(userId, chatId, dto);
                var cacheKey = $"Chat:{chatId}";
                _memoryCache.Remove(cacheKey);
                return new ApiResponse<Guid?>(result);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("{chatId}")]
        public async Task<ActionResult<ApiResponse<List<MessageInfo>>>> GetMessages(Guid chatId)
        {
            var cacheKey = $"Chat:{chatId}";
            var chats = new List<MessageInfo>();
            if (_memoryCache.TryGetValue(cacheKey, out chats))
            {
                return Ok(new ApiResponse<List<MessageInfo>>(chats));
            }
            try
            {
                chats = await _messageService.GetAsync(chatId);
                _memoryCache.Set(cacheKey, chats, DateTime.Now.AddMinutes(1));
                return Ok(new ApiResponse<List<MessageInfo>>(chats));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMessage(Guid messageId)
        {
            try
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if(userId == null)
                {
                    return BadRequest("User not found");
                }
                var deleted = await _messageService.DeleteAsync(messageId, Guid.Parse(userId));
                return Ok(new ApiResponse<bool>(deleted));
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
