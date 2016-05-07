// <copyright file="RecallLogic.cs" company="SuperAutoBuddy">
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

    internal static class RecallLogic
    {
        private static readonly Obj_SpawnPoint Spawn;

        private static bool _active;

        private static GrassObject _g;

        //private float lastRecallGold;
        private static float _lastRecallTime;

        static RecallLogic()
        {
            var menu = Program.Menu.AddSubMenu("Recall settings", "ergtrh");

            foreach (var so in
                ObjectManager.Get<Obj_SpawnPoint>().Where(so => so.Team == ObjectManager.Player.Team))
            {
                Spawn = so;
            }
            Core.DelayAction(ShouldRecall, 3000);
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }
        }

        public static void Activate()
        {
            if (_active)
            {
                return;
            }
            _active = true;
            _g = null;
            Game.OnTick += Game_OnTick;
        }

        public static void Deactivate()
        {
            _lastRecallTime = 0;
            _active = false;
            Game.OnTick -= Game_OnTick;
        }

        private static void CastRecall()
        {
            if (ObjectManager.Player.Distance(Spawn) < 500)
            {
                return;
            }
            Core.DelayAction(CastRecall2, 300);
        }

        private static void CastRecall2() //Kappa
        {
            if (ObjectManager.Player.Distance(Spawn) < 500)
            {
                return;
            }

            if (!Walker.Recalling())
            {
                if (Walker.Recall.IsReady())
                {
                    Walker.Recall.Cast();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(
                250,
                55,
                Color.Gold,
                "RecallLogic, active: " + _active + " next item: " + AutoShopper.Next + " gold needed:"
                + AutoShopper.MissingGold);
        }

        private static void Game_OnTick(EventArgs args)
        {
            Walker.SetMode(Orbwalker.ActiveModes.Combo);
            if (ObjectManager.Player.Distance(Spawn) < 400 && ObjectManager.Player.HealthPercent() > 85
                && (ObjectManager.Player.ManaPercent > 80 || ObjectManager.Player.PARRegenRate <= .0001))
            {
                LogicSelector.SetLogic(LogicSelector.Logics.PushLogic);
            }
            else if (ObjectManager.Player.Distance(Spawn) < 2000)
            {
                Walker.WalkTo(Spawn.Position);
            }
            else if (!ObjectManager.Player.IsRecalling() && Game.Time > _lastRecallTime)
            {
                var isSafe = false;
                var nearestTurret =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(t => t.Team == ObjectManager.Player.Team && !t.IsDead())
                        .OrderBy(t => t.Distance(ObjectManager.Player))
                        .First();

                var recallPos = nearestTurret.Position.Extend(Spawn, 300).To3DWorld();

                // find near safe location
                if (!EntityManager.Heroes.Enemies.Any(e => !e.IsDead && e.Distance(Walker.Hero) < 2000) &&
                    !EntityManager.Turrets.Enemies.Any(t => t.Distance(Walker.Hero) < 1500))
                {
                    recallPos = Walker.Hero.Position;
                    isSafe = true;
                }

                if (!isSafe && Walker.Hero.HealthPercent() > 35)
                {
                    if (_g == null)
                    {
                        _g =
                            ObjectManager.Get<GrassObject>()
                                .Where(
                                    gr =>
                                        gr.Distance(Walker.AllyNexus) < Walker.Hero.Distance(Walker.AllyNexus)
                                        && gr.Distance(Walker.Hero) > Orbwalker.HoldRadius)
                                .OrderBy(gg => gg.Distance(Walker.Hero))
                                .FirstOrDefault(
                                    gr => ObjectManager.Get<GrassObject>().Count(gr2 => gr.Distance(gr2) < 65) >= 4);
                    }
                    if (_g != null && _g.Distance(Walker.Hero) < nearestTurret.Position.Distance(Walker.Hero))
                    {
                        Walker.SetMode(Orbwalker.ActiveModes.Flee);
                        recallPos = _g.Position;
                    }
                }

                if ((!Walker.Hero.IsMoving && ObjectManager.Player.Distance(recallPos) < Orbwalker.HoldRadius + 50)
                    || (Walker.Hero.IsMoving && ObjectManager.Player.Distance(recallPos) < 50))
                {
                    CastRecall();
                }
                else
                {
                    Walker.WalkTo(recallPos);
                }
            }
        }

        private static void ShouldRecall()
        {
            if (_active)
            {
                Core.DelayAction(ShouldRecall, 500);
                return;
            }
            if (LogicSelector.Current == LogicSelector.Logics.CombatLogic)
            {
                Core.DelayAction(ShouldRecall, 500);
                return;
            }

            if (AutoShopper.HasEnoughGold() || Walker.Hero.HealthPercent() < 25)
            {
                LogicSelector.SetLogic(LogicSelector.Logics.RecallLogic);
            }
            Core.DelayAction(ShouldRecall, 500);
        }

        public static void Initialize()
        {
            // let static ctor work
        }
    }
}