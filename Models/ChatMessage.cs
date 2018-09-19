namespace TresetaApp.Models
{
    public class ChatMessage
    {
        public ChatMessage(User user, string text)
        {
            this.User = user; ;
            this.Text = text;
        }
        public User User { get; set; }
        public string Text { get; set; }
    }
}