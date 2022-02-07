using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlaveMod
{
    public class DiscordSlave
    {
        public string firstName { get; set; }
        public string nickname { get; set; }
        public int base_diplomacy { get; set; }
        public int base_stewardship { get; set; }
        public int base_warfare { get; set; }
        public int base_intelligence { get; set; }
        public int base_health { get; set; }
        public int base_attack_speed { get; set; }
        public int base_accuracy { get; set; }
        public int base_speed { get; set; }
        public int base_crit { get; set; }
        public int base_armor { get; set; }
        public bool follow { get; set; }
        public string race { get; set; }
        public string traits { get; set; }
        public string boots { get; set; }
        public string amulet { get; set; }
        public string bodyarmor { get; set; }
        public string helmet { get; set; }
        public string weapon { get; set; }
        public string ring { get; set; }
        public bool is_twitch { get; set; }
        public bool revolt { get; set; }

    }
}
