#region License and Terms
// GnuBridge (C#)
// Copyright (c) 2011-2012 Regan Sarwas. All rights reserved.
//
// Modeled after GNUBridge 0.1.19 (java) by Paul Slusarz (http://gnubridge.org)
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

using BridgeIt.Core;
using System;
using NUnit.Framework;

namespace Tests.Core
{
	[TestFixture()]
	public class CardTest
	{
		[Test()]
		public void TestCase1()
		{
			Card card1 = new Card (Rank.Ace, Suit.Spades);
			Card card2 = new Card (Rank.Eight, Suit.Spades);
			Card card3 = new Card (Rank.Ace, Suit.Hearts);
			Card card4 = new Card (Rank.Ace, Suit.Spades);

			Assert.AreNotEqual (card1, card2);
			Assert.AreNotEqual (card1, card3);
			Assert.AreEqual (card1, card4);
		}
        [Test()]
        public void HighPointTests ()
        {
            Card ace = new Card(Rank.Ace, Suit.Spades);
            Card king = new Card(Rank.King, Suit.Hearts);
            Card queen = new Card(Rank.Queen, Suit.Spades);
            Card jack = new Card(Rank.Jack, Suit.Hearts);
            Card ten = new Card(Rank.Ten, Suit.Spades);
            Card nine = new Card(Rank.Nine, Suit.Hearts);
            Card[] cards = new Card[]{ace, king, queen, jack, ten, nine};
            Assert.AreEqual(cards.HighCardPoints(), 10);
            Assert.AreEqual(cards.HighCardPoints(Suit.Hearts), 4);
            Assert.AreEqual(cards.HighCardPoints(Suit.Spades), 6);
            Assert.AreEqual(cards.HighCardPoints(Suit.Clubs), 0);
            Assert.AreEqual((new Card[]{}).HighCardPoints(), 0);
            Assert.AreEqual(cards.HighestInSuit(Suit.Hearts).Rank, Rank.King);
            Assert.AreEqual(cards.HighestInSuit(Suit.Spades).Rank, Rank.Ace);
            Assert.AreEqual(cards.LongestSuit(), Suit.Spades);
        }

	}
}

