using System.Collections.Generic;
using System.Linq;

namespace TresetaApp.Models
{
    public class Player
    {
        private List<Card> _cards;
        public Player(User user)
        {
            User = user;
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
        public User User { get; set; }
    
    }
}