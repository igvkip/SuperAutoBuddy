// <copyright file="Node.cs" company="SuperAutoBuddy">
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
    using System.IO;

    using EloBuddy.SDK;
    using EloBuddy.SDK.Rendering;

    using SharpDX;

    internal class Node
    {
        private readonly NavGraph navGraph;
        public readonly Vector3 position;

        public int[] Neighbors;

        public bool passable = true;

        public Node(int[] neighbors, Vector3 pos, NavGraph graph)
        {
            navGraph = graph;
            position = pos;
            Neighbors = neighbors;
        }

        public Node(Stream f, byte[] b, NavGraph graph)
        {
            navGraph = graph;
            f.Read(b, 0, 4);
            Neighbors = new int[BitConverter.ToInt32(b, 0)];
            for (var i = 0; i < Neighbors.Length; i++)
            {
                f.Read(b, 0, 4);
                Neighbors[i] = BitConverter.ToInt32(b, 0);
            }
            position = new Vector3();
            f.Read(b, 0, 4);
            position.X = BitConverter.ToSingle(b, 0);
            f.Read(b, 0, 4);
            position.Y = BitConverter.ToSingle(b, 0);
            f.Read(b, 0, 4);
            position.Z = BitConverter.ToSingle(b, 0);
        }

        public void AddNeighbor(int neighbor)
        {
            var tmp = new int[Neighbors.Length + 1];
            Neighbors.CopyTo(tmp, 0);
            Neighbors = tmp;
            Neighbors[Neighbors.Length - 1] = neighbor;
        }

        public void DrawLinks()
        {
            var onscreen = position.IsOnScreen();
            foreach (var i in Neighbors)
            {
                var n = navGraph.Nodes[i];
                if (onscreen || n.position.IsOnScreen())
                {
                    Line.DrawLine(navGraph.LineColor, 1f, position, n.position);
                }
            }
        }

        public void DrawPositions()
        {
            if (position.IsOnScreen())
            {
                Circle.Draw(navGraph.NodeColor, 40, 2f, position);
            }
        }

        public float GetDistance(int dest)
        {
            return position.Distance(navGraph.Nodes[dest].position);
        }

        public void RemoveNeighbor(int neighbor)
        {
            var tmp = new int[Neighbors.Length - 1];
            var index = 0;
            for (var i = 0; i < Neighbors.Length; i++)
            {
                if (Neighbors[i] != neighbor)
                {
                    continue;
                }
                index = i;
                break;
            }
            Array.Copy(Neighbors, 0, tmp, 0, index);
            Array.Copy(Neighbors, index + 1, tmp, index, Neighbors.Length - index - 1);
            Neighbors = tmp;
        }

        public void Serialize(FileStream f)
        {
            f.Write(BitConverter.GetBytes(Neighbors.Length), 0, 4);
            foreach (var neighbor in Neighbors)
            {
                f.Write(BitConverter.GetBytes(neighbor), 0, 4);
            }
            f.Write(BitConverter.GetBytes(position.X), 0, 4);
            f.Write(BitConverter.GetBytes(position.Y), 0, 4);
            f.Write(BitConverter.GetBytes(position.Z), 0, 4);
        }
    }
}