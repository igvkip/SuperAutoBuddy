// <copyright file="AutoLevel.cs" company="SuperAutoBuddy">
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
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;

    using Humanizers;

    using MainLogics;

    public static class AutoLevel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        static AutoLevel()
        {
            Obj_AI_Base.OnLevelUp += Player_OnLevelUp;
            OnLvLUp();
        }

        /// <summary>
        /// Event handler for the player level up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void Player_OnLevelUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (sender != ObjectManager.Player)
            {
                return;
            }

            // Humanizes
            Core.DelayAction(OnLvLUp, RandGen.Randomizer.Next(920, 3010));
        }

        /// <summary>
        /// Levels up the correct skill
        /// </summary>
        private static void OnLvLUp()
        {
            if (ObjectManager.Player.SpellTrainingPoints == 0)
            {
                return;
            }

            // checks if player has at least one value point on each skill
            var missingBasic = GetMissingBasicSlot();

            if (missingBasic != SpellSlot.Unknown)
            {
                Upgrade(missingBasic);
                return;
            }

            // then follows priority
            foreach (var slot in LogicSelector.Champion.SkillSequence)
            {
                switch (slot)
                {
                    case SpellSlot.Q:
                        if (Upgrade(SpellSlot.Q))
                        {
                            return;
                        }
                        break;
                    case SpellSlot.W:
                        if (Upgrade(SpellSlot.W))
                        {
                            return;
                        }
                        break;
                    case SpellSlot.E:
                        if (Upgrade(SpellSlot.E))
                        {
                            return;
                        }
                        break;
                    case SpellSlot.R:
                        if (Upgrade(SpellSlot.R))
                        {
                            return;   
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Returns a non ultimate SpellSlot that has not been leveled yet,
        /// following the skill sequence priority
        /// </summary>
        /// <returns>The missing SpellSlot</returns>
        private static SpellSlot GetMissingBasicSlot()
        {
            foreach (var basic in LogicSelector.Champion.SkillSequence.Where(s => s != SpellSlot.R))
            {
                if (ObjectManager.Player.Spellbook.GetSpell(basic).Level == 0)
                {
                    return basic;
                }
            }

            return SpellSlot.Unknown;
        }

        /// <summary>
        /// Tries to upgrade the slot
        /// </summary>
        /// <param name="slot">The slot to upgrade</param>
        /// <returns>True if successful, false otherwise</returns>
        private static bool Upgrade(SpellSlot slot)
        {
            if (ObjectManager.Player.Spellbook.CanSpellBeUpgraded(slot))
            {
                return ObjectManager.Player.Spellbook.LevelSpell(slot);
            }

            return false;
        }

        /// <summary>
        /// Let the static constructor do the work
        /// </summary>
        public static void Initialize()
        {
            // static ctor
        }
    }
}