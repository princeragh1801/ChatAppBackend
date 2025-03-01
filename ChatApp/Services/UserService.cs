using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApp.Database;
using ChatApp.Dtos;
using ChatApp.Entites;
using ChatApp.Interfaces;

namespace ChatApp.Services
{
    public class UserService : IUserService
    {
        private readonly BackendContext _context;
        public UserService(BackendContext context)
        {
            _context = context;
        }
        public async Task<bool> AddMultipleAsync(List<UserDto> dtos)
        {
            try
            {

                var users = dtos.Select(x => new User
                {
                    //Bio = x.Bio,
                    Name = x.Name,
                    Username = x.Username,
                    Password = x.Password,
                }).ToList();
                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();  
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> AddAsync(UserDto dto)
        {
            try
            {
                var userExist = await GetUser(dto.Username);
                if(userExist != null)
                {
                    return false;
                }
                await _context.Users.AddAsync(UserDto.MapToEntity(dto));
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<UserInfoDto?> GetAsync(Guid id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
                if(user == null)
                {
                    return null;
                }

                return UserInfoDto.Convert(user);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<UserInfoDto>> GetAllAsync(Guid userId)
        {
            try
            {
                var users = await _context.Users.Where(x => x.Id != userId).Select(x => UserInfoDto.Convert(x)).ToListAsync();
                return users;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<LoginInfo> LoginUser(LoginDto dto)
        {
            try
            {
                var user = await GetUser(dto.Username);
                if(user == null)
                {
                    throw new Exception(ResponseCodes.UserNotFound);
                }
                if(user.Password != dto.Password)
                {
                    throw new Exception(ResponseCodes.InvalidPassword);
                }
                var token = CreateToken(user);
                var info = new LoginInfo
                {
                    Token = token,
                    User = UserInfoDto.Convert(user)
                };
                return info;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<User?> GetUser(string contact)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == contact);
            return user;
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("Username", user.Username),
                new Claim("Name", user.Name),
                new Claim("Email", user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("jkweoerjdsioaoitnksadiuhewiuejkjkdsoweoiwkt"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = new JwtSecurityToken(
                //issuer: "http://localhost:14536",
                //audience: "https://localhost:7219",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1000),
                signingCredentials: creds
            );

            return tokenHandler.WriteToken(token);
        }
    }
}
