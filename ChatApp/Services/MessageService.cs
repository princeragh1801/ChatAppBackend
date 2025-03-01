using Microsoft.EntityFrameworkCore;
using ChatApp.Database;
using ChatApp.Dtos;
using ChatApp.Entites;
using ChatApp.Interfaces;

namespace ChatApp.Services
{
    public class MessageService : IMessageService
    {
        private readonly BackendContext _context;
        private readonly IHubService _hubService;
        public MessageService(BackendContext context, IHubService hubService)
        {
            _context = context;
            _hubService = hubService;
        }
        public async Task<Guid?> AddAsync(Guid senderId, Guid chatId, UpsertMessageDto dto)
        {
            try
            {
                var chat = await _context.Chats.Include(x => x.Admin).FirstOrDefaultAsync(x => x.Id == chatId);
                if (chat == null)
                {
                    return null;
                }
                var message = new Message
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    SentAt = DateTime.Now,
                    Content = dto.Content,
                };
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
                // Send the message to the specific user (recipientId)
                var sender = await _context.Users.FirstOrDefaultAsync(x => x.Id == senderId);
                var messageData = new
                {
                    Id = message.Id,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    Sender = sender,
                    SenderId = senderId,
                    ChatId = chatId
                };
                await _hubService.SendMessage(senderId, chatId, messageData);

                return message.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddMultipleAsync(Guid senderId, Guid chatId, List<UpsertMessageDto> dtos)
        {
            try
            {
                var messages = dtos.Select(dto => new Message
                {
                    ChatId = chatId,
                    SenderId = senderId,
                    SentAt = DateTime.UtcNow,
                    Content = dto.Content,
                }).ToList();
                _context.Messages.AddRange(messages);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<MessageInfo>> GetAsync(Guid chatId)
        {
            try
            {
                var messages = await _context.Messages.Include(x => x.Sender).Where(x => x.ChatId == chatId).OrderByDescending(x => x.SentAt).Select(x => MessageInfo.FromMessage(x)).ToListAsync();

                return messages;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid messageId, Guid userId)
        {
            try
            {
                var message = await _context.Messages.FirstOrDefaultAsync(x => x.Id == messageId);
                if (message == null)
                {
                    return false;
                }
                if (message.SenderId != userId)
                {
                    return false;
                }
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
