using ChatApp.Dtos;

namespace ChatApp.Interfaces
{
    public interface IMessageService
    {
        Task<List<MessageInfo>> GetAsync(Guid chatId);
        Task<Guid?> AddAsync(Guid senderId, Guid chatId, UpsertMessageDto dto);
        Task<bool> AddMultipleAsync(Guid senderId, Guid chatId, List<UpsertMessageDto> dtos);
        Task<bool> DeleteAsync(Guid messageId, Guid userId);
    }
}
