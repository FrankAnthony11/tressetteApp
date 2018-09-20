using TresetaApp.Models;

namespace TresetaApp.Models
{
    public class CardAndUser
    {
        public CardAndUser(Card card, User user)
        {
            Card=card;
            User=user;
        }
        public Card Card { get; set; }
        public User User { get; set; }
    }
}