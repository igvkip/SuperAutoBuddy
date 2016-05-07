// <copyright file="LogicSelector.cs" company="SuperAutoBuddy">
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

    using Champions;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using Utilities;

    internal static class LogicSelector
    {
        public static bool SaveMylife;
        public static readonly SuperChampion Champion;

        static LogicSelector()
        {
            Current = Logics.Nothing;
            Champion = ChampionFactory.CreateChampion(ObjectManager.Player.ChampionName);

            AutoLevel.Initialize();
            AutoShopper.Initialize();

            SurvivalLogic.Initialize();
            RecallLogic.Initialize();
            PushLogic.Initialize();
            CombatLogic.Initialize();

            BootstrapLogic.Initiliaze();
            LocalAwareness.Initialize();

            Core.DelayAction(BootstrapLogic.SetLane, 1000);
            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
            {
                Drawing.OnEndScene += Drawing_OnDraw;
            }

            Core.DelayAction(Watchdog, 3000);
        }

        public static void Initialize()
        {
            // let static ctor work
        }

        public static Logics Current { get; set; }

        public static Logics SetLogic(Logics newlogic)
        {
            if (SaveMylife)
            {
                return Current;
            }
            if (newlogic != Logics.PushLogic)
            {
                PushLogic.Deactivate();
            }
            var old = Current;
            switch (Current)
            {
                case Logics.SurviveLogic:
                    SurvivalLogic.Deactivate();
                    break;
                case Logics.RecallLogic:
                    RecallLogic.Deactivate();
                    break;
                case Logics.CombatLogic:
                    CombatLogic.Deactivate();
                    break;
            }

            switch (newlogic)
            {
                case Logics.PushLogic:
                    PushLogic.Activate();
                    break;
                case Logics.LoadLogic:
                    BootstrapLogic.Activate();
                    break;
                case Logics.SurviveLogic:
                    SurvivalLogic.Activate();
                    break;
                case Logics.RecallLogic:
                    RecallLogic.Activate();
                    break;
                case Logics.CombatLogic:
                    CombatLogic.Activate();
                    break;
            }

            Current = newlogic;
            return old;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawText(250, 85, Color.Gold, Current.ToString());
            var v = Game.CursorPos.WorldToScreen();
            Drawing.DrawText(v.X, v.Y - 20, Color.Gold, LocalAwareness.LocalDomination(Game.CursorPos) + " ");
        }

        private static void Watchdog()
        {
            Core.DelayAction(Watchdog, 500);
            if (Current == Logics.Nothing && !BootstrapLogic.waiting)
            {
                Chat.Print("Hang detected");
                BootstrapLogic.SetLane();
            }
        }

        internal enum Logics
        {
            PushLogic,
            RecallLogic,
            LoadLogic,
            SurviveLogic,
            CombatLogic,
            Nothing
        }

        public static bool IsSurvive()
        {
            return Current == Logics.SurviveLogic;
        }
    }
}