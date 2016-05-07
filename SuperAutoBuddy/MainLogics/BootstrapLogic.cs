// <copyright file="BootstrapLogic.cs" company="SuperAutoBuddy">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using Humanizers;

    using SharpDX;

    using Utilities;

    using Color = System.Drawing.Color;

    internal static class BootstrapLogic
    {
        private const float waitTime = 40;

        private static readonly float startTime;

        private static float lastSliderSwitch;

        private static string status = " ";

        public static bool waiting;

        private static bool waitingSlider;

        static BootstrapLogic()
        {
            startTime = Game.Time + waitTime + RandGen.Randomizer.NextFloat(-10, 20);
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }
            if (!Walker.Hero.Name.Equals("Challenjour Ryze"))
            {
                Chat.OnMessage += Chat_OnMessage;
            }
            MainMenu.GetMenu("AB").Get<CheckBox>("reselectlane").OnValueChange += Checkbox_OnValueChange;
            MainMenu.GetMenu("AB").Get<Slider>("lane").OnValueChange += Slider_OnValueChange;
        }

        public static void Activate()
        {
        }

        public static void Deactivate()
        {
        }

        public static void SelectLane2(Lane l)
        {
            status = "selected " + l;
            Obj_AI_Turret ally = null, enemy = null;

            if (l == Lane.Top)
            {
                ally =
                    (ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("L_03_A"))
                     ?? ObjectManager.Get<Obj_AI_Turret>()
                         .FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("L_02_A")))
                    ?? ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("L_01_A"));

                enemy =
                    (ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("L_03_A"))
                     ?? ObjectManager.Get<Obj_AI_Turret>()
                         .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("L_02_A")))
                    ?? ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("L_01_A"));
            }
            else if (l == Lane.Bot)
            {
                ally =
                    (ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("R_03_A"))
                     ?? ObjectManager.Get<Obj_AI_Turret>()
                         .FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("R_02_A")))
                    ?? ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("R_01_A"));

                enemy =
                    (ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("R_03_A"))
                     ?? ObjectManager.Get<Obj_AI_Turret>()
                         .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("R_02_A")))
                    ?? ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("R_01_A"));
            }
            else if (l == Lane.Mid)
            {
                ally =
                    (ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("C_05_A"))
                     ?? ObjectManager.Get<Obj_AI_Turret>()
                         .FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("C_04_A")))
                    ?? ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsAlly && tur.Name.EndsWith("C_03_A"));

                enemy =
                    (ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("C_05_A"))
                     ?? ObjectManager.Get<Obj_AI_Turret>()
                         .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("C_04_A")))
                    ?? ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(tur => tur.IsEnemy && tur.Name.EndsWith("C_03_A"));
            }

            if (ally == null)
            {
                ally = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsAlly && tur.GetLane() == Lane.HQ);
            }
            if (ally == null)
            {
                ally =
                    ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsAlly && tur.GetLane() == Lane.Spawn);
            }

            if (enemy == null)
            {
                enemy = ObjectManager.Get<Obj_AI_Turret>()
                    .FirstOrDefault(tur => tur.IsEnemy && tur.GetLane() == Lane.HQ);
            }
            if (enemy == null)
            {
                enemy =
                    ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsEnemy && tur.GetLane() == Lane.Spawn);
            }

            PushLogic.Reset(ally, enemy, l);
        }

        public static void SetLane()
        {
            if (MainMenu.GetMenu("AB").Get<Slider>("lane").CurrentValue != 1)
            {
                switch (MainMenu.GetMenu("AB").Get<Slider>("lane").CurrentValue)
                {
                    case 2:
                        SelectLane2(Lane.Top);
                        break;
                    case 3:
                        SelectLane2(Lane.Mid);
                        break;
                    case 4:
                        SelectLane2(Lane.Bot);
                        break;
                }
                return;
            }

            if (ObjectManager.Get<Obj_AI_Turret>().Count() == 24)
            {
                if (Walker.Hero.Gold < 550 && MainMenu.GetMenu("AB").Get<CheckBox>("mid").CurrentValue)
                {
                    var p =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .First(tur => tur.IsAlly && tur.Name.EndsWith("C_05_A"))
                            .Position;

                    Core.DelayAction(
                        () => SafeFunctions.Ping(PingCategory.OnMyWay, p.Randomized()),
                        RandGen.Randomizer.Next(1500, 3000));
                    Core.DelayAction(() => SafeFunctions.SayChat("mid"), RandGen.Randomizer.Next(200, 1000));
                    Walker.SetMode(Orbwalker.ActiveModes.Combo);
                    Walker.WalkTo(
                        p.Extend(Walker.AllyNexus, 200 + RandGen.Randomizer.NextFloat(0, 100)).To3DWorld().Randomized());
                }

                CanSelectLane();
            }
            else
            {
                SelectMostPushedLane();
            }
        }

        private static List<ChampLane> GetChampLanes(float maxDistance = 2000, float maxDistanceFront = 3000)
        {
            var top1 =
                ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("L_03_A"));
            var top2 =
                ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("L_02_A"));
            var mid1 =
                ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("C_05_A"));
            var mid2 =
                ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("C_04_A"));
            var bot1 =
                ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("R_03_A"));
            var bot2 =
                ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("R_02_A"));

            var ret = new List<ChampLane>();

            foreach (var h in EntityManager.Heroes.Allies.Where(hero => hero.IsAlly && !hero.IsMe))
            {
                var lane = Lane.Unknown;
                if (h.Distance(top1) < maxDistanceFront || h.Distance(top2) < maxDistance)
                {
                    lane = Lane.Top;
                }
                if (h.Distance(mid1) < maxDistanceFront || h.Distance(mid2) < maxDistance)
                {
                    lane = Lane.Mid;
                }
                if (h.Distance(bot1) < maxDistanceFront || h.Distance(bot2) < maxDistance)
                {
                    lane = Lane.Bot;
                }
                ret.Add(new ChampLane(h, lane));
            }
            return ret;
        }

        private static void CanSelectLane()
        {
            waiting = true;
            status = "looking for free lane, time left " + (int) (startTime - Game.Time);
            if (Game.Time > startTime || GetChampLanes().All(cl => cl.lane != Lane.Unknown))
            {
                waiting = false;
                SelectLane();
            }
            else
            {
                Core.DelayAction(CanSelectLane, 500);
            }
        }

        private static void Chat_OnMessage(AIHeroClient sender, ChatMessageEventArgs args)
        {
            if (!args.Message.StartsWith("<font color=\"#40c1ff\">Challenjour Ryze"))
            {
                return;
            }
            if (args.Message.Contains("have fun"))
            {
                Core.DelayAction(() => Chat.Say("gl hf"), RandGen.Randomizer.Next(2000, 4000));
            }
            if (args.Message.Contains("hello"))
            {
                Core.DelayAction(() => Chat.Say("hi Christian"), RandGen.Randomizer.Next(2000, 4000));
            }
            if (args.Message.Contains("Which") || args.Message.Contains("Whats"))
            {
                Core.DelayAction(
                    () => Chat.Say(Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString()),
                    RandGen.Randomizer.Next(2000, 4000));
            }
            if (args.Message.Contains("go top please."))
            {
                Core.DelayAction(() => Chat.Say("kk"), RandGen.Randomizer.Next(1000, 2000));
                Core.DelayAction(() => SelectLane2(Lane.Top), RandGen.Randomizer.Next(2500, 4000));
            }
            if (args.Message.Contains("go mid please."))
            {
                Core.DelayAction(() => Chat.Say("ok"), RandGen.Randomizer.Next(1000, 2000));
                Core.DelayAction(() => SelectLane2(Lane.Mid), RandGen.Randomizer.Next(2500, 4000));
            }
            if (args.Message.Contains("go bot please."))
            {
                Core.DelayAction(() => Chat.Say("k"), RandGen.Randomizer.Next(1000, 2000));
                Core.DelayAction(() => SelectLane2(Lane.Bot), RandGen.Randomizer.Next(2500, 4000));
            }
            if (args.Message.Contains("go where you want."))
            {
                Core.DelayAction(() => Chat.Say("yes sir"), RandGen.Randomizer.Next(1000, 2000));
                Core.DelayAction(SelectLane, RandGen.Randomizer.Next(2500, 4000));
            }
            if (args.Message.Contains("Thank you"))
            {
                Core.DelayAction(() => Chat.Say("np"), RandGen.Randomizer.Next(1000, 2000));
                Core.DelayAction(SelectLane, RandGen.Randomizer.Next(2500, 4000));
            }
        }

        private static void Checkbox_OnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            ReselectLane();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(250, 70, Color.Gold, "Lane selector status: " + status);
        }

        private static void handleSlider(bool x = true)
        {
            if (waitingSlider && x)
            {
                return;
            }
            if (lastSliderSwitch > Game.Time)
            {
                waitingSlider = true;
                Core.DelayAction(() => handleSlider(false), (int) ((lastSliderSwitch - Game.Time)*1000) + 50);
            }
            else
            {
                ReselectLane();
            }
        }

        public static void ReselectLane()
        {
            SetLane();
            waitingSlider = false;
            Chat.Print("Reselecting lane");
        }

        private static void SelectLane()
        {
            status = "selected free lane";
            var list = GetChampLanes();
            if (list.All(cl => cl.lane != Lane.Mid))
            {
                PushLogic.Reset(
                    ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("C_05_A")),
                    ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsEnemy && tur.Name.EndsWith("C_05_A")),
                    Lane.Mid);
                return;
            }
            if (list.Count(cl => cl.lane == Lane.Bot) < 2)
            {
                PushLogic.Reset(
                    ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("R_03_A")),
                    ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsEnemy && tur.Name.EndsWith("R_03_A")),
                    Lane.Bot);
                return;
            }
            if (list.Count(cl => cl.lane == Lane.Top) < 2)
            {
                PushLogic.Reset(
                    ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsAlly && tur.Name.EndsWith("L_03_A")),
                    ObjectManager.Get<Obj_AI_Turret>().First(tur => tur.IsEnemy && tur.Name.EndsWith("L_03_A")),
                    Lane.Top);
            }
        }

        private static void SelectMostPushedLane()
        {
            status = "selected most pushed lane";
            var nMyNexus = ObjectManager.Get<Obj_HQ>().First(hq => hq.IsEnemy);

            var andrzej =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(min => min.Name.Contains("Minion") && min.IsAlly && min.Health > 0)
                    .OrderBy(min => min.Distance(nMyNexus))
                    .First();

            Obj_AI_Base ally =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly && tur.Health > 0 && tur.GetLane() == andrzej.GetLane())
                    .OrderBy(tur => tur.Distance(andrzej))
                    .FirstOrDefault();
            if (ally == null)
            {
                ally =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(tur => tur.Health > 0 && tur.IsAlly && tur.GetLane() == Lane.HQ)
                        .OrderBy(tur => tur.Distance(andrzej))
                        .FirstOrDefault();
            }
            if (ally == null)
            {
                ally =
                    ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsAlly && tur.GetLane() == Lane.Spawn);
            }

            Obj_AI_Base enemy =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsEnemy && tur.Health > 0 && tur.GetLane() == andrzej.GetLane())
                    .OrderBy(tur => tur.Distance(andrzej))
                    .FirstOrDefault();
            if (enemy == null)
            {
                enemy =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(tur => tur.Health > 0 && tur.IsEnemy && tur.GetLane() == Lane.HQ)
                        .OrderBy(tur => tur.Distance(andrzej))
                        .FirstOrDefault();
            }
            if (enemy == null)
            {
                enemy =
                    ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => tur.IsEnemy && tur.GetLane() == Lane.Spawn);
            }

            PushLogic.Reset(ally, enemy, andrzej.GetLane());
        }

        private static void Slider_OnValueChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            lastSliderSwitch = Game.Time + 1;
            handleSlider();
        }

        public static void Initiliaze()
        {
            // static ctor do work
        }
    }
}