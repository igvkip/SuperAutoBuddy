// <copyright file="LocalAwareness.cs" company="SuperAutoBuddy">
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
    using EloBuddy.SDK;

    using SharpDX;

    internal static class LocalAwareness
    {
        public static readonly List<HeroInfo> heroTable;

        public static readonly HeroInfo me;

        static LocalAwareness()
        {
            heroTable = new List<HeroInfo>();
            foreach (var h in EntityManager.Heroes.AllHeroes)
            {
                if (h.IsMe)
                {
                    me = new HeroInfo(h);
                    heroTable.Add(me);
                }
                else
                {
                    heroTable.Add(new HeroInfo(h));
                }
            }
        }

        public static float HeroStrength(HeroInfo h)
        {
            return h.hero.HealthPercent()*(100 + h.hero.Level*10 + h.kills*5);
        }

        public static float HeroStrength(AIHeroClient h)
        {
            return HeroStrength(heroTable.First(he => he.hero == h));
        }

        public static int LocalDomination(Vector3 pos)
        {
            float danger = 0;
            foreach (var h in
                heroTable.Where(hh => hh.hero.IsVisible() && !hh.hero.IsDead() && hh.hero.Position.Distance(pos) < 900))
            {
                if (h.hero.IsZombie)
                {
                    danger += (-0.0042857142857143f*(h.hero.Distance(pos) + 100) + 4.4285714285714f)*15000
                              *(h.hero.IsEnemy ? 1 : -1);
                }

                else
                {
                    danger += (-0.0042857142857143f*(h.hero.Distance(pos) + 100) + 4.4285714285714f)*HeroStrength(h)
                              *(h.hero.IsEnemy ? 1 : -1);
                }
            }
            foreach (var tt in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(min => min.Health > 0 && min.Distance(pos) < 550 + Walker.Hero.BoundingRadius))
            {
                if (tt.Name.StartsWith("H28-G"))
                {
                    danger += 10000*(tt.IsAlly ? -1 : 1);
                }
                else if (tt.CombatType == GameObjectCombatType.Ranged)
                {
                    danger += 800*(tt.IsAlly ? -1 : 2);
                }
                else if (tt.CombatType == GameObjectCombatType.Ranged
                         && tt.Distance(Walker.Hero) < 130 + Walker.Hero.BoundingRadius)
                {
                    danger += 800*(tt.IsAlly ? -1 : 2);
                }
            }
            if (Walker.Hero.GetNearestTurret().Distance(pos) < 1000 + Walker.Hero.BoundingRadius)
            {
                danger += 35000;
            }
            if (Walker.Hero.GetNearestTurret(false).Distance(pos) < 400)
            {
                danger -= 35000;
            }
            return (int) danger;
        }

        public static int LocalDomination(Obj_AI_Base ob)
        {
            return LocalDomination(ob.Position);
        }

        public static float MyStrength()
        {
            return HeroStrength(me);
        }

        public static void Initialize()
        {
            // let static ctor work
        }
    }
}