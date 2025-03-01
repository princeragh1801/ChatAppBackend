namespace ChatApp.Dtos
{
    public class LoginInfo
    {
        public string Token { get; set; } = string.Empty;
        public UserInfoDto User { get; set; }
    }
}
