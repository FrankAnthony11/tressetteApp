using System.Collections.Generic;

namespace TresetaApp.Models.Dtos
{
    public class GameDto
    {
        public string Id { get; set; }
        public List<PlayerDto> Players { get; set; }
        public List<Card> MyCards { get; set; }
        public List<TeamDto> Teams { get; set; }
        public User UserTurnToPlay { get; set; }
        public List<CardAndUser> CardsPlayed { get; set; }
        public List<CardAndUser> CardsDrew { get; set; }
        public int DeckSize { get; set; }
        public int PlayUntilPoints { get; set; }
        public bool GameEnded { get; set; } = false;
    }
}