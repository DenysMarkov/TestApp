using TestApp;

namespace TestAppAPI
{
    public interface IUserRepository
    {
        Task CreateUser(User user);
        Task DeleteUser(int userId);
    }
}
