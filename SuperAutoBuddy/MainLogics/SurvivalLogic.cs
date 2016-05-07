// <copyright file="SurvivalLogic.cs" company="SuperAutoBuddy">
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
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using SharpDX;

    using Utilities;

    using Color = System.Drawing.Color;

    internal static class SurvivalLogic
    {
        private static bool active;

        private static int hits;

        private static int hits2;

        private static LogicSelector.Logics returnTo;

        private static float spierdalanko;

        static SurvivalLogic()
        {
            Game.OnTick += Game_OnTick;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            DecHits();
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }
        }

        public static void Activate()
        {
            active = true;
        }

        public static void Deactivate()
        {
            active = false;
        }

        private static void DecHits()
        {
            if (hits > 3)
            {
                hits = 3;
            }
            if (hits > 0)
            {
                hits--;
            }
            hits2--;
            Core.DelayAction(DecHits, 600);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(
                250,
                10,
                Color.Gold,
                "SurvivalLogic, active: " + active + "  hits: " + hits + "  dangervalue: " + DangerValue);
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (hits*20 > Walker.Hero.HealthPercent()
                || (hits2 >= 5 && Walker.Hero.Level < 8 && Walker.Hero.HealthPercent < 50
                    && !EntityManager.Heroes.Enemies.Any(
                        en =>
                            en.IsVisible() && en.HealthPercent < 10
                            && en.Distance(Walker.Hero) < LogicSelector.Champion.MaxComboRange)))
            {
                SetSpierdalanko(.5f);
            }
            DangerValue = LocalAwareness.LocalDomination(Walker.Hero);
            if (DangerValue > -2000 || Walker.Hero.Distance(Walker.EnemyFountain) < 1500)
            {
                SetSpierdalankoUnc(.5f);
                LogicSelector.SaveMylife = true;
            }
            if (!active)
            {
                return;
            }
            if (Shop.CanShop == false)
            {
                var hppotval = Program.HpPctUsePot;
                if (ObjectManager.Player.HealthPercent() < hppotval)
                {
                    Walker.UsePot();
                }
            }
            if (Game.Time > spierdalanko)
            {
                LogicSelector.SaveMylife = false;
                LogicSelector.SetLogic(returnTo);
            }
            var enemyTurret = Walker.Hero.GetNearestTurret().Position;

            Vector3 closestTurret;
            if (Walker.Hero.Distance(enemyTurret) > 1200)
            {
                closestTurret = Walker.Hero.GetNearestTurret(false).Position;

                if (closestTurret.Distance(Walker.Hero) > 2000)
                {
                    var ally =
                        EntityManager.Heroes.Allies.Where(
                            a =>
                                a.Distance(Walker.Hero) < 1500
                                && LocalAwareness.LocalDomination(a.Position) < -40000)
                            .OrderBy(al => al.Distance(Walker.Hero))
                            .FirstOrDefault();
                    if (ally != null)
                    {
                        closestTurret = ally.Position;
                    }
                }
                if (closestTurret.Distance(Walker.Hero) > 150)
                {
                    var ene =
                        EntityManager.Heroes.Enemies.FirstOrDefault(
                            en => en.Health > 0 && en.Distance(closestTurret) < 300);
                    if (ene != null)
                    {
                        closestTurret = Walker.AllyNexus.Position;
                    }
                }
                Walker.SetMode(
                    Walker.Hero.Distance(closestTurret) < 200
                        ? Orbwalker.ActiveModes.Combo
                        : Orbwalker.ActiveModes.Flee);
                Walker.WalkTo(closestTurret.Extend(Walker.AllyNexus, 200).To3DWorld());
            }
            else
            {
                Walker.WalkTo(Walker.Hero.Position.Away(enemyTurret, 1200));
                Walker.SetMode(Orbwalker.ActiveModes.Flee);
            }

            if (Walker.Hero.HealthPercent < 10)
            {
                if (Walker.Hero.HealthPercent < 7)
                {
                    Walker.UseHeal();
                    Walker.UseBarrier();
                    //Walker.UseSeraphs();
                }
            }

            if (EntityManager.Heroes.Enemies.Any(en => en.IsVisible() && en.Distance(Walker.Hero) < 600))
            {
                //if (Walker.Hero.HealthPercent < 30)
                //    Walker.UseSeraphs();
                if (Walker.Hero.HealthPercent < 25)
                {
                    Walker.UseBarrier();
                }
                if (Walker.Hero.HealthPercent < 18)
                {
                    Walker.UseHeal();
                }
            }

            if (Walker.HasGhost && Walker.Ghost.IsReady() && DangerValue > 20000)
            {
                Walker.UseGhost();
            }
            if (DangerValue > 10000)
            {
                //if (Walker.Hero.HealthPercent < 45)
                //    Walker.UseSeraphs();
                if (Walker.Hero.HealthPercent < 30)
                {
                    Walker.UseBarrier();
                }
                if (Walker.Hero.HealthPercent < 25)
                {
                    Walker.UseHeal();
                }
            }

            LogicSelector.Champion.Flee();
        }

        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || args.Target == null)
            {
                return;
            }
            if (!args.Target.IsMe)
            {
                return;
            }
            if (sender.IsAlly)
            {
                return;
            }
            if (sender.Type == GameObjectType.obj_AI_Turret)
            {
                SetSpierdalanko((1100 - Walker.Hero.Distance(sender))/Walker.Hero.MoveSpeed);
            }
            else if (sender.Type == GameObjectType.obj_AI_Minion)
            {
                hits++;
                hits2++;
            }
            else if (sender.Type == GameObjectType.AIHeroClient)
            {
                hits += 2;
            }
        }

        private static void SetSpierdalanko(float sec)
        {
            spierdalanko = Game.Time + sec;
            if (active || (LogicSelector.Current == LogicSelector.Logics.CombatLogic && Walker.Hero.HealthPercent() > 13))
            {
                return;
            }
            var returnT = LogicSelector.SetLogic(LogicSelector.Logics.SurviveLogic);
            if (returnT != LogicSelector.Logics.SurviveLogic)
            {
                returnTo = returnT;
            }
        }

        private static void SetSpierdalankoUnc(float sec)
        {
            spierdalanko = Game.Time + sec;
            if (active)
            {
                return;
            }
            var returnT = LogicSelector.SetLogic(LogicSelector.Logics.SurviveLogic);
            if (returnT != LogicSelector.Logics.SurviveLogic)
            {
                returnTo = returnT;
            }
        }

        public static int DangerValue { get; set; }

        public static void Initialize()
        {
            // let static ctor work
        }
    }
}