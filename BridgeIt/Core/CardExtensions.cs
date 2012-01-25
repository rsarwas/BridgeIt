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
    public static class CardExtensions
    {

        public static IEnumerable<Card> GetSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.Where(c => c.Suit == suit);
        }


        public static Card GetHighestInSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.GetSuit(suit).OrderByDescending(c => (int)c.Rank).First();
        }

        public static bool VoidOfSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return !cards.Any(c => c.Suit == suit);
        }

        public static int HighCardPoints (this IEnumerable<Card> cards)
        {
            int total = 0;
            return cards.Aggregate(total, (t,c) => t + c.Points());
        }


        public static int HighCardPoints (this IEnumerable<Card> cards, Suit suit)
        {
            int total = 0;
            return cards.GetSuit(suit).Aggregate(total, (t,c) => t + c.Points());
        }


        public static int Points (this Card card)
        {
            int rank = (int)card.Rank; //two = 0 ... Ace = 12
            return (rank > 8) ? rank - 8 : 0;

        }


        public static Suit LongestSuit (this IEnumerable<Card> cards)
        {
            int longest = 0;
            Suit longSuit = Suit.None;
            foreach (Suit suit in Deck.Suits)
            {
                int len = cards.LengthOfSuit(suit);
                if (longest < len)
                {
                    longest = len;
                    longSuit = suit;
                }
            }
            return longSuit;
        }


        public static int LengthOfSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.Count(c => c.Suit == suit);
        }


        public static IEnumerable<Card> RankOrderSuit (this IEnumerable<Card> cards, Suit suit)
        {
            return cards.GetSuit(suit).OrderByDescending<Card, Rank>(c => c.Rank);
        }

        public static Card HighestInSuit (this IEnumerable<Card> cards, Suit suit)
        {
            //FIXME - Will throw if there are no cards in suit
            return cards.RankOrderSuit(suit).First();
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
                foreach (var card in cards.RankOrderSuit(suit))
                    sb.Append(" " + Card.RankToString(card.Rank));
            }
            return sb.ToString();
        }


    }
}

