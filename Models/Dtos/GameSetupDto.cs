using TresetaApp.Enums;

namespace TresetaApp.Models.Dtos
{
    public class GameSetupDto
    {
        public string Id { get; set; }
        public bool IsPasswordProtected { get; set; }
        public int PlayUntilPoints { get; set; }
        public int ExpectedNumberOfPlayers { get; set; }
        public TypeOfDeck TypeOfDeck { get; set; }

    }
}