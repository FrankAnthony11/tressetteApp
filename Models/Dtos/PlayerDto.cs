using TresetaApp.Models;

namespace TresetaApp.Models.Dtos
{
    public class PlayerDto
    {
        public int Points { get; set; } = 0;
        public int CalculatedPoints { get; set; }
        public User User { get; set; }
    }
}