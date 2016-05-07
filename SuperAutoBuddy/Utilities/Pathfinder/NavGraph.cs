// <copyright file="NavGraph.cs" company="SuperAutoBuddy">
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
    using System.IO;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;

    using SharpDX;

    using Properties;

    using Color = System.Drawing.Color;

    internal class NavGraph
    {
        private readonly Vector3[] _baseGates = new Vector3[2];

        public readonly Color LineColor;

        public readonly ColorBGRA NodeColor;

        public Node[] Nodes;

        private NavGraphTester _tester;

        public NavGraph()
        {
            if (ObjectManager.Player.Team == GameObjectTeam.Chaos)
            {
                _baseGates[0] = new Vector3(2685, 4784, 75);
                _baseGates[1] = new Vector3(4813, 2806, 79);
            }
            else
            {
                _baseGates[0] = new Vector3(10018, 12144, 75);
                _baseGates[1] = new Vector3(12078, 10104, 79);
            }

            NodeColor = new ColorBGRA(50, 200, 0, 255);
            LineColor = Color.Gold;
            Load();
            Chat.OnInput += Chat_OnInput;
        }

        public void AddLink(int node1, int node2)
        {
            Nodes[node1].AddNeighbor(node2);
            Nodes[node2].AddNeighbor(node1);
        }

        public void AddNode(Vector3 pos)
        {
            var tmp = new Node[Nodes.Length + 1];
            Nodes.CopyTo(tmp, 0);
            Nodes = tmp;
            Nodes[Nodes.Length - 1] = new Node(new int[0], pos, this);
        }

        public void Draw()
        {
            for (var i = 0; i < Nodes.Length; i++)
            {
                Nodes[i].DrawPositions();
                //if(Nodes[i].position.IsOnScreen())
                // Drawing.DrawText(Nodes[i].position.WorldToScreen(), Color.Gold, "      " + i + "    " + Nodes[i].Neighbors.Length, 10);
            }
            for (var i = 0; i < Nodes.Length; i++)
            {
                Nodes[i].DrawLinks();
            }
        }

        public int FindClosestNode(Vector3 pos)
        {
            var minDist = float.MaxValue;
            var node = -1;
            for (var i = 0; i < Nodes.Length; i++)
            {
                if (!(pos.Distance(Nodes[i].position) < minDist))
                {
                    continue;
                }
                minDist = pos.Distance(Nodes[i].position);
                node = i;
            }
            return node;
        }

        private int FindClosestNode(Vector3 pos, Vector3 end)
        {
            var minDist = float.MaxValue;
            var node = -1;
            for (var i = 0; i < Nodes.Length; i++)
            {
                if (!(pos.Distance(Nodes[i].position) < minDist)
                    || end.Distance(Nodes[i].position) > end.Distance(ObjectManager.Player))
                {
                    continue;
                }
                minDist = pos.Distance(Nodes[i].position);
                node = i;
            }
            return node;
        }

        public int FindClosestNode(Vector3 pos, int except)
        {
            var minDist = float.MaxValue;
            var node = -1;
            for (var i = 0; i < Nodes.Length; i++)
            {
                if (except == i || !(pos.Distance(Nodes[i].position) < minDist || !Nodes[i].passable))
                {
                    continue;
                }
                minDist = pos.Distance(Nodes[i].position);
                node = i;
            }
            return node;
        }

        public List<Vector3> FindPath2(Vector3 start, Vector3 end)
        {
            var p = FindPath(FindClosestNode(start, end), FindClosestNode(end));
            var ret = new List<Vector3> {end};
            while (p != null)
            {
                ret.Add(Nodes[p.node].position);
                p = p.Parent;
            }
            ret.Reverse();
            return ret;
        }

        public List<Vector3> FindPathRandom(Vector3 start, Vector3 end)
        {
            var p = FindPath(FindClosestNode(start, end), FindClosestNode(end));
            var ret = new List<Vector3> {end};
            while (p != null)
            {
                ret.Add(Nodes[p.node].position.Randomized(-15f, 15f));
                p = p.Parent;
            }
            ret.Reverse();
            return ret;
        }

        public bool LinkExists(int node1, int node2)
        {
            return Nodes[node1].Neighbors.Contains(node2);
        }

        public void Load()
        {
            var buffer = new byte[4];
            using (var f = new MemoryStream(Resources.NavGraphSummonersRift))
            {
                f.Read(buffer, 0, 4);
                Nodes = new Node[BitConverter.ToInt32(buffer, 0)];
                for (var i = 0; i < Nodes.Length; i++)
                {
                    Nodes[i] = new Node(f, buffer, this);
                    Nodes[i].passable = Nodes[i].position.Distance(_baseGates[0]) > 200
                                        && Nodes[i].position.Distance(_baseGates[1]) > 200;
                }
            }
        }

        public void RemoveLink(int node1, int node2)
        {
            Nodes[node1].RemoveNeighbor(node2);
            Nodes[node2].RemoveNeighbor(node1);
        }

        public void RemoveNode(int node)
        {
            while (Nodes[node].Neighbors.Length > 0)
            {
                RemoveLink(node, Nodes[node].Neighbors[0]);
            }
            var tmp = new Node[Nodes.Length - 1];
            Array.Copy(Nodes, 0, tmp, 0, node);
            Array.Copy(Nodes, node + 1, tmp, node, Nodes.Length - node - 1);
            Nodes = tmp;
            for (var i = 0; i < Nodes.Length; i++)
            {
                for (var j = 0; j < Nodes[i].Neighbors.Length; j++)
                {
                    if (Nodes[i].Neighbors[j] > node)
                    {
                        Nodes[i].Neighbors[j]--;
                    }
                }
            }
        }

        private void Chat_OnInput(ChatInputEventArgs args)
        {
            if (_tester != null || !args.Input.Equals("/navgraph"))
            {
                return;
            }
            args.Process = false;
            _tester = new NavGraphTester(this);
        }

        private float Distance(PathNode p1, int p2)
        {
            return Nodes[p1.node].position.Distance(Nodes[p2].position);
        }

        private float Distance(int p1, int p2)
        {
            return Nodes[p1].position.Distance(Nodes[p2].position);
        }

        private PathNode FindPath(int startNode, int endNode)
        {
            if (startNode == -1 || endNode == -1)
            {
                return null;
            }
            var open = new List<PathNode>();
            var closed = new List<PathNode>();
            open.Add(new PathNode(0, Nodes[startNode].GetDistance(endNode), startNode, null));

            while (open.Count > 0)
            {
                var q = open.OrderBy(n => n.fCost).First();
                open.Remove(q);
                foreach (var neighbor in Nodes[q.node].Neighbors)
                {
                    if (!Nodes[neighbor].passable)
                    {
                        continue;
                    }
                    var s =
                        new PathNode(
                            q.gCost
                            + Distance(q, neighbor)
                            *(Nodes[neighbor].position.GetNearestTurret().Distance(Nodes[neighbor].position) < 900
                                ? 20
                                : 1),
                            Distance(endNode, neighbor),
                            neighbor,
                            q);
                    if (neighbor == endNode)
                    {
                        return s;
                    }
                    var sameNodeOpen = open.FirstOrDefault(el => el.node == neighbor);
                    if (sameNodeOpen != null)
                    {
                        if (sameNodeOpen.fCost < s.fCost)
                        {
                            continue;
                        }
                    }
                    var sameNodeClosed = closed.FirstOrDefault(el => el.node == neighbor);
                    if (sameNodeClosed != null)
                    {
                        if (sameNodeClosed.fCost < s.fCost)
                        {
                            continue;
                        }
                    }
                    open.Add(s);
                }
                closed.Add(q);
            }
            return null;
        }
    }
}