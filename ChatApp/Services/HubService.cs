using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using ChatApp.Hubs;
using ChatApp.Interfaces;

namespace ChatApp.Services
{
    public class HubService : IHubService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMemoryCache _memoryCache;
        public HubService(IHubContext<ChatHub> hubContext, IMemoryCache memoryCache)
        {
            _hubContext = hubContext;
            _memoryCache = memoryCache;
        }
        public async Task AddToGroup(Guid userId, Guid groupId)
        {
            if (_memoryCache.TryGetValue(userId.ToString(), out string? connectionId))
            {
                if (connectionId != null)
                {
                    await _hubContext.Groups.AddToGroupAsync(connectionId, groupId.ToString());
                }
            }
        }

        public async Task RemoveFromGroup(Guid userId, Guid groupId)
        {
            if (_memoryCache.TryGetValue(userId.ToString(), out string? connectionId))
            {
                if (connectionId != null)
                {
                    await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupId.ToString());
                }
            }
        }

        public async Task NewChat(Guid groupId, object message)
        {
            await _hubContext.Clients.Group(groupId.ToString()).SendAsync("NewChat", message);
        }

        public async Task SendMessage(Guid userId, object message)
        {

            if (_memoryCache.TryGetValue(userId.ToString(), out string? connectionId))
            {
                if (connectionId != null)
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }

        }
        public async Task SendMessage(Guid userId, Guid chatId, object message)
        {

            if (_memoryCache.TryGetValue(userId.ToString(), out string? connectionId))
            {
                if (connectionId != null)
                {
                    await _hubContext.Clients.GroupExcept(chatId.ToString(), connectionId).SendAsync("ReceiveMessage", message);
                }
            }

        }
    }
}
