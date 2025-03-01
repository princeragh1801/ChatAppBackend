using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Entites
{
    public class Chat
    {
        [Key]
        public Guid Id { get; set; }
        public bool IsGroupChat { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? AdminId { get; set; }
        [ForeignKey(nameof(AdminId))]
        public User? Admin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<UserChat> Participants { get; set; }  
    }
}
