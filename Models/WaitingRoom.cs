using System;
using System.Collections.Generic;

namespace TresetaApp.Models
{
    public class WaitingRoom
    {
        public WaitingRoom(User user, int playUntilPoints, int expectedNumberOfPlayers)
        {
            Id = Guid.NewGuid().ToString();
            Users = new List<User>() { user };
            PlayUntilPoints = playUntilPoints;
            ExpectedNumberOfPlayers=expectedNumberOfPlayers;
        }

        public string Id { get; set; }
        public string Password { get; set; }
        public List<User> Users { get; set; }
        public int PlayUntilPoints { get; set; }
        public int ExpectedNumberOfPlayers { get; set; }
    }
}