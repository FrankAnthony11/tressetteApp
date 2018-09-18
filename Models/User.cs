namespace TresetaApp.Models
{
    public class User
    {
        public User(string connectionId, string name)
        {
            ConnectionId=connectionId;
            Name=name;
        }
        public string ConnectionId { get; set; }
        public string Name { get; set; }
    }
}