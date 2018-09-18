using System;

namespace TresetaApp.Models
{
    public class WaitingRoom
    {
        public WaitingRoom(User user1, int playUntilPoints)
        {
            Id = Guid.NewGuid().ToString();
            User1 = user1;
            PlayUntilPoints=playUntilPoints;
        }
        public string Id { get; set; }
        public User User1 { get; set; }
        public User User2 { get; set; }
        public int PlayUntilPoints { get; set; }
    }
}