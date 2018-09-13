using System.Collections.Generic;

namespace TresetaApp.Models
{
    public class Player
    {
        public Player(List<Card> cards, string connectionId)
        {
            Cards = cards;
            ConnectionId = connectionId;
        }

        public List<Card> Cards { get; set; }
        public string ConnectionId { get; set; }
        public int Points { get; set; } = 0;
        public int CalculatedPoints
        {
            get { return Points / 3; }
        }
    }
}