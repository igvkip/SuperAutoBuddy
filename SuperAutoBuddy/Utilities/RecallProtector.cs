// <copyright file="RecallProtector.cs" company="SuperAutoBuddy">
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
    using EloBuddy;
    using EloBuddy.SDK;

    /// <summary>The recall protector.</summary>
    internal static class RecallProtector
    {
        /// <summary>The start.</summary>
        public static void Start()
        {
            Player.OnIssueOrder += (sender, args) =>
            {
                if (sender.IsMe && ObjectManager.Player.IsRecalling())
                {
                    args.Process = false;
                }
            };

            Spellbook.OnCastSpell += (sender, args) =>
            {
                if (sender.Owner.IsMe && ObjectManager.Player.IsRecalling())
                {
                    args.Process = false;
                }
            };
        }
    }
}