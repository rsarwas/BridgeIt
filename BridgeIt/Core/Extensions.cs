#region License and Terms
// Bridge-It
// Copyright (c) 2011-2012 Regan Sarwas. All rights reserved.
//
// Licensed under the GNU General Public License, Version 3.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.gnu.org/licenses/gpl-3.0.html
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace BridgeIt.Core
{
	public static class Extensions
	{
		public static bool IsMajor (this Suit suit)
		{
			return suit == Suit.Spades || suit == Suit.Hearts;
		}

		public static bool IsMinor (this Suit suit)
		{
			return suit == Suit.Diamonds || suit == Suit.Clubs;
		}

		public static bool IsNone (this Suit suit)
		{
			return suit == Suit.None;
		}
		
		public static string PrintFormat (this IEnumerable<Card> cards)
        {
            var sb = new StringBuilder();
            foreach (var suit in Deck.Suits)
            {
                sb.AppendLine();
                sb.Append(Card.SuitToGlyph(suit));
                foreach (var card in cards.Where(c => c.Suit == suit).OrderByDescending<Card,Rank>(c => c.Rank))
                    sb.Append(" " + Card.RankToString(card.Rank));
            }
            return sb.ToString();
        }

        public static Team GetTeam (this Seat seat)
        {
            switch (seat)
            {
                case Seat.None:
                    return Team.None;
                case Seat.North:
                case Seat.South:
                    return Team.NorthSouth;
                case Seat.East:
                case Seat.West:
                    return Team.WestEast;
                default:
                    throw new ArgumentException("Seat of '" + seat +"' not recognized.");
            }
        }
	}
}

