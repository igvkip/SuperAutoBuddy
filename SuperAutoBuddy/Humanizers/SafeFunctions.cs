// <copyright file="SafeFunctions.cs" company="SuperAutoBuddy">
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
    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using SharpDX;

    internal static class SafeFunctions
    {
        private static float lastChat;

        private static float lastPing;

        static SafeFunctions()
        {
            lastChat = 0;
        }

        public static void Ping(PingCategory cat, Vector3 pos)
        {
            if (MainMenu.GetMenu("AB").Get<CheckBox>("disablepings").CurrentValue)
            {
                return;
            }
            if (lastPing > Game.Time)
            {
                return;
            }
            lastPing = Game.Time + 1.8f;
            Core.DelayAction(() => TacticalMap.SendPing(cat, pos), RandGen.Randomizer.Next(450, 800));
        }

        public static void Ping(PingCategory cat, GameObject target)
        {
            if (MainMenu.GetMenu("AB").Get<CheckBox>("disablepings").CurrentValue)
            {
                return;
            }
            if (lastPing > Game.Time)
            {
                return;
            }
            lastPing = Game.Time + 1.8f;
            Core.DelayAction(() => TacticalMap.SendPing(cat, target), RandGen.Randomizer.Next(450, 800));
        }

        public static void SayChat(string msg)
        {
            if (MainMenu.GetMenu("AB").Get<CheckBox>("disablechat").CurrentValue)
            {
                return;
            }
            if (lastChat > Game.Time)
            {
                return;
            }
            lastChat = Game.Time + .8f;
            Core.DelayAction(() => Chat.Say(msg), RandGen.Randomizer.Next(150, 400));
        }
    }
}