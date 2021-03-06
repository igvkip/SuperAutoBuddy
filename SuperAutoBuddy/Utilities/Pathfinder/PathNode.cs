﻿// <copyright file="PathNode.cs" company="SuperAutoBuddy">
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
    internal class PathNode
    {
        public double gCost;

        public double hCost;

        public int node;

        public PathNode Parent;

        public PathNode(double gCost, double hCost, int node, PathNode parent)
        {
            Parent = parent;
            this.node = node;
            this.gCost = gCost;
            this.hCost = hCost;
        }

        public double fCost
        {
            get { return gCost + hCost; }
        }
    }
}