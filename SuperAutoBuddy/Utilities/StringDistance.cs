// <copyright file="StringDistance.cs" company="SuperAutoBuddy">
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

#region Header

// <copyright file="StringDistance.cs" company="AutoBuddy_BETA_Fixed">
//   Copyright (C) 2016 AutoBuddy_BETA_Fixed
// </copyright>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace SuperAutoBuddy.Utilities
{
    using System;
    using System.Text;

    /// <summary>The string distance.</summary>
    internal static class StringDistance
    {
        /// <summary>The default match score.</summary>
        private const double defaultMatchScore = 1.0;

        /// <summary>The default mismatch score.</summary>
        private const double defaultMismatchScore = 0.0;

        /// <summary>The match.</summary>
        /// <param name="s">The s.</param>
        /// <param name="t">The t.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public static double Match(this string s, string t)
        {
            return RateSimilarity(t, s);
        }

        /// <summary>The rate similarity.</summary>
        /// <param name="_firstWord">The _first word.</param>
        /// <param name="_secondWord">The _second word.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public static double RateSimilarity(string _firstWord, string _secondWord)
        {
            _firstWord = _firstWord.Replace("\'", string.Empty).Replace(" ", string.Empty).ToLower();
            _secondWord = _secondWord.Replace("\'", string.Empty).Replace(" ", string.Empty).ToLower();
            if (_firstWord == _secondWord)
            {
                return defaultMatchScore;
            }
            var halfLength = Math.Min(_firstWord.Length, _secondWord.Length)/2 + 1;

            var common1 = GetCommonCharacters(_firstWord, _secondWord, halfLength);
            var commonMatches = common1.Length;

            if (commonMatches == 0)
            {
                return defaultMismatchScore;
            }

            var common2 = GetCommonCharacters(_secondWord, _firstWord, halfLength);

            if (commonMatches != common2.Length)
            {
                return defaultMismatchScore;
            }
            var transpositions = 0;
            for (var i = 0; i < commonMatches; i++)
            {
                if (common1[i] != common2[i])
                {
                    transpositions++;
                }
            }

            transpositions /= 2;
            var jaroMetric = commonMatches/(3.0*_firstWord.Length) + commonMatches/(3.0*_secondWord.Length)
                             + (commonMatches - transpositions)/(3.0*commonMatches);
            return jaroMetric;
        }

        /// <summary>The get common characters.</summary>
        /// <param name="firstWord">The first word.</param>
        /// <param name="secondWord">The second word.</param>
        /// <param name="separationDistance">The separation distance.</param>
        /// <returns>The <see cref="StringBuilder"/>.</returns>
        private static StringBuilder GetCommonCharacters(string firstWord, string secondWord, int separationDistance)
        {
            if ((firstWord == null) || (secondWord == null))
            {
                return null;
            }
            var returnCommons = new StringBuilder(20);
            var copy = new StringBuilder(secondWord);
            var firstWordLength = firstWord.Length;
            var secondWordLength = secondWord.Length;

            for (var i = 0; i < firstWordLength; i++)
            {
                var character = firstWord[i];
                var found = false;

                for (var j = Math.Max(0, i - separationDistance);
                    !found && j < Math.Min(i + separationDistance, secondWordLength);
                    j++)
                {
                    if (copy[j] == character)
                    {
                        found = true;
                        returnCommons.Append(character);
                        copy[j] = '#';
                    }
                }
            }

            return returnCommons;
        }
    }
}