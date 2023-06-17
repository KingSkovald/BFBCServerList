using System.Collections.Generic;

namespace BFBCServerList
{
    public static class LevelMaps
    {
        public static readonly IDictionary<string, string> GoldRushMaps = new Dictionary<string, string>
        {
            ["Levels/MP_BeachHead_Day1"] = "Harvest Day",
            ["Levels/MP_BeachHead_Day2"] = "Over and Out",
            ["Levels/MP_Harbour_Day1"] = "Valley Run",
            ["Levels/MP_Harbour_Day2"] = "Deconstruction",
            ["Levels/MP_Ignition_Day1"] = "Oasis",
            ["Levels/MP_Ignition_Day2"] = "Final Ignition",
            ["Levels/MP_Monastery_Day1"] = "End of the Line",
            ["Levels/MP_Monastery_Day2"] = "Ascension"
        };

        public static readonly IDictionary<string, string> ConquestMaps = new Dictionary<string, string>
        {
            ["levels/mp_beachhead_day1"] = "Harvest Day",
            ["levels/mp_beachhead_day2"] = "Crossing Over",
            ["levels/mp_harbour_day1"] = "Par for the Course",
            ["levels/mp_harbour_day2"] = "Ghost Town",
            ["levels/mp_ignition_day1"] = "Oasis",
            ["levels/mp_ignition_day2"] = "Acta Non Verba",
            ["levels/mp_monastery_day1"] = "End of the Line",
            ["levels/mp_monastery_day2"] = "Ascension"
        };
    }
}
