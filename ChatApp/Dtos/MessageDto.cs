using ChatApp.Entites;

namespace ChatApp.Dtos
{
    public class UpsertMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }
    public class MessageInfo : UpsertMessageDto
    {
        public Guid Id { get; set; }
        public UserInfoDto Sender { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public static MessageInfo FromMessage(Message message)
        {
            return new MessageInfo
            {
                Content = message.Content,
                Id = message.Id,
                Sender = UserInfoDto.Convert(message.Sender),
                SentAt = message.SentAt,
            };
        }
    }
}
