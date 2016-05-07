// <copyright file="ShopItem.cs" company="SuperAutoBuddy">
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
    using System.Linq;

    using EloBuddy.SDK;

    using Newtonsoft.Json.Linq;

    using Properties;

    public class ShopItem
    {
        private static readonly Dictionary<int, ShopItem> CachedItems = new Dictionary<int, ShopItem>();
        private static readonly List<ShopItem> ShopItems = new List<ShopItem>();
        private static readonly JObject JsonData;

        static ShopItem()
        {
            var json = JObject.Parse(Resources.item);
            JsonData = (JObject) json.GetValue("data");

            foreach (var itemData in JsonData)
            {
                LoadItem(int.Parse(itemData.Key));
            }

            CachedItems.Clear();
            CachedItems = null;
        }

        public int Id { get; private set; }
        public int BaseCost { get; private set; }
        public int TotalCost { get; private set; }
        public string Name { get; private set; }
        private List<string> Aka { get; set; }
        public List<ShopItem> BuildsFrom { get; private set; }

        public int MissingCost
        {
            get
            {
                var missing = GetMissingComponents().Sum(i => i.BaseCost);
                return missing + BaseCost;
            }
        }

        public static ShopItem GetFromDb(string nameOrColloq)
        {
            var aka = ShopItems.FirstOrDefault(i => i.Aka.Contains(nameOrColloq));
            

            if (aka != null)
            {
                return aka;
            }

            return ShopItems.FirstOrDefault(i => i.Name.ToLower().Contains(nameOrColloq.ToLower()));
        }

        private static ShopItem LoadItem(int key)
        {
            if (CachedItems.ContainsKey(key))
            {
                return CachedItems[key];
            }

            var itemData = JsonData.GetValue(key.ToString());

            if ((bool) itemData["maps"]["11"] && (bool) itemData["gold"]["purchasable"])
            {
                var item = new ShopItem
                {
                    Name = itemData["name"].ToString(),
                    Id = key,
                    BuildsFrom = new List<ShopItem>(),
                    BaseCost = (int) itemData["gold"]["base"],
                    TotalCost = (int) itemData["gold"]["total"],
                    Aka = new List<string>()
                };

                var invalidItem = false;

                if (itemData["from"] != null)
                {
                    foreach (var from in itemData["from"].Select(s => LoadItem((int) s)))
                    {
                        // item is built from an invalid item (Muramana CS)
                        if (from == null)
                        {
                            invalidItem = true;
                        }
                        item.BuildsFrom.Add(from);
                    }
                }

                if (itemData["colloq"] != null)
                {
                    item.Aka.AddRange(itemData["colloq"].ToString().Split(';').Where(c => !string.IsNullOrEmpty(c)));
                }

                if (invalidItem)
                {
                    return null;
                }

                ShopItems.Add(item);
                CachedItems.Add(key, item);

                return item;
            }

            return null;
        }

        public List<ShopItem> GetComponents()
        {
            var components = new List<ShopItem>();
            foreach (var @base in BuildsFrom.OrderByDescending(i => i.TotalCost))
            {
                components.AddRange(@base.GetComponents());
                components.Add(@base);
            }

            return components;
        }

        public List<ShopItem> GetMissingComponents()
        {
            var components = new List<ShopItem>();
            foreach (var @base in BuildsFrom.Where(i => !Item.HasItem(i.Id)).OrderByDescending(i => i.TotalCost))
            {
                components.AddRange(@base.GetComponents());
                components.Add(@base);
            }

            return components;
        }


        public override string ToString()
        {
            return Name;
        }

        public static void Initialize()
        {
            //
        }
    }
}