using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSocketDemo.Entites
{
    public class UserChat
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ChatId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        [ForeignKey(nameof(ChatId))]
        public Chat Chat { get; set; }
    }
}
