using System;

namespace TresetaApp.Enums
{
    public enum CardColor
    {
        Denari=1,
        Spade,
        Coppe,
        Bastoni

    }
    public enum CardNumber
    {
        Four=1,
        Five,
        Six,
        Seven,
        Fante,
        Cavallo,
        Re,
        Ace,
        Two,
        Three
    }
    public enum TypeOfMessage{
        Chat,
        Server,
        Spectators
    }
    public enum TypeOfExtraPoint{
        Napoletana,
        SameKind
    }
    public enum TypeOfDeck{
        Napoletano = 1,
        Triestino 
    }

    public enum GameMode{
        Plain,
        Evasion
    }
}