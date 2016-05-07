// <copyright file="BuildLoader.cs" company="SuperAutoBuddy">
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
    using System.IO;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.Sandbox;
    using EloBuddy.SDK.Utils;

    using Humanizers;

    using Properties;

    public static class BuildLoader
    {
        private static readonly string BuildPath = SandboxConfig.DataDirectory + "SuperBuddy\\Builds\\";

        public static List<ShopItem> LoadBuild()
        {
            if (!Directory.Exists(BuildPath))
            {
                Directory.CreateDirectory(BuildPath);
            }

            var buildFile = Path.Combine(BuildPath, ObjectManager.Player.ChampionName + ".txt");

            if (!File.Exists(buildFile))
            {
                var fs = new StreamWriter(File.Create(buildFile));
                fs.Write(GetDefaultBuild());
                fs.Close();
                Logger.Debug("Generated default build for {0}", ObjectManager.Player.ChampionName);
            }

            return ReadFile(buildFile);
        }

        private static string GetDefaultBuild()
        {
            var data = ChampionData.GetPlayerData();

            if (data.Magic > data.Attack)
            {
                return Resources.MageBuilds;
            }

            return Resources.MarksmanBuilds;
        }

        private static List<ShopItem> ReadFile(string buildFile)
        {
            try
            {
                var lines = File.ReadAllLines(buildFile);
                var builds = new List<List<ShopItem>>();
                foreach (var line in lines)
                {
                    var build = new List<ShopItem>();
                    foreach (var itemName in line.Split(';').Where(i => !string.IsNullOrEmpty(i)))
                    {
                        build.Add(ShopItem.GetFromDb(itemName));
                    }
                    builds.Add(build);
                }

                var variant = RandGen.RandomItem(builds);
                Logger.Debug("Loaded build from: {0}, variant = {1}", buildFile, builds.IndexOf(variant) + 1);
                return variant;
            }
            catch
            {
                Logger.Error("Could not parse build file {0}", buildFile);
                return null;
            }
        }
    }
}