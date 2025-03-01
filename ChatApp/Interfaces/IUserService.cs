using ChatApp.Dtos;

namespace ChatApp.Interfaces
{
    public interface IUserService
    {
        Task<bool> AddMultipleAsync(List<UserDto> dtos);
        Task<bool> AddAsync(UserDto dto);
        Task<UserInfoDto?> GetAsync(Guid id);
        Task<List<UserInfoDto>> GetAllAsync(Guid id);
        Task<LoginInfo> LoginUser(LoginDto dto);
    }
}
