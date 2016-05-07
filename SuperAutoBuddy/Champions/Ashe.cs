// <copyright file="Ashe.cs" company="SuperAutoBuddy">
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
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Utils;

    public class Ashe : SuperChampion
    {
        public Ashe()
        {
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 0, int.MaxValue, 60);
            E = new Spell.Skillshot(SpellSlot.E, 25000, SkillShotType.Linear, 0, int.MaxValue, 60);
            R = new Spell.Skillshot(SpellSlot.R, 25000, SkillShotType.Linear, 500, 1000, 250);
            ((Spell.Skillshot) W).AllowedCollisionCount = 1;

            RLogic = new SkillShotLogic((Spell.Skillshot) R);
            WLogic = new SkillShotLogic((Spell.Skillshot) W);
        }

        public override SpellSlot[] SkillSequence
        {
            get { return new[] {SpellSlot.R, SpellSlot.W, SpellSlot.Q, SpellSlot.E}; }
        }

        public override void Combo(AIHeroClient target)
        {
            Logger.Info("Combo");
            RLogic.Cast(2000);
            WLogic.Cast(1150, DamageType.Physical, HitChance.Medium);

            AsheQ();
        }

        private void AsheQ()
        {
            if (Player.Instance.CountEnemiesInRange(620) > 0 && 
                ObjectManager.Player.GetBuffCount("asheqcastready")> 0)
            {
                Q.Cast();
            }
        }

        public override void Harass(AIHeroClient target)
        {
            WLogic.Cast(1150);
            AsheQ();
        }

        public override void Flee()
        {
            WLogic.Cast(700);
        }

        protected override void Always()
        {
            WLogic.Ks((int) W.Range);
        }
    }
}