namespace ChatApp.Services
{
    public class UserService
    {
        public required string UserName { get; set; }


        public void RegisterUser(string username)
        {
            UserName = username;
        }
    }
}
