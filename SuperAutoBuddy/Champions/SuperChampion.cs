// <copyright file="ISuperChampion.cs" company="SuperAutoBuddy">
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

namespace SuperAutoBuddy.Champions
{
    using EloBuddy;
    using EloBuddy.SDK;

    public abstract class SuperChampion
    {
        protected SuperChampion()
        {
            Game.OnTick += args =>
            {
                Always();
            };
        }

        protected Spell.SpellBase Q;
        protected Spell.SpellBase W;
        protected Spell.SpellBase E;
        protected Spell.SpellBase R;

        protected ISkillLogic QLogic;
        protected ISkillLogic WLogic;
        protected ISkillLogic ELogic;
        protected ISkillLogic RLogic;


        public virtual float HarassRange
        {
            get
            {
                return ObjectManager.Player.AttackRange;
            }
        }

        public virtual float MaxComboRange
        {
            get
            {
                return ObjectManager.Player.AttackRange;
            }
        }

        public virtual SpellSlot[] SkillSequence
        {
            // Defaults to R Q W E
            get { return new[] {SpellSlot.R, SpellSlot.Q, SpellSlot.W, SpellSlot.E }; }
        }

        public abstract void Combo(AIHeroClient target);
        public abstract void Harass(AIHeroClient target);
        public abstract void Flee();
        protected abstract void Always();
    }
}