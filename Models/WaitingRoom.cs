using System;

namespace TresetaApp.Models
{
    public class WaitingRoom
    {
        public WaitingRoom(string connectionId1, int playUntilPoints)
        {
            Id = Guid.NewGuid().ToString();
            ConnectionId1 = connectionId1;
            PlayUntilPoints=playUntilPoints;
        }
        public string Id { get; set; }
        public string ConnectionId1 { get; set; }
        public string ConnectionId2 { get; set; }
        public int PlayUntilPoints { get; set; }
    }
}