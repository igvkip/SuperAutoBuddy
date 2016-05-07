// <copyright file="NavGraphTester.cs" company="SuperAutoBuddy">
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

namespace SuperAutoBuddy.Utilities.Pathfinder
{
    using System;
    using System.Collections.Generic;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Rendering;

    using SharpDX;

    using Color = System.Drawing.Color;

    internal class NavGraphTester
    {
        private readonly NavGraph navGraph;

        private bool drawon;

        private List<Vector3> p;

        private int selectedNode = -1;

        public NavGraphTester(NavGraph n)
        {
            p = new List<Vector3>();
            navGraph = n;
            var menu = MainMenu.AddMenu("AB NavGraph", "abnavgraph");
            var addSelectNode = new KeyBind("Add/select node", false, KeyBind.BindTypes.HoldActive);
            addSelectNode.OnValueChange += addSelectNode_OnValueChange;
            var removeNode = new KeyBind("remove node", false, KeyBind.BindTypes.HoldActive);
            removeNode.OnValueChange += removeNode_OnValueChange;
            var addremoveneighbor = new KeyBind("Add/remove neighbor", false, KeyBind.BindTypes.HoldActive);
            addremoveneighbor.OnValueChange += addremoveneighbor_OnValueChange;

            menu.Add("addselsect", addSelectNode);
            menu.Add("removeno", removeNode);
            menu.Add("addneigh", addremoveneighbor);
            var zoom = new Slider("Zoom", 2250, 0, 5000);
            menu.Add("zoom", zoom);
            zoom.CurrentValue = (int) Camera.ZoomDistance;
            zoom.OnValueChange += zoom_OnValueChange;
            Chat.OnInput += Chat_OnInput;
        }

        private void addremoveneighbor_OnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (!args.NewValue || selectedNode < 0)
            {
                return;
            }
            var closestNodeId = navGraph.FindClosestNode(Game.CursorPos, selectedNode);
            if (navGraph.Nodes[closestNodeId].position.Distance(Game.CursorPos) > 300)
            {
                return;
            }
            if (navGraph.LinkExists(selectedNode, closestNodeId))
            {
                navGraph.RemoveLink(selectedNode, closestNodeId);
            }
            else
            {
                navGraph.AddLink(selectedNode, closestNodeId);
                selectedNode = closestNodeId;
            }
        }

        private void addSelectNode_OnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (!args.NewValue)
            {
                return;
            }
            var closestNodeId = navGraph.FindClosestNode(Game.CursorPos);
            if (closestNodeId == -1 || navGraph.Nodes[closestNodeId].position.Distance(Game.CursorPos) > 300)
            {
                navGraph.AddNode(Game.CursorPos);
                selectedNode = navGraph.Nodes.Length - 1;
            }
            else
            {
                selectedNode = closestNodeId;
            }
        }

        private void Chat_OnInput(ChatInputEventArgs args)
        {
            if (args.Input.Equals("/ng path"))
            {
                args.Process = false;
                p = navGraph.FindPath2(ObjectManager.Player.Position, Game.CursorPos);
            }
            if (args.Input.Equals("/ng walk"))
            {
                args.Process = false;
                p = navGraph.FindPath2(ObjectManager.Player.Position, Game.CursorPos);
                Game.OnTick += Game_OnTick;
            }
            if (args.Input.Equals("/ng save"))
            {
                args.Process = false;
            }
            if (args.Input.Equals("/ng load"))
            {
                args.Process = false;
                selectedNode = -1;
                p = new List<Vector3>();
                navGraph.Load();
            }
            if (args.Input.Equals("/ng clear"))
            {
                args.Process = false;
                navGraph.Nodes = new Node[0];
                p = new List<Vector3>();
                selectedNode = -1;
            }
            if (args.Input.Equals("/ng show"))
            {
                args.Process = false;
                if (drawon)
                {
                    return;
                }
                Drawing.OnDraw += Drawing_OnDraw;
                drawon = true;
            }
            if (args.Input.Equals("/ng hide"))
            {
                args.Process = false;
                Drawing.OnDraw -= Drawing_OnDraw;
                drawon = false;
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (selectedNode >= 0)
            {
                Circle.Draw(new ColorBGRA(255, 0, 0, 255), 100, navGraph.Nodes[selectedNode].position);
            }
            navGraph.Draw();

            for (var i = 0; i < p.Count - 1; i++)
            {
                if (p[i].IsOnScreen() || p[i + 1].IsOnScreen())
                {
                    Line.DrawLine(Color.Aqua, 4, p[i], p[i + 1]);
                }
            }
        }

        private void Game_OnTick(EventArgs args)
        {
            if (p.Count == 0)
            {
                Game.OnTick -= Game_OnTick;
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, p[0], true);
            if (ObjectManager.Player.Distance(p[0]) < 400)
            {
                p.RemoveAt(0);
            }
        }

        private void removeNode_OnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (!args.NewValue || selectedNode < 0)
            {
                return;
            }
            if (selectedNode >= 0)
            {
                navGraph.RemoveNode(selectedNode);
                selectedNode = navGraph.FindClosestNode(Game.CursorPos);
            }
        }

        private void zoom_OnValueChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            Camera.SetZoomDistance(args.NewValue);
        }
    }
}