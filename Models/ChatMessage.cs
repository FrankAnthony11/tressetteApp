using System;
using TresetaApp.Enums;

namespace TresetaApp.Models
{
    public class ChatMessage
    {
        public ChatMessage(string user, string text, TypeOfMessage typeOfMessage)
        {
            this.Username = user; ;
            this.Text = text;
            this.TypeOfMessage = typeOfMessage;
            this.CreatedUtc=DateTime.UtcNow;
        }
        public string Username { get; set; }
        public string Text { get; set; }
        public TypeOfMessage TypeOfMessage { get; set; }
        public DateTime CreatedUtc {get;set;}
    }
}