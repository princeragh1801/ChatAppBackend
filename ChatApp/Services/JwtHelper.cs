using System.IdentityModel.Tokens.Jwt;

namespace ChatApp.Services
{
    public static class JwtHelper
    {
        public static JwtTokenDecoded DecodeJwtToken(HttpContext context)
        {
            var token = context.Request.Query["access_token"].ToString();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            var username = jsonToken?.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
            var email = jsonToken?.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
            var name = jsonToken?.Claims.FirstOrDefault(c => c.Type == "Name")?.Value;

            return new JwtTokenDecoded { UserId = userId??"", UserName = username??"", Email = email ?? "", Name = name??"" };
        }
    }

    public class JwtTokenDecoded
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

}
