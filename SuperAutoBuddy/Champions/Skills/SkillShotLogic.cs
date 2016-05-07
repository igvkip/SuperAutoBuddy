// <copyright file="SkillShotLogic.cs" company="SuperAutoBuddy">
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
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Utils;

    /// <summary>
    /// Generic skillshot logic, use this if you don't want to implement custom logics
    /// </summary>
    public class SkillShotLogic : ISkillLogic
    {
        private readonly Spell.Skillshot _spell;

        public SkillShotLogic(Spell.Skillshot spell)
        {
            _spell = spell;
        }

        public void Cast(int acquisitionRange, DamageType damageType = DamageType.Physical,
            HitChance chance = HitChance.High)
        {
            if (!_spell.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(acquisitionRange, damageType);

            if (target == null)
            {
                return;
            }

            // if target is in AA range and near death, we don't risk an skillshot
            if (ObjectManager.Player.IsInAutoAttackRange(target) &&
                ObjectManager.Player.GetAutoAttackDamage(target, true)*2 <= target.Health)
            {
                return;
            }

            CastIfChance(target, chance);
        }

        private void CastIfChance(AIHeroClient target, HitChance chance = HitChance.High)
        {
            if (target != null && _spell.GetPrediction(target).HitChance >= chance)
            {
                _spell.Cast(target.Position);
            }
        }

        public void Ks(int acquisitionRange, HitChance chance = HitChance.High)
        {
            var target = EntityManager.Heroes.Enemies
                .FirstOrDefault(e => !e.IsDead &&
                                     !e.IsZombie &&
                                     e.IsValidTarget(acquisitionRange) &&
                                     ObjectManager.Player.GetSpellDamage(e, _spell.Slot) > e.Health);

            if (target != null)
            {
                CastIfChance(target, chance);
            }
        }
    }
}