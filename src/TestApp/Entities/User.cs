namespace TestApp
{
    public class User
    {
        public int UserId { get; }
        public string Name { get; }

        public User(int id, string name)
        {
            UserId = id;
            Name = name;
        }
    }
}
