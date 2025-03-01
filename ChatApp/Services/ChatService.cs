using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;
using ChatApp.Database;
using ChatApp.Dtos;
using ChatApp.Entites;
using ChatApp.Hubs;
using ChatApp.Interfaces;

namespace ChatApp.Services
{
    public class ChatService : IChatService
    {
        private readonly BackendContext _context;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IHubService _hubService;
        public ChatService(BackendContext context, IMessageService messageService, IUserService userService, IHubService hubService)
        {
            _context = context;
            _messageService = messageService;
            _userService = userService;
            _hubService = hubService;
        }
        public async Task<bool> AddAsync(UpsertChatDto dto, Guid userId)
        {
            try
            {
                var chat = new Chat { CreatedAt = DateTime.Now, Name = dto.Name, IsGroupChat = true, AdminId = userId };
                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();
                var participants = dto.Participants.Select(x => new UserChat
                {
                    UserId = x,
                    ChatId = chat.Id
                }).ToList();
                participants.Add(new UserChat { UserId = userId, ChatId = chat.Id });
                await _context.UsersChats.AddRangeAsync(participants);
                await _context.SaveChangesAsync();  
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<ChatDto>> GetAllAsync(Guid userId)
        {
            try
            {
                var result = new List<ChatDto>();   
                var userChats = await _context.Chats.Include(x => x.Admin).Include(x => x.Messages).Include(x => x.Participants).ThenInclude(x => x.User).Where(x => x.Participants.Any(x => x.UserId == userId)).ToListAsync();
                foreach (var chat in userChats)
                {
                    var chatDto = new ChatDto
                    {
                        Id = chat.Id,
                        Name = chat.Name,
                        Admin = chat.Admin != null ? chat.Admin.Name : "",
                        CreatedAt = chat.CreatedAt,
                        LastMessage = (chat.Messages != null && chat.Messages.Count > 0) ? chat.Messages.OrderByDescending(x => x.SentAt).First().Content : string.Empty,
                        LastMessageTime = (chat.Messages != null && chat.Messages.Count > 0) ? chat.Messages.OrderByDescending(x => x.SentAt).First().SentAt : chat.CreatedAt,
                        IsGroupChat = chat.IsGroupChat,
                    };
                    var participants = chat.Participants.Where(x => x.UserId != userId).ToList();
                    if(participants.Count > 0 )
                    {
                        if (chat.IsGroupChat)
                        {
                            chatDto.Participants = participants.Select(x => UserInfoDto.Convert(x.User)).ToList();
                        }
                        else
                        {
                            chatDto.User = participants.Select(x => UserInfoDto.Convert(x.User)).FirstOrDefault();
                            chat.Name = chatDto.User != null ? chatDto.User.Name : "";
                        }
                    }
                    result.Add(chatDto);
                }
                
                return result.OrderByDescending(x => x.LastMessageTime).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<MessageInfo>> GetChats(Guid chatId)
        {
            try
            {
                return await _messageService.GetAsync(chatId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddUserChatAsync(Guid userId, Guid recieverId)
        {
            try
            {
                var exist = await _context.Chats.Include(x => x.Participants).Where(x => x.IsGroupChat == false && (x.AdminId == userId || x.AdminId == recieverId)).ToListAsync();
                foreach(var item in exist)
                {
                    if(item.Participants.Any())
                    {
                        if(item.AdminId == userId && item.Participants.Any(x => x.UserId == recieverId))
                        {
                            return false;
                        } else if(item.AdminId == recieverId && item.Participants.Any(x => x.UserId == userId))
                        {
                            return false;
                        }
                    }
                }
                var chat = new Chat { CreatedAt = DateTime.Now, Name = "", IsGroupChat = false, AdminId = userId};
                await _context.Chats.AddAsync(chat);
                await _context.SaveChangesAsync();

                var participants = new List<UserChat>
                {
                    new UserChat { UserId = userId, ChatId = chat.Id },
                    new UserChat { UserId = recieverId, ChatId = chat.Id }
                };
                await _context.UsersChats.AddRangeAsync(participants);
                await _context.SaveChangesAsync();
                /*if(_memoryCache.TryGetValue(userId.ToString(), out string connectionId))
                {
                    if(connectionId != null)
                    {
                        await _hubContext.Groups.AddToGroupAsync(connectionId, chat.Id.ToString());
                    }
                }
                if (_memoryCache.TryGetValue(recieverId.ToString(), out string receiverConnectionId))
                {
                    if (receiverConnectionId != null)
                    {
                        await _hubContext.Groups.AddToGroupAsync(receiverConnectionId, chat.Id.ToString());
                    }
                }*/
                
                await _hubService.AddToGroup(userId, chat.Id);
                await _hubService.AddToGroup(recieverId, chat.Id);
                
                var userInfo = await _userService.GetAsync(userId);
                var info = new ChatDto
                {
                    Id = chat.Id,
                    Name = chat.Name,
                    Admin = userId.ToString(),
                    CreatedAt = chat.CreatedAt,
                    IsGroupChat = chat.IsGroupChat,
                    LastMessage = "",
                    User = userInfo
                };
                await _hubService.NewChat(chat.Id, info);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ChatDto?> GetGroupInfo(Guid chatId)
        {
            try
            {
                var chat = await _context.Chats.Include(x => x.Messages).Include(x => x.Participants).ThenInclude(x => x.User).FirstOrDefaultAsync(x => x.Id == chatId);
                if(chat == null)
                {
                    return null;
                }

                var groupInfo = new ChatDto
                {
                    Id = chat.Id,
                    Name = chat.Name,
                    IsGroupChat = chat.IsGroupChat,
                    Participants = chat.Participants.Select(x => UserInfoDto.Convert(x.User)).ToList(),
                    CreatedAt = chat.CreatedAt,
                    Admin = chat.AdminId != null ? chat.AdminId.ToString() : "",
                    LastMessageTime = DateTime.UtcNow,
                    LastMessage = "Hello"
                };
                return groupInfo;
            }
            catch (Exception)
            {

                throw;
            }
        }
    
        public async Task<bool> RemoveUser(Guid chatId, Guid userId)
        {
            try
            {
                var userChat = await _context.UsersChats.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == userId);
                if(userChat == null)
                {
                    return false;
                }
                _context.UsersChats.Remove(userChat);
                await _context.SaveChangesAsync();
                await _hubService.RemoveFromGroup(userId, chatId);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> AddUser(Guid chatId, Guid userId)
        {
            try
            {
                var userChat = await _context.UsersChats.FirstOrDefaultAsync(x => x.ChatId == chatId && x.UserId == userId);
                if (userChat != null)
                {
                    return false;
                }
                var newUser = new UserChat
                {
                    ChatId = chatId,
                    UserId = userId,
                };
                await _context.UsersChats.AddAsync(newUser);
                await _context.SaveChangesAsync();
                await _hubService.AddToGroup(userId, chatId);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveChat(Guid chatId)
        {
            try
            {
                var chatUsers = await _context.UsersChats.Where(x => x.ChatId == chatId).ToListAsync();
                if (chatUsers.Any())
                {
                    _context.UsersChats.RemoveRange(chatUsers);
                    await _context.SaveChangesAsync();
                    var messages = await _context.Messages.Where(x => x.ChatId == chatId).ToListAsync();
                    if (messages.Any())
                    {
                        _context.Messages.RemoveRange(messages);
                        await _context.SaveChangesAsync();
                    }
                    var chat = await _context.Chats.FirstOrDefaultAsync(x => x.Id == chatId);
                    if(chat != null)
                    {
                        _context.Chats.Remove(chat);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateGroupInfo(Guid chatId, UpdateGroupInfo groupInfo)
        {
            try
            {
                var chat = await _context.Chats.FirstOrDefaultAsync(x => x.IsGroupChat && x.Id == chatId);
                if(chat == null)
                {
                    return false;
                }
                chat.Name = groupInfo.Name;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Guid>> GetChat(Guid userId)
        {
            try
            {
                var chats = await _context.Chats.Include(x => x.Participants).Where(x => x.Participants.Any(y => y.UserId == userId)).Select(x => x.Id).ToListAsync();
                return chats;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}