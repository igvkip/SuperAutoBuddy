// <copyright file="HeroInfo.cs" company="SuperAutoBuddy">
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

    internal class HeroInfo
    {
        public AIHeroClient hero;

        public HeroInfo(AIHeroClient h)
        {
            hero = h;
            Game.OnNotify += Game_OnNotify;
        }

        public int assists
        {
            get { return hero.Assists; }
        }

        public int deaths
        {
            get { return hero.Deaths; }
        }

        public int farm
        {
            get { return hero.MinionsKilled + hero.NeutralMinionsKilled; }
        }

        public int kills { get; private set; }

        public int kills2
        {
            get { return hero.ChampionsKilled; }
        }

        private void Game_OnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnChampionKill && args.NetworkId == hero.NetworkId)
            {
                kills++;
            }
        }
    }
}