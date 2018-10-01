using TresetaApp.Models;

namespace TresetaApp.Models.Dtos
{
    public class PlayerDto
    {
        public User User { get; set; }
        public bool LeftGame { get; set; }
    }
}