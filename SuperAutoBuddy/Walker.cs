// <copyright file="Walker.cs" company="SuperAutoBuddy">
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
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Rendering;

    using SharpDX;

    using Utilities;
    using Utilities.Pathfinder;

    using Color = System.Drawing.Color;

    internal static class Walker
    {
        public static readonly Obj_AI_Turret EnemyFountain;
        public static readonly Obj_HQ EnemyNexus;
        public static readonly Obj_HQ AllyNexus;
        private static readonly ColorBGRA TargetColor;
        private static readonly NavGraph NavGraph;

        public static Spell.Active Ghost;
        public static Spell.Active Barrier;
        public static Spell.Active Heal;
        public static Spell.Active Recall;

        public static bool HasHeal;
        public static bool HasBarrier;
        public static bool HasGhost;
        
        private static Orbwalker.ActiveModes _activeMode = Orbwalker.ActiveModes.None;
        private static InventorySlot _potSlot;
        private static List<Vector3> _pfNodes;
        private static InventorySlot _seraphs;


        static Walker()
        {
            NavGraph = new NavGraph();
            _pfNodes = new List<Vector3>();
            TargetColor = new ColorBGRA(79, 219, 50, 255);
            AllyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsAlly);
            EnemyNexus = ObjectManager.Get<Obj_HQ>().First(n => n.IsEnemy);
            EnemyFountain =
                ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tur => !tur.IsAlly && tur.GetLane() == Lane.Spawn);
            InitSummonerSpells();

            Target = ObjectManager.Player.Position;
            Orbwalker.DisableMovement = false;

            Orbwalker.DisableAttacking = false;
            Game.OnUpdate += Game_OnUpdate;

            if (!MainMenu.GetMenu("AB").Get<CheckBox>("disableAutoBuddy").CurrentValue)
            {
                Orbwalker.OverrideOrbwalkPosition = () => Target;
            }

            if (Orbwalker.HoldRadius > 130 || Orbwalker.HoldRadius < 80)
            {
                Chat.Print("=================WARNING=================", Color.Red);
                Chat.Print("Your hold radius value in orbwalker isn't optimal for AutoBuddy", Color.Aqua);
                Chat.Print("Please set hold radius through menu=>Orbwalker");
                Chat.Print("Recommended values: Hold radius: 80-130, Delay between movements: 100-250");
            }

            if (MainMenu.GetMenu("AB").Get<CheckBox>("debuginfo").CurrentValue)
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }

            UpdateItems();
            Core.DelayAction(OnEndGame, 20000);
            Game.OnTick += OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static AIHeroClient Hero
        {
            get { return ObjectManager.Player; }
        }

        public static Vector3 Target { get; private set; }

        public static bool Recalling()
        {
            return ObjectManager.Player.IsRecalling();
        }

        public static void SetMode(Orbwalker.ActiveModes mode)
        {
            if (_activeMode != Orbwalker.ActiveModes.Combo)
            {
                Orbwalker.DisableAttacking = false;
            }
            _activeMode = mode;
        }

        public static void UseBarrier()
        {
            if (HasBarrier && Barrier.IsReady())
            {
                Barrier.Cast();
            }
        }

        public static void UseGhost()
        {
            if (HasGhost && Ghost.IsReady())
            {
                Ghost.Cast();
            }
        }

        public static void UseHeal()
        {
            if (HasHeal && Heal.IsReady())
            {
                Heal.Cast();
            }
        }

        public static void UsePot()
        {
            UpdateItems();
            if (!Player.HasBuff("RegenerationPotion"))
            {
                UpdateItems();
                if (_potSlot == null)
                {
                    return;
                }
                _potSlot.Cast();
                _potSlot = null;
            }
        }

        public static void UseSeraphs()
        {
            if (_seraphs != null && _seraphs.CanUseItem())
            {
                _seraphs.Cast();
            }
        }

        public static void WalkTo(Vector3 tgt)
        {
            if (_pfNodes.Any())
            {
                var dist = tgt.Distance(_pfNodes[_pfNodes.Count - 1]);
                if (dist > 900 || dist > 300 && ObjectManager.Player.Distance(tgt) < 2000)
                {
                    _pfNodes = NavGraph.FindPathRandom(ObjectManager.Player.Position, tgt);
                }
                else
                {
                    _pfNodes[_pfNodes.Count - 1] = tgt;
                }
                Target = _pfNodes[0];
            }
            else
            {
                if (tgt.Distance(ObjectManager.Player) > 900)
                {
                    _pfNodes = NavGraph.FindPathRandom(ObjectManager.Player.Position, tgt);
                    Target = _pfNodes[0];
                }
                else
                {
                    Target = tgt;
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Circle.Draw(TargetColor, 40, Target);
            for (var i = 0; i < _pfNodes.Count - 1; i++)
            {
                if (_pfNodes[i].IsOnScreen() || _pfNodes[i + 1].IsOnScreen())
                {
                    Line.DrawLine(Color.Aqua, 4, _pfNodes[i], _pfNodes[i + 1]);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_activeMode == Orbwalker.ActiveModes.LaneClear)
            {
                Orbwalker.ActiveModesFlags = ObjectManager.Player.TotalAttackDamage < 150
                                             && EntityManager.MinionsAndMonsters.EnemyMinions.Any(
                                                 en =>
                                                     en.Distance(ObjectManager.Player) <
                                                     ObjectManager.Player.AttackRange + en.BoundingRadius
                                                     && Prediction.Health.GetPrediction(en, 2000)
                                                     < ObjectManager.Player.GetAutoAttackDamage(en))
                    ? Orbwalker.ActiveModes.Harass
                    : Orbwalker.ActiveModes.LaneClear;
            }
            else
            {
                Orbwalker.ActiveModesFlags = _activeMode;
            }
        }

        private static void InitSummonerSpells()
        {
            Recall = new Spell.Active(SpellSlot.Recall);

            //InitHeal
            var heal = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("heal"));
            if (heal != null)
            {
                HasHeal = true;
                Heal = new Spell.Active(heal.Slot, 600);
            }
            //InitGhost
            var ghost = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("haste"));
            if (ghost != null)
            {
                HasGhost = true;
                Ghost = new Spell.Active(ghost.Slot);
            }
            //InitBarrier
            var barrier = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("barrier"));
            if (barrier != null)
            {
                HasBarrier = true;
                Barrier = new Spell.Active(barrier.Slot);
            }
        }

        private static void OnEndGame()
        {
            if (AllyNexus != null && EnemyNexus != null && (AllyNexus.Health > 1) && (EnemyNexus.Health > 1))
            {
                Core.DelayAction(OnEndGame, 5000);
                return;
            }

            if (MainMenu.GetMenu("AB").Get<CheckBox>("autoclose").CurrentValue)
            {
                Core.DelayAction(() => { Game.QuitGame(); }, 14000);
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (_pfNodes.Count != 0)
            {
                Target = _pfNodes[0];
                if (ObjectManager.Player.Distance(_pfNodes[0]) < 600)
                {
                    _pfNodes.RemoveAt(0);
                }
            }
        }

        private static void UpdateItems()
        {
            _potSlot = ObjectManager.Player.InventoryItems.FirstOrDefault(t => t.Id.IsHPotion());
            _seraphs = ObjectManager.Player.InventoryItems.FirstOrDefault(it => (int) it.Id == 3040);
            Core.DelayAction(UpdateItems, 5000);
        }
    }
}