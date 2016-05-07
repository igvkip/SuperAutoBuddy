// <copyright file="ChampionData.cs" company="SuperAutoBuddy">
//   Copyright (C) 2016 SuperAutoBuddy
// </copyright>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace SuperAutoBuddy.Utilities
{
    using System.Collections.Generic;
    using System.Linq;

    using EloBuddy;

    using Newtonsoft.Json.Linq;

    using Properties;

    public class ChampionData
    {
        private static readonly List<ChampionData> Champions = new List<ChampionData>();
        public List<string> Tags = new List<string>();
        public string Name { get; set; }

        public int Attack { get; set; }
        public int Magic { get; set; }

        static ChampionData()
        {
            var json = JObject.Parse(Resources.champion);
            var jsonData = (JObject) json.GetValue("data");

            foreach (var champData in jsonData.Values())
            {
                var dragon = new ChampionData
                {
                    Name = champData["name"].ToString(),
                    Attack = (int) champData["info"]["attack"],
                    Magic = (int) champData["info"]["magic"],
                };

                dragon.Tags.AddRange(champData["tags"].Select(t => (string) t));


                Champions.Add(dragon);
            }
        }

       

        public static ChampionData GetByChampionName(string name)
        {
            return Champions.FirstOrDefault(c => c.Name == name);
        }

        public static ChampionData GetPlayerData()
        {
            return GetByChampionName(ObjectManager.Player.ChampionName);
        }
    }
}