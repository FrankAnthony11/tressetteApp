using System;
namespace TresetaApp.Models
{
    public class User
    {
        public User(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
            LastBuzzedUtc=DateTime.UtcNow;
        }

        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public DateTime LastBuzzedUtc {get;set;}
    }
}