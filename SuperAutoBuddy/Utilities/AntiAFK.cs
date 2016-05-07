// <copyright file="AntiAFK.cs" company="SuperAutoBuddy">
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
    using System;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;

    using SharpDX;

    /// <summary>The anti afk.</summary>
    internal static class AntiAFK
    {
        /// <summary>The alerted.</summary>
        private static bool alerted;

        /// <summary>The current position.</summary>
        private static Vector3 currentPos;

        /// <summary>The current tick.</summary>
        private static int currentTick;

        /// <summary>The last position.</summary>
        private static Vector3 lastPos;

        /// <summary>The last tick.</summary>
        private static int lastTick;

        /// <summary>The my nexus.</summary>
        private static Obj_HQ myNexus;

        /// <summary>The start.</summary>
        public static void Start()
        {
            myNexus = ObjectManager.Get<Obj_HQ>().FirstOrDefault(n => n.IsAlly);
            lastTick = 0;

            Game.OnTick += args =>
            {
                if (!ObjectManager.Player.IsDead)
                {
                    currentTick = Environment.TickCount;

                    if (lastTick == 0)
                    {
                        lastTick = Environment.TickCount;
                    }

                    currentPos = ObjectManager.Player.ServerPosition;

                    if (lastPos.IsZero)
                    {
                        lastPos = ObjectManager.Player.ServerPosition;
                    }

                    var distance = myNexus.Position.Distance(currentPos);

                    if (currentPos.Distance(lastPos) > 200 && distance > 1800)
                    {
                        lastTick = currentTick;
                        lastPos = currentPos;
                    }

                    var ticksPassed = currentTick - lastTick;

                    if (ticksPassed > 60000 && !alerted)
                    {
                        alerted = true;
                        Chat.Print("Quiting in 10 seconds");
                    }

                    if (currentTick - lastTick > 70000)
                    {
                        Game.QuitGame();
                    }
                }
            };
        }
    }
}