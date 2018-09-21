using System.Collections.Generic;

namespace TresetaApp.Models.Dtos
{
    public class TeamDto
    {
        public string Name { get; set; }
        public List<User> Users { get; set; }
        public int CalculatedPoints { get; set; }
    }
}