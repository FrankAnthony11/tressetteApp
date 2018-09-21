using System.Collections.Generic;
using System.Linq;

namespace TresetaApp.Models
{
    public class Team
    {
        public Team(List<User> users)
        {
            Users = users;
            Name = string.Join(" & ", users.Select(y => y.Name));
        }
        public string Name { get; set; }
        public List<User> Users { get; set; }
        public int Points { get; set; } = 0;
        public int CalculatedPoints { get; set; }
    }
}