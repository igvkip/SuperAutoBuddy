// <copyright file="CombatLogic.cs" company="SuperAutoBuddy">
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

namespace SuperAutoBuddy.MainLogics
{
    using System;
    using System.Drawing;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using Utilities;

    internal static class CombatLogic
    {
        private static bool active;
        private static string lastMode = " ";
        private static LogicSelector.Logics returnTo;

        static CombatLogic()
        {
            Game.OnTick += Game_OnTick;
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }
        }

        public static void Initialize()
        {
            
        }

        public static void Activate()
        {
            active = true;
        }

        public static void Deactivate()
        {
            active = false;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(
                250,
                25,
                Color.Gold,
                "CombatLogic, active:  " + active + " last mode: " + lastMode);
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (LogicSelector.IsSurvive())
            {
                return;
            }
            AIHeroClient har = null;
            AIHeroClient victim = null;
            if (SurvivalLogic.DangerValue < -15000)
            {
                victim =
                    EntityManager.Heroes.Enemies.Where(
                        vic =>
                            !vic.IsZombie
                            && vic.Distance(Walker.Hero) < vic.BoundingRadius + Walker.Hero.AttackRange + 450
                            && vic.IsVisible() && vic.Health > 0
                            && LocalAwareness.MyStrength()/LocalAwareness.HeroStrength(vic) > 1.5)
                        .OrderBy(v => v.Health)
                        .FirstOrDefault();
            }

            if (victim == null || Walker.Hero.GetNearestTurret().Distance(Walker.Hero) > 1100)
            {
                har =
                    EntityManager.Heroes.Enemies.Where(
                        h =>
                            !h.IsZombie && h.Distance(Walker.Hero) < Walker.Hero.AttackRange + h.BoundingRadius + 50
                            && h.IsVisible() && h.HealthPercent() > 0)
                        .OrderBy(h => h.Distance(Walker.Hero))
                        .FirstOrDefault();
            }

            if ((victim != null || har != null) && !active)
            {
                var returnT = LogicSelector.SetLogic(LogicSelector.Logics.CombatLogic);
                if (returnT != LogicSelector.Logics.CombatLogic)
                {
                    returnTo = returnT;
                }
            }
            if (!active)
            {
                return;
            }
            if (victim == null && har == null)
            {
                LogicSelector.SetLogic(returnTo);
                return;
            }
            if (victim != null)
            {
                LogicSelector.Champion.Combo(victim);
                var vicPos = Prediction.Position.PredictUnitPosition(victim, 500).To3D();

                var posToWalk = Walker.Hero.Position.Away(
                    vicPos,
                    (victim.BoundingRadius + LogicSelector.Champion.MaxComboRange - 30)
                    *Math.Min(
                        LocalAwareness.HeroStrength(victim)/ LocalAwareness.MyStrength()*1.6f,
                        1));

                if (NavMesh.GetCollisionFlags(posToWalk).HasFlag(CollisionFlags.Wall))
                {
                    posToWalk =
                        vicPos.Extend(
                            PushLogic.myTurret,
                            (victim.BoundingRadius + Walker.Hero.AttackRange - 30)
                            *Math.Min(
                                LocalAwareness.HeroStrength(victim)/ LocalAwareness.MyStrength()*2f,
                                1)).To3DWorld();
                }

                var nearestEnemyTurret = posToWalk.GetNearestTurret();
                lastMode = "combo";
                if (Walker.Hero.Distance(nearestEnemyTurret) < 950 + Walker.Hero.BoundingRadius)
                {
                    if (victim.Health > Walker.Hero.GetAutoAttackDamage(victim) + 15
                        || victim.Distance(Walker.Hero) > Walker.Hero.AttackRange + victim.BoundingRadius - 20)
                    {
                        lastMode = "enemy under turret, ignoring";
                        LogicSelector.SetLogic(returnTo);
                        return;
                    }
                    lastMode = "combo under turret";
                }

                Orbwalker.DisableAttacking = !ObjectManager.Player.IsInAutoAttackRange(victim);
                Walker.SetMode(Orbwalker.ActiveModes.Combo);
                Walker.WalkTo(posToWalk);

                if (Walker.HasGhost && Walker.Ghost.IsReady()
                    && Walker.Hero.HealthPercent()/victim.HealthPercent() > 2
                    && victim.Distance(Walker.Hero) > Walker.Hero.AttackRange + victim.BoundingRadius + 150
                    && victim.Distance(victim.Position.GetNearestTurret()) > 1500)
                {
                    Walker.Ghost.Cast();
                }

                if (ObjectManager.Player.HealthPercent() < 35)
                {
                    if (Walker.Hero.HealthPercent < 25)
                    {
                        Walker.UseSeraphs();
                    }
                    if (Walker.Hero.HealthPercent < 20)
                    {
                        Walker.UseBarrier();
                    }

                    Walker.UsePot();
                }

                if (Shop.CanShop == false)
                {
                    var hppotval = Program.HpPctUsePot;
                    if (ObjectManager.Player.HealthPercent() < hppotval)
                    {
                        Walker.UsePot();
                    }
                }
            }
            else
            {
                var harPos = Prediction.Position.PredictUnitPosition(har, 500).To3D();
                harPos = Walker.Hero.Position.Away(harPos, LogicSelector.Champion.HarassRange + har.BoundingRadius - 20);

                lastMode = "harass";
                var tu = harPos.GetNearestTurret();
                Walker.SetMode(Orbwalker.ActiveModes.Harass);
                if (harPos.Distance(tu) < 1000)
                {
                    if (harPos.Distance(tu) < 850 + Walker.Hero.BoundingRadius)
                    {
                        Walker.SetMode(Orbwalker.ActiveModes.Flee);
                    }
                    harPos = Walker.Hero.Position.Away(tu.Position, 1090);

                    lastMode = "harass under turret";

                    /*if (harPos.Distance(Walker.AllyNexus) > tu.Distance(Walker.AllyNexus))
                        harPos =
                            tu.Position.Extend(Walker.AllyNexus, 1050 + Walker.Hero.BoundingRadius).To3DWorld();*/
                }

                LogicSelector.Champion.Harass(har);

                Walker.WalkTo(harPos);
            }
        }
    }
}