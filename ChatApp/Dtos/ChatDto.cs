using ChatApp.Entites;

namespace ChatApp.Dtos
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Admin { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserInfoDto? User { get; set; }
        public List<UserInfoDto>? Participants { get; set; }
        public bool IsGroupChat { get; set; }   
        public static ChatDto Convert(Chat chat)
        {
            return new ChatDto
            {
                Id = chat.Id,
                LastMessage = chat.Messages.OrderByDescending(x => x.SentAt).FirstOrDefault()?.Content ?? string.Empty,
                CreatedAt = chat.CreatedAt,
                LastMessageTime = chat.Messages.OrderByDescending(x => x.SentAt).FirstOrDefault()?.SentAt ?? DateTime.MinValue,
                IsGroupChat = chat.IsGroupChat,
            };
        }

    }
    public class UpsertChatDto
    {
        public string Name { get; set;} = string.Empty;
        public List<Guid> Participants { get; set; }
    }
    
}
