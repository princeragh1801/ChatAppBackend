using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using ChatApp.Entites;
using ChatApp.Interfaces;
using ChatApp.Services;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IChatService _chatService;
        public ChatHub(ILogger<ChatHub> logger, IMemoryCache memoryCache, IChatService chatService)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _chatService = chatService;
        }
        public override async Task<Task> OnConnectedAsync()
        {
            var context = Context.GetHttpContext();
            var userId = GetUserIdFromToken(context);
            var cacheKey = userId;
            if (cacheKey != null && userId != null)
            {
                _memoryCache.Remove(cacheKey);
                _memoryCache.Set(cacheKey, Context.ConnectionId);
                var chats = await _chatService.GetChat(Guid.Parse(userId));
                foreach (var chat in chats)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, chat.ToString());
                }
            }
            return base.OnConnectedAsync();
        }

        public async Task JoinChat(Guid chatId)
        {

            var context = Context.GetHttpContext();
            var userId = GetUserIdFromToken(context);
            _logger.LogInformation($"User has joined chat with user id - {userId} and chat id - {chatId}");
            var cacheKey = userId;
            if (cacheKey != null)
            {
                _memoryCache.Remove(cacheKey);
                _memoryCache.Set(cacheKey, Context.ConnectionId);
                // Add the user to the group corresponding to the chatId
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            }

        }

        public async Task Typing(Guid chatId)
        {
            var context = Context.GetHttpContext();
            // Add the user to the group corresponding to the chatId
            //await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            var data = JwtHelper.DecodeJwtToken(context);
            var messageData = new
            {
                Content = $"{data.Name} is typing...",
                SenderId = data.UserId,
                SentAt = DateTime.UtcNow,
                ChatId = chatId
            };
            var connectionId = Context.ConnectionId;
            // Optionally, you could notify the group that a user has joined (optional)
            await Clients.GroupExcept(chatId.ToString(), connectionId).SendAsync("Typing", messageData);
            
        }

        public async Task StopTyping(Guid chatId)
        {
            var context = Context.GetHttpContext();
            var userId = GetUserIdFromToken(context);
            _logger.LogInformation($"User has stopped typing in chat with user id - {userId} and chat id - {chatId}");
            var connectionId = Context.ConnectionId;
            // Add the user to the group corresponding to the chatId
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

            // Optionally, you could notify the group that a user has joined (optional)
            await Clients.GroupExcept(chatId.ToString(), connectionId).SendAsync("StopTyping", chatId);
            Console.WriteLine($"User has joined chat with user id - {userId} and chat id - {chatId}");
            // You can also store the user's chat state if needed, like storing the userId in a group for later retrieval.
        }

        // This method will be called by the client when they send a message.
        public async Task SendMessage(string user, string message)
        {
            // This sends a message to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task RecieveMessage(string user, string message)
        {
            // This sends a message to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            
            return base.OnDisconnectedAsync(exception);
        }

        private string? GetUserIdFromToken(HttpContext context)
        {
            var token = context.Request.Query["access_token"].ToString();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            if(jwtToken == null)
            {
                return null;
            }
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            return userId;
        }
    }
}
