using ChatApp.Dtos;

namespace ChatApp.Interfaces
{
    public interface IChatService
    {
        Task<bool> AddAsync(UpsertChatDto dto, Guid userId);
        Task<List<ChatDto>> GetAllAsync(Guid userId);
        Task<List<MessageInfo>> GetChats(Guid chatId);
        Task<bool> AddUserChatAsync(Guid userId, Guid recieverId);
        Task<ChatDto?> GetGroupInfo(Guid chatId);
        Task<bool> RemoveUser(Guid chatId, Guid userId);
        Task<bool> AddUser(Guid chatId, Guid userId);
        Task<bool> RemoveChat(Guid chatId);
        Task<bool> UpdateGroupInfo(Guid chatId, UpdateGroupInfo groupInfo);
        Task<List<Guid>> GetChat(Guid userId);
    }
}
