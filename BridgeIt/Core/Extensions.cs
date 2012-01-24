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
            if (cards == null)
                return "<null>";

            var sb = new StringBuilder();
            foreach (var suit in Deck.Suits)
            {
                sb.AppendLine();
                sb.Append(Card.SuitToGlyph(suit));
                //TODO - investigate resharper comments
                //suit: Access to modified closure; <Card,Rank> redundant; possible multiple enumerations of cards
                //var suit1 = suit
                //foreach (var card in cards.Where(c => c.Suit == suit1).OrderByDescending(c => c.Rank))
                foreach (var card in cards.Where(c => c.Suit == suit).OrderByDescending<Card, Rank>(c => c.Rank))
                        sb.Append(" " + Card.RankToString(card.Rank));
            }
            return sb.ToString();
        }

        public static Seat GetRightHandOpponent (this Seat seat)
        {
            //Seat 0 is reserved for the null seat
            seat--;
            if ((int)seat < 1)
                seat = (Seat)4;
            return seat;

        }

        public static Seat GetLeftHandOpponent (this Seat seat)
        {
            //Seat 0 is reserved for the null seat
            seat++;
            if ((int)seat > 4)
                seat = (Seat)1;
            return seat;
        }

        public static Seat GetNextSeat (this Seat seat)
        {
            return seat.GetLeftHandOpponent();
        }


        public static Side GetSide (this Seat seat)
        {
            switch (seat)
            {
                case Seat.None:
                    return Side.None;
                case Seat.North:
                case Seat.South:
                    return Side.NorthSouth;
                case Seat.East:
                case Seat.West:
                    return Side.WestEast;
                default:
                    throw new ArgumentException("Seat of '" + seat + "' not recognized.");
            }
        }

        /// <summary>
        /// Returns an enumeration of at most the last n items in the original order.  Return value may have less than n items.
        /// </summary>
        /// <remarks>
        /// Convert to a list instead of using items.Skip(items.Count() - n) which requires multiple enumerations, and checks for Count() less than n
        /// Converting to a list also guarantees a copy.
        /// Plus internally we can take advantage of the underlying list to speed up calling methods
        /// </remarks>
        /// <value>
        /// The last three items.
        /// </value>
        public static IEnumerable<TSource> Last<TSource> (this IEnumerable<TSource> items, int n)
        {
            var itemList = items.ToList();
            return (itemList.Count <= n) ? itemList : itemList.GetRange(itemList.Count - n, n);
        }

        public static bool VoidOfSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return !cards.Any(c => c.Suit == suit);
        }

        public static IEnumerable<Card> GetSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.Where(c => c.Suit == suit);
        }

        public static Card GetHighestInSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.GetSuit(suit).OrderByDescending(c => (int)c.Rank).First();
        }


	}
}

