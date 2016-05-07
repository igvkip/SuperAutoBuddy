// <copyright file="PushLogic.cs" company="SuperAutoBuddy">
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
    using EloBuddy.SDK.Rendering;

    using Humanizers;

    using SharpDX;

    using Utilities;

    using Color = System.Drawing.Color;

    internal static class PushLogic
    {
        private static readonly ColorBGRA color;

        private static readonly ColorBGRA colorGreen;

        private static readonly ColorBGRA colorRed;

        private static bool active;

        private static Obj_AI_Minion[] currentWave;

        private static int CurrentWaveNum;

        private static Lane lane;

        private static float lastAtk;

        private static float lastRand;

        private static Vector3 rand;

        private static float randomAngle;

        private static float randomExtend;

        private static Vector3 randomVector;

        private static bool wholeWave;

        static PushLogic()
        {
            color = new ColorBGRA(255, 210, 105, 255);
            colorRed = new ColorBGRA(139, 0, 0, 255);
            colorGreen = new ColorBGRA(0, 100, 0, 255);
            SetRandVector();
            randomVector = new Vector3();
            currentWave = new Obj_AI_Minion[0];
            Core.DelayAction(SetWaveNumber, 500);
            SetCurrentWave();
            SetOffset();
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }
        }

        public static void Initialize()
        {
            // Let static constructor work
        }

        public static Obj_AI_Base enemyTurret { get; private set; }

        public static Obj_AI_Base myTurret { get; private set; }

        public static void Activate()
        {
            Walker.SetMode(Orbwalker.ActiveModes.LaneClear);
            LogicSelector.Current = LogicSelector.Logics.PushLogic;

            if (active)
            {
                return;
            }

            Game.OnTick += Game_OnTick;
            active = true;
        }

        public static void Deactivate()
        {
            active = false;
            Game.OnTick -= Game_OnTick;
        }

        public static void Reset(Obj_AI_Base myTower, Obj_AI_Base enemyTower, Lane ln)
        {
            var pingPos = Walker.Hero.Distance(Walker.AllyNexus) - 100 > myTower.Distance(Walker.AllyNexus)
                ? enemyTower.Position
                : myTower.Position;
            Core.DelayAction(() => SafeFunctions.Ping(PingCategory.OnMyWay, pingPos.Randomized()), RandGen.Randomizer.Next(3000));
            lane = ln;
            currentWave = new Obj_AI_Minion[0];
            myTurret = myTower;
            enemyTurret = enemyTower;
            randomExtend = 0;
            LogicSelector.SetLogic(LogicSelector.Logics.PushLogic);
        }

        private static Vector3 AvgPos(Obj_AI_Minion[] objects)
        {
            double x = 0, y = 0;
            foreach (var obj in objects)
            {
                x += obj.Position.X;
                y += obj.Position.Y;
            }
            return new Vector2((float) (x/objects.Count()), (float) (y/objects.Count())).To3DWorld();
        }

        private static void Between()
        {
            Walker.SetMode(Orbwalker.ActiveModes.LaneClear);
            var p = AvgPos(currentWave);
            if (p.Distance(Walker.AllyNexus) > myTurret.Distance(Walker.AllyNexus))
            {
                var ally =
                    EntityManager.Heroes.Allies.Where(
                        al =>
                            !al.IsMe && Walker.Hero.Distance(al) < 1500
                            && al.Distance(enemyTurret) < p.Distance(enemyTurret) + 100
                            && LocalAwareness.LocalDomination(al) < -10000)
                        .OrderBy(l => l.Distance(Walker.Hero))
                        .FirstOrDefault();
                if (ally != null
                    && Math.Abs(p.Distance(Walker.EnemyNexus) - Walker.Hero.Distance(Walker.EnemyNexus)) < 200)
                {
                    p = ally.Position.Extend(myTurret, 160).To3DWorld() + randomVector;
                }
                p =
                    p.Extend(
                        p.Extend(
                            Walker.Hero.Distance(myTurret) < Walker.Hero.Distance(enemyTurret)
                                ? myTurret
                                : enemyTurret,
                            400).To3D().RotatedAround(p, 1.57f),
                        randomExtend).To3DWorld();
                Walker.WalkTo(p);
            }
            else
            {
                UnderMyTurret();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(
                250,
                40,
                Color.Gold,
                "PushLogic, active: " + active + "  wave num: " + CurrentWaveNum + " minions left: " + currentWave.Length);
            Circle.Draw(color, 100, currentWave.Length <= 0 ? Walker.Hero.Position : AvgPos(currentWave));

            if (myTurret != null)
            {
                Circle.Draw(colorGreen, 200, myTurret.Position);
            }

            if (enemyTurret != null)
            {
                Circle.Draw(colorRed, 200, enemyTurret.Position);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Shop.CanShop == false)
            {
                var hppotval = Program.HpPctUsePot;
                if (ObjectManager.Player.HealthPercent() < hppotval)
                {
                    Walker.UsePot();
                }
            }
            if (!active || myTurret == null)
            {
                return;
            }
            if (!Walker.Hero.IsDead() && (myTurret.Health <= 0 || enemyTurret.Health <= 0))
            {
                BootstrapLogic.SetLane();
            }
            if (currentWave.Length == 0)
            {
                UnderMyTurret();
            }
            else if (Walker.Hero.Distance(enemyTurret) < 950 + Walker.Hero.BoundingRadius)
            {
                UnderEnemyTurret();
            }
            else
            {
                Between();
            }
        }

        private static void SetCurrentWave()
        {
            if (CurrentWaveNum == 0)
            {
                Core.DelayAction(SetCurrentWave, 1000);
                return;
            }
            currentWave = currentWave.Where(min => min.Health > 1).ToArray();
            if (currentWave.Length > 1)
            {
                Core.DelayAction(SetCurrentWave, 1000);
                return;
            }

            var newMinions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(min => min.IsAlly && min.GetLane() == lane && min.GetWave() == CurrentWaveNum)
                    .ToArray();
            if (!wholeWave && newMinions.Length < 7)
            {
                wholeWave = true;

                Core.DelayAction(
                    SetCurrentWave,
                    newMinions.Any(min => min.Distance(Walker.AllyNexus) < 800) ? 3000 : 300);
            }
            else
            {
                wholeWave = false;
                currentWave = newMinions;
                Core.DelayAction(SetCurrentWave, 1000);
            }
        }

        private static void SetOffset()
        {
            if (!active)
            {
                Core.DelayAction(SetOffset, 500);
                return;
            }
            var newEx = randomExtend;
            while (Math.Abs(newEx - randomExtend) < 190)
            {
                newEx = RandGen.Randomizer.NextFloat(-400, 400);
            }
            randomAngle = RandGen.Randomizer.NextFloat(0, 6.28f);
            randomExtend = newEx;
            Core.DelayAction(SetOffset, RandGen.Randomizer.Next(800, 1600));
        }

        private static void SetRandVector()
        {
            randomVector.X = RandGen.Randomizer.NextFloat(0, 300);
            randomVector.Y = RandGen.Randomizer.NextFloat(0, 300);
            Core.DelayAction(SetRandVector, 1000);
        }

        private static void SetWaveNumber()
        {
            Core.DelayAction(SetWaveNumber, 500);
            var closest =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        min => min.IsAlly && min.Name.Length > 13 && min.GetLane() == lane && min.HealthPercent() > 80)
                    .OrderBy(min => min.Distance(enemyTurret))
                    .FirstOrDefault();
            if (closest != null)
            {
                CurrentWaveNum = closest.GetWave();
            }
        }

        private static void UnderEnemyTurret()
        {
            if (
                ObjectManager.Get<Obj_AI_Minion>()
                    .Count(min => min.IsAlly && min.HealthPercent() > 30 && min.Distance(enemyTurret) < 850) < 2
                || EntityManager.Heroes.Enemies.Any(
                    en =>
                        en.IsVisible && en.HasBuffOfType(BuffType.Damage)
                        && Walker.Hero.HealthPercent - en.HealthPercent < 65 && en.Distance(enemyTurret) < 800
                        && Walker.Hero.Distance(enemyTurret) < Walker.Hero.BoundingRadius + 850))
            {
                Walker.SetMode(Orbwalker.ActiveModes.LaneClear);
                Walker.WalkTo(Walker.Hero.Position.Away(enemyTurret.Position, 1200));
                return;
            }
            if (Walker.Hero.Distance(enemyTurret)
                < Walker.Hero.AttackRange + enemyTurret.BoundingRadius + Orbwalker.HoldRadius
                && Walker.Hero.Distance(enemyTurret) > Walker.Hero.AttackRange)
            {
                Walker.SetMode(Orbwalker.ActiveModes.None);
                if (Game.Time > lastAtk)
                {
                    lastAtk = Game.Time + RandGen.Randomizer.NextFloat(.2f, .4f);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, enemyTurret);
                }
            }
            else
            {
                Walker.SetMode(Orbwalker.ActiveModes.LastHit);
                Walker.WalkTo(
                    enemyTurret.Position.Extend(Walker.Hero, Walker.Hero.AttackRange + enemyTurret.BoundingRadius)
                        .To3DWorld());
            }
        }

        private static void UnderMyTurret()
        {
            if (Walker.Hero.Gold <= 100
                || !EntityManager.MinionsAndMonsters.EnemyMinions.Any(en => en.Distance(myTurret) < 1000))
            {
                if (Game.Time > lastRand)
                {
                    lastRand = Game.Time + RandGen.Randomizer.NextFloat(5, 10);
                    rand = new Vector3(RandGen.Randomizer.NextFloat(-400, 400), RandGen.Randomizer.NextFloat(-400, 400), 0);
                    while ((myTurret.Position.Extend(Walker.AllyNexus, 200).To3D() + rand).Distance(myTurret) < 250)
                    {
                        rand = new Vector3(RandGen.Randomizer.NextFloat(-400, 400), RandGen.Randomizer.NextFloat(-400, 400), 0);
                    }
                }

                Walker.WalkTo(myTurret.Position.Extend(Walker.AllyNexus, 200).To3D() + rand);
                return;
            }

            var p = new Vector3();
            var ally =
                EntityManager.Heroes.Allies.Where(
                    al =>
                        !al.IsMe && Walker.Hero.Distance(al) < 1200
                        && al.Distance(enemyTurret) < p.Distance(enemyTurret) + 150
                        && LocalAwareness.LocalDomination(al) < -15000)
                    .OrderBy(l => l.Distance(Walker.Hero))
                    .FirstOrDefault();
            if (Walker.Hero.Gold > 100 && ally != null)
            {
                p = ally.Position.Extend(myTurret, 160).To3DWorld() + randomVector;
                Walker.SetMode(
                    Walker.Hero.Distance(enemyTurret) < 900
                        ? Orbwalker.ActiveModes.LastHit
                        : Orbwalker.ActiveModes.LaneClear);
                Walker.WalkTo(p);
            }
            else
            {
                Walker.WalkTo(
                    myTurret.Position.Extend(Walker.Hero.Position, 350 + randomExtend/2)
                        .To3D()
                        .RotatedAround(myTurret.Position, randomAngle));
            }
        }
    }
}