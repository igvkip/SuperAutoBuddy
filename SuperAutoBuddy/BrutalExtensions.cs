// <copyright file="BrutalExtensions.cs" company="SuperAutoBuddy">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;

    using EloBuddy;
    using EloBuddy.SDK;

    using Humanizers;

    using SharpDX;

    using Utilities;

    /// <summary>The brutal extensions.</summary>
    internal static class BrutalExtensions
    {
        /// <summary>The all indexes of.</summary>
        /// <param name="str">The str.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="List"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("the string to find may not be empty", "value");
            }
            var indexes = new List<int>();
            for (var index = 0;; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                {
                    return indexes;
                }
                indexes.Add(index);
            }
        }

        /// <summary>The away.</summary>
        /// <param name="myPos">The my pos.</param>
        /// <param name="threatPos">The threat pos.</param>
        /// <param name="range">The range.</param>
        /// <param name="add">The add.</param>
        /// <param name="resolution">The resolution.</param>
        /// <returns>The <see cref="Vector3"/>.</returns>
        public static Vector3 Away(
            this Vector3 myPos,
            Vector3 threatPos,
            float range,
            float add = 200,
            float resolution = 40)
        {
            var r = threatPos.Extend(myPos, range).To3D();
            var re = threatPos.Extend(myPos, range + add).To3D();
            if (!NavMesh.GetCollisionFlags(re).HasFlag(CollisionFlags.Wall))
            {
                return r;
            }
            for (var i = 1; i < resolution; i++)
            {
                if (
                    !NavMesh.GetCollisionFlags(re.RotatedAround(threatPos, 3.14f/resolution*i))
                        .HasFlag(CollisionFlags.Wall))
                {
                    return r.RotatedAround(threatPos, 3.14f/resolution*i);
                }
                if (
                    !NavMesh.GetCollisionFlags(re.RotatedAround(threatPos, 3.14f/resolution*i*-1f))
                        .HasFlag(CollisionFlags.Wall))
                {
                    return r.RotatedAround(threatPos, 3.14f/resolution*i*-1f);
                }
            }

            return r;
        }

        /// <summary>The concatenate.</summary>
        /// <param name="source">The source.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="string"/>.</returns>
        public static string Concatenate<T>(this IEnumerable<T> source, string delimiter)
        {
            var s = new StringBuilder();
            var first = true;
            foreach (var t in source)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    s.Append(delimiter);
                }

                s.Append(t);
            }

            return s.ToString();
        }

        /// <summary>The copy.</summary>
        /// <param name="from">The from.</param>
        /// <returns>The <see cref="Vector3"/>.</returns>
        public static Vector3 Copy(this Vector3 from)
        {
            return new Vector3(from.X, from.Y, from.Z);
        }

        /// <summary>The copy.</summary>
        /// <param name="from">The from.</param>
        /// <returns>The <see cref="Vector3[]"/>.</returns>
        public static Vector3[] Copy(this Vector3[] from)
        {
            var ar = new Vector3[from.Length];
            for (var i = 0; i < ar.Length; i++)
            {
                ar[i] = from[i].Copy();
            }

            return ar;
        }

        /// <summary>The get dmg.</summary>
        /// <param name="slot">The slot.</param>
        /// <returns>The <see cref="float"/>.</returns>
        public static float GetDmg(this SpellSlot slot)
        {
            return 1;
        }

        // This was causing AutoBuddy to not load in Medium bot games :/
        // public static string GetGameType()
        // {

        // if (EntityManager.Heroes.Allies.Count < 5 || EntityManager.Heroes.Allies.Count(en=>en.Name.EndsWith(" Bot"))>1)
        // return "custom";

        // if (EntityManager.Heroes.Enemies.All(en => en.Name.EndsWith(" Bot")))
        // {
        // return EntityManager.Heroes.Enemies.All(en =>en.SkinId==0) ? "bot_easy" : "bot_intermediate";
        // }
        // return "normal";
        // }

        /// <summary>The get game type.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetGameType()
        {
            return "custom";
        }

        /// <summary>The get item slot.</summary>
        /// <param name="it">The it.</param>
        /// <returns>The <see cref="int"/>.</returns>
        /*public static int GetItemSlot(this LoLItem it)
        {
            //BrutalItemInfo.GetItemSlot(it.Id);
            return -1;
        }*/

        /// <summary>The get lane.</summary>
        /// <param name="min">The min.</param>
        /// <returns>The <see cref="Lane"/>.</returns>
        public static Lane GetLane(this Obj_AI_Minion min)
        {
            try
            {
                if (min.Name == null || min.Name.Length < 13)
                {
                    return Lane.Unknown;
                }
                if (min.Name[12] == '0')
                {
                    return Lane.Bot;
                }
                if (min.Name[12] == '1')
                {
                    return Lane.Mid;
                }
                if (min.Name[12] == '2')
                {
                    return Lane.Top;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetLane:" + e.Message);
            }

            return Lane.Unknown;
        }

        /// <summary>The get lane.</summary>
        /// <param name="tur">The tur.</param>
        /// <returns>The <see cref="Lane"/>.</returns>
        public static Lane GetLane(this Obj_AI_Turret tur)
        {
            if (tur.Name.EndsWith("Shrine_A"))
            {
                return Lane.Spawn;
            }
            if (tur.Name.EndsWith("C_02_A") || tur.Name.EndsWith("C_01_A"))
            {
                return Lane.HQ;
            }
            if (tur.Name == null || tur.Name.Length < 12)
            {
                return Lane.Unknown;
            }
            if (tur.Name[10] == 'R')
            {
                return Lane.Bot;
            }
            if (tur.Name[10] == 'C')
            {
                return Lane.Mid;
            }
            if (tur.Name[10] == 'L')
            {
                return Lane.Top;
            }
            return Lane.Unknown;
        }

        /// <summary>The get nearest turret.</summary>
        /// <param name="pos">The pos.</param>
        /// <param name="enemy">The enemy.</param>
        /// <returns>The <see cref="Obj_AI_Turret"/>.</returns>
        public static Obj_AI_Turret GetNearestTurret(this Vector3 pos, bool enemy = true)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.Health > 0 && tur.IsAlly ^ enemy)
                    .OrderBy(tur => tur.Distance(pos))
                    .First();
        }

        /// <summary>The get nearest turret.</summary>
        /// <param name="unit">The unit.</param>
        /// <param name="enemy">The enemy.</param>
        /// <returns>The <see cref="Obj_AI_Turret"/>.</returns>
        public static Obj_AI_Turret GetNearestTurret(this Obj_AI_Base unit, bool enemy = true)
        {
            return unit.Position.GetNearestTurret(enemy);
        }

        /// <summary>The get response text.</summary>
        /// <param name="address">The address.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetResponseText(this string address)
        {
            var request = (HttpWebRequest) WebRequest.Create(address);
            request.Proxy = null;
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var encoding = Encoding.GetEncoding(response.CharacterSet);

                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream, encoding)) return reader.ReadToEnd();
            }
        }

        /// <summary>The get wave.</summary>
        /// <param name="min">The min.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public static int GetWave(this Obj_AI_Minion min)
        {
            if (min.Name == null || min.Name.Length < 17)
            {
                return 0;
            }
            int result;
            try
            {
                result = int.Parse(min.Name.Substring(14, 2));
            }
            catch (FormatException)
            {
                result = 0;
                Console.WriteLine("GetWave error, minion name: " + min.Name);
            }

            return result;
        }

        /// <summary>The health percent.</summary>
        /// <param name="unit">The unit.</param>
        /// <returns>The <see cref="float"/>.</returns>
        public static float HealthPercent(this Obj_AI_Base unit)
        {
            return unit.Health/unit.MaxHealth*100f;
        }

        /// <summary>The is dead.</summary>
        /// <param name="unit">The unit.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsDead(this Obj_AI_Base unit)
        {
            return unit.Health <= 0;
        }

        /// <summary>The is healthly consumable.</summary>
        /// <param name="i">The i.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsHealthlyConsumable(this ItemId i)
        {
            return (int) i == 2003 || (int) i == 2009 || (int) i == 2010;
        }

        /// <summary>The is h potion.</summary>
        /// <param name="i">The i.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsHPotion(this ItemId i)
        {
            return (int) i == 2003 || (int) i == 2009 || (int) i == 2010 || (int) i == 2031;
        }

        /// <summary>The is visible.</summary>
        /// <param name="unit">The unit.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool IsVisible(this Obj_AI_Base unit)
        {
            return !unit.IsDead() && unit.IsHPBarRendered;
        }

        /// <summary>The post.</summary>
        /// <param name="address">The address.</param>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string Post(this string address, Dictionary<string, string> data)
        {
            var request = (HttpWebRequest) WebRequest.Create(address);
            request.Method = "POST";
            request.Proxy = null;
            request.ContentType = "application/x-www-form-urlencoded";
            var postData = data.Aggregate(
                string.Empty,
                (current, pair) => current + pair.Key + "=" + pair.Value.ToBase64URL() + "&");
            postData = postData.Substring(0, postData.Length - 1);

            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var encoding = Encoding.GetEncoding(response.CharacterSet);

                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream, encoding)) return reader.ReadToEnd();
            }
        }

        /// <summary>The randomized.</summary>
        /// <param name="vec">The vec.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>The <see cref="Vector3"/>.</returns>
        public static Vector3 Randomized(this Vector3 vec, float min = -300, float max = 300)
        {
            return new Vector3(vec.X + RandGen.Randomizer.NextFloat(min, max), vec.Y + RandGen.Randomizer.NextFloat(min, max), vec.Z);
        }

        /// <summary>The rotated around.</summary>
        /// <param name="rotated">The rotated.</param>
        /// <param name="around">The around.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The <see cref="Vector3"/>.</returns>
        public static Vector3 RotatedAround(this Vector3 rotated, Vector3 around, float angle)
        {
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);

            var ret = new Vector2(rotated.X - around.X, rotated.Y - around.Y);

            var xnew = ret.X*c - ret.Y*s;
            var ynew = ret.X*s + ret.Y*c;

            ret.X = (float) xnew + around.X;
            ret.Y = (float) ynew + around.Y;

            return ret.To3DWorld();
        }

        /// <summary>The to base 64 url.</summary>
        /// <param name="toEncode">The to encode.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string ToBase64URL(this string toEncode)
        {
            var toEncodeAsBytes = Encoding.Default.GetBytes(toEncode);
            var returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return HttpUtility.UrlEncode(returnValue);
        }
    }
}