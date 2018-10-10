using System.Collections.Generic;
using TresetaApp.Enums;

namespace TresetaApp.Models
{
    public class ExtraPoint
    {
        public ExtraPoint(List<Card> cards, TypeOfExtraPoint typeOfExtraPoint)
        {
            TypeOfExtraPoint=typeOfExtraPoint;
            Cards=cards;
        }
        public List<Card> Cards { get; set; }
        public TypeOfExtraPoint TypeOfExtraPoint { get; set; }
    }
}