// <copyright file="Program.cs" company="SuperAutoBuddy">
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

namespace SuperAutoBuddy
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using EloBuddy;
    using EloBuddy.Sandbox;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using Humanizers;

    using MainLogics;

    using Utilities;
    //using Utilities.AutoLvl;
    //using Utilities.AutoShop;

    using Color = System.Drawing.Color;

    /// <summary>The program.</summary>
    internal static class Program
    {
        public static Item BlackSpear { get; set; }
        public static int HpPctUsePot { get; set; }

        /// <summary>The menu.</summary>
        public static Menu Menu;

        private static string _version;

        /// <summary>The main.</summary>
        public static void Main()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            _version = v.Major + "." + v.MajorRevision + "." + v.Minor + "." + v.MinorRevision;
            Tests(EventArgs.Empty);
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            //Loading.OnLoadingComplete += Tests;
        }

        private static void Tests(EventArgs args)
        {
            //AutoShopper.Initialize();
            ChampionData.GetByChampionName("Aatrox");
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            // AutoBlack Spear
            if (ObjectManager.Player.Hero == Champion.Kalista)
            {
                BlackSpear = new Item(ItemId.The_Black_Spear);
                Chat.Print("Auto Black Spear loaded! Thanks @Enelx");
                Game.OnUpdate += On_Update;
            }

            SayHello();
            Core.DelayAction(Start, 5000);
            CreateMenu();
        }

        private static void CreateMenu()
        {
            Menu = MainMenu.AddMenu("AUTOBUDDY", "AB");
            Menu.Add("sep1", new Separator(1));
            var c = new CheckBox("Call mid, will leave if other player stays on mid(only auto lane)", true);
            Menu.Add("mid", c);
            var s = Menu.Add("lane", new Slider("", 1, 1, 4));
            string[] lanes =
            {
                string.Empty, "Selected lane: Auto", "Selected lane: Top", "Selected lane: Mid",
                "Selected lane: Bot"
            };
            // updates lane name on the Slider
            s.DisplayName = lanes[s.CurrentValue];
            s.OnValueChange += (sender, args) =>
            {
                sender.DisplayName = lanes[args.NewValue];
            };
            Menu.Add("reselectlane", new CheckBox("Reselect lane", false));
            Menu.Add("disablepings", new CheckBox("Disable pings", false));
            Menu.Add("disablechat", new CheckBox("Disable chat", false));
            var hpValue = Menu.Add("HPPot", new Slider("Minimum HP% to use Health Pot?", 43, 1, 100));
            HpPctUsePot = hpValue.CurrentValue;
            hpValue.OnValueChange +=
                delegate { HpPctUsePot = hpValue.CurrentValue; };
            Menu.AddSeparator(10);
            Menu.Add("sep2", new Separator(10));
            Menu.AddLabel("Champ will follow cursor. DON'T turn on if you are botting!");
            Menu.Add("disableAutoBuddy", new CheckBox("Disable AutoBuddy Movement. F5 to apply.", false));
            Menu.AddSeparator(5);
            var autoclose = new CheckBox("Auto close lol when the game ends. F5 to apply", false);
            Menu.Add("autoclose", autoclose);
            Menu.AddSeparator(5);
            Menu.Add("oldWalk", new CheckBox("Use old orbwalking. F5 to apply", false));
            Menu.Add("debuginfo", new CheckBox("Draw debug info. F5 to apply", true));
            Menu.Add("l1", new Label("By Christian Brutal Sniper - Maintained by TheYasuoMain"));
            Menu.Add("l2", new Label("Version: " + _version));
        }

        private static void SayHello()
        {
            Chat.Print("AutoBuddy:", Color.White);
            Chat.Print("Loaded Version: " + _version, Color.LimeGreen);
            Chat.Print("AutoBuddy: Starting in 5 seconds.");
        }

        // send to Kalista TODO
        /// <summary>The on_ update.</summary>
        /// <param name="args">The args.</param>
        private static void On_Update(EventArgs args)
        {
            if (BlackSpear.IsOwned())
            {
                foreach (var ally in EntityManager.Heroes.Allies)
                {
                    if (ally != null)
                    {
                        BlackSpear.Cast(ally);
                    }
                }
            }
        }

        /// <summary>The start.</summary>
        private static void Start()
        {
            Surrender.Start();
            RecallProtector.Start();
            AntiAFK.Start();
            RandGen.Start();

            #region Champ Logic

            var generic = true;
            /*switch (ObjectManager.Player.Hero)
            {
                case Champion.Ashe:
                    _champLogic = new Ashe();
                    break;
                case Champion.Caitlyn:
                    _champLogic = new Caitlyn();
                    break;
                default:
                    generic = true;
                    _champLogic = new Generic();
                    break;
                case Champion.Ezreal:
                    _champLogic = new Ezreal();
                    break;
                case Champion.Cassiopeia:
                    _champLogic = new Cassiopeia();
                    break;
                case Champion.Ryze:
                    _champLogic = new Ryze();
                    break;
                case Champion.Soraka:
                    _champLogic = new Soraka();
                    break;
                case Champion.Kayle:
                    _champLogic = new Kayle();
                    break;
                case Champion.Tristana:
                    _champLogic = new Tristana();
                    break;
                case Champion.Sivir:
                    _champLogic = new Sivir();
                    break;
                case Champion.Ahri:
                    _champLogic = new Ahri();
                    break;
                case Champion.Anivia:
                    _champLogic = new Anivia();
                    break;
                case Champion.Annie:
                    _champLogic = new Annie();
                    break;
                case Champion.Corki:
                    _champLogic = new Corki();
                    break;
                case Champion.Brand:
                    _champLogic = new Brand();
                    break;
                case Champion.Azir:
                    _champLogic = new Azir();
                    break;
                case Champion.Xerath:
                    _champLogic = new Xerath();
                    break;
                case Champion.Morgana:
                    _champLogic = new Morgana();
                    break;
                case Champion.Draven:
                    _champLogic = new Draven();
                    break;
                case Champion.Twitch:
                    _champLogic = new Twitch();
                    break;
                case Champion.Kalista:
                    _champLogic = new Kalista();
                    break;
                case Champion.Velkoz:
                    _champLogic = new Velkoz();
                    break;
                case Champion.Leblanc:
                    _champLogic = new Leblanc();
                    break;
                case Champion.Jinx:
                    _champLogic = new Jinx();
                    break;
                case Champion.Katarina:
                    _champLogic = new Katarina();
                    break;
                case Champion.Nidalee:
                    _champLogic = new Nidalee();
                    break;
            }*/

            #endregion

            /*var cl = new CustomLvlSeq(
                Menu,
                Walker.Hero,
                Path.Combine(SandboxConfig.DataDirectory, "AutoBuddy\\Skills"));*/
            /*if (!generic)
            {
                var bc = new BuildCreator(
                    Menu,
                    _champLogic.ShopSequence);
            }
            else
            {
                var bc = new BuildCreator(
                        Menu,
                        _champLogic.ShopSequence);
            }
            */
            LogicSelector.Initialize();
        }
    }
}