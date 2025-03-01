using ChatApp.Entites;

namespace ChatApp.Dtos
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public static UserInfoDto Convert(User user)
        {
            return new UserInfoDto { Id = user.Id, Username = user.Username, Name = user.Name, Email = user.Email, Avatar = "https://ui-avatars.com/api/?name=John+Doe" };
        }
    }
    public class UserDto : UserInfoDto
    {
        public string Password { get; set; } = string.Empty;
        public static User MapToEntity(UserDto userDto)
        {
            return new User
            {
                Name = userDto.Name,
                Username = userDto.Username,
                Password = userDto.Password,
                Email = userDto.Email,
                Avatar = "https://ui-avatars.com/api/?name=John+Doe",
                CreatedAt = DateTime.UtcNow
            };
        }
    }

}
