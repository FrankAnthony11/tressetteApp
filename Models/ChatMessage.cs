namespace TresetaApp.Models
{
    public class ChatMessage
    {
        public ChatMessage(string player, string text)
        {
            this.Player = player;
            this.Text = text;
        }
        public string Player { get; set; }
        public string Text { get; set; }
    }
}