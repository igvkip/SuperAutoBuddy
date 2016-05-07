// <copyright file="RandGen.cs" company="SuperAutoBuddy">
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

namespace SuperAutoBuddy.Humanizers
{
    using System;
    using System.Collections.Generic;

    using EloBuddy;
    using EloBuddy.SDK;

    internal static class RandGen
    {
        private static int lastPath = 1;

        static RandGen()
        {
            Randomizer = new Random();
            Core.DelayAction(ChangeSeed, 5000);
            Obj_AI_Base.OnNewPath += Obj_AI_Base_OnNewPath;
        }

        public static Random Randomizer { get; private set; }

        public static void Start()
        {
        }

        private static void ChangeSeed()
        {
            Randomizer =
                new Random(
                    DateTime.Now.Millisecond*lastPath*(int) (Game.CursorPos.X + 1000)
                    *(int) (Game.CursorPos.Y + 1000));
            Core.DelayAction(ChangeSeed, Randomizer.Next(10000, 20000));
        }

        private static void Obj_AI_Base_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            lastPath = DateTime.Now.Millisecond;
        }

        public static T RandomItem<T>(List<T> list)
        {
            return list[Randomizer.Next(list.Count)];
        }
    }
}