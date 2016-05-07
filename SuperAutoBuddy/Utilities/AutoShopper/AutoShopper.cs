// <copyright file="AutoShopper.cs" company="SuperAutoBuddy">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Utils;

    using Humanizers;

    internal static class AutoShopper
    {
        private static readonly Queue<ShopItem> Build = new Queue<ShopItem>();

        static AutoShopper()
        {
            if (Game.Time/60 <= 1)
            {
                BuyStartingItems();
            }

            ShopItem.Initialize();

            BuildLoader.LoadBuild().ForEach(Enqueue);
            Shop();
        }

        public static ShopItem Next
        {
            get
            {
                if (IsFullBuild())
                {
                    return null;
                }

                return Build.Peek();
            }
        }

        public static int MissingGold
        {
            get
            {
                if (IsFullBuild())
                {
                    return 0;
                }

                return Build.Peek().MissingCost;
            }
        }

        private static int CurrentGold
        {
            get { return (int) ObjectManager.Player.Gold; }
        }

        private static void BuyStartingItems()
        {
        }

        private static void Enqueue(ShopItem buildItem)
        {
            if (Item.HasItem(buildItem.Id))
            {
                return;
            }

            foreach (var component in buildItem.BuildsFrom)
            {
                Enqueue(component);
            }

            Logger.Debug("Enqueued item {0}", buildItem);
            Build.Enqueue(buildItem);
        }

        public static void Initialize()
        {
        }

        public static bool HasEnoughGold()
        {
            if (IsFullBuild())
            {
                return false;
            }

            return CurrentGold > 800 + ObjectManager.Player.Level * 100;
        }

        private static void Shop()
        {
            if (IsFullBuild())
            {
                return;
            }

            if (!EloBuddy.Shop.CanShop)
            {
                Core.DelayAction(Shop, RandGen.Randomizer.Next(2000, 3000));
                return;
            }

            var buy = Build.Peek();

            if (Item.HasItem(buy.Id))
            {
                Build.Dequeue();
                Core.DelayAction(Shop, RandGen.Randomizer.Next(739, 1301));
                return;
            }

            if (buy.MissingCost <= CurrentGold)
            {
                Logger.Debug("Buying {0}", buy);
                if (EloBuddy.Shop.BuyItem(buy.Id))
                {
                    Build.Dequeue();
                }
            }
            else
            {
                var cheapest = buy.GetAffordableComponent();
                if (cheapest != null)
                {
                    EloBuddy.Shop.BuyItem(cheapest.Id);
                }
            }

            Core.DelayAction(Shop, RandGen.Randomizer.Next(739, 1301));
        }

        public static bool IsFullBuild()
        {
            return Build.Count == 0;
        }

        private static ShopItem GetAffordableComponent(this ShopItem biggerItem)
        {
            // try to finish one of its 1st grade components
            // eg Triforce would try to finish zeal/sheen/phage, if any of them have
            // at least 1 of its components

            return
                biggerItem.GetMissingComponents()
                    .Where(i => !Item.HasItem(i.Id))
                    .FirstOrDefault(i => i.MissingCost <= CurrentGold);
        }
    }
}