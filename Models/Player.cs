using System.Collections.Generic;
using System.Linq;

namespace TresetaApp.Models
{
    public class Player
    {
        private List<Card> _cards;
        public Player(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public List<Card> Cards
        {
            get
            {
                _cards=_cards.OrderBy(y => y.Color).ThenBy(y => y.Number).ToList();
                return _cards;
            }
            set { _cards = value; }
        }
        public string ConnectionId { get; set; }
        public int Points { get; set; } = 0;
        public int CalculatedPoints { get; set; }
    
    }
}