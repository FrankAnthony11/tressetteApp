using TresetaApp.Enums;

namespace TresetaApp.Models
{
    public class ChatMessage
    {
        public ChatMessage(User user, string text, TypeOfMessage typeOfMessage)
        {
            this.User = user; ;
            this.Text = text;
            this.TypeOfMessage = typeOfMessage;
        }
        public User User { get; set; }
        public string Text { get; set; }
        public TypeOfMessage TypeOfMessage { get; set; }
    }
}