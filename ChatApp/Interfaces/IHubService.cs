namespace ChatApp.Interfaces
{
    public interface IHubService
    {
        Task SendMessage(Guid userId, Guid chatId, object message);
        Task NewChat(Guid groupId, object message);
        Task AddToGroup(Guid userId, Guid groupId);
        Task RemoveFromGroup(Guid userId, Guid groupId);
        Task SendMessage(Guid userId, object message);
    }
}
