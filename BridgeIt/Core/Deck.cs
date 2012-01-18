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
using System.Collections.Generic;

/*
 Deck is a deck of cards
 It starts with 52 cards in order
 It should be shuffled before use
 */

namespace BridgeIt.Core
{
	public class Deck
	{
		
		#region Useful Constants
		
		public const int Size = 52;
		
		public static readonly IEnumerable<Suit> Suits = new[]
			{ Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs};
		
		public static readonly IEnumerable<Rank> Ranks = new[]
			{ Rank.Ace, Rank.King, Rank.Queen, Rank.Jack,
		      Rank.Ten, Rank.Nine, Rank.Eight, Rank.Seven,
			  Rank.Six, Rank.Five, Rank.Four, Rank.Three, Rank.Two };

		public static readonly Card TwoOfSpades = new Card (Rank.Two, Suit.Spades);
		public static readonly Card ThreeOfSpades = new Card (Rank.Three, Suit.Spades);
		public static readonly Card FourOfSpades = new Card (Rank.Four, Suit.Spades);
		public static readonly Card FiveOfSpades = new Card (Rank.Five, Suit.Spades);
		public static readonly Card SixOfSpades = new Card (Rank.Six, Suit.Spades);
		public static readonly Card SevenOfSpades = new Card (Rank.Seven, Suit.Spades);
		public static readonly Card EightOfSpades = new Card (Rank.Eight, Suit.Spades);
		public static readonly Card NineOfSpades = new Card (Rank.Nine, Suit.Spades);
		public static readonly Card TenOfSpades = new Card (Rank.Ten, Suit.Spades);
		public static readonly Card JackOfSpades = new Card (Rank.Jack, Suit.Spades);
		public static readonly Card QueenOfSpades = new Card (Rank.Queen, Suit.Spades);
		public static readonly Card KingOfSpades = new Card (Rank.King, Suit.Spades);
		public static readonly Card AceOfSpades = new Card (Rank.Ace, Suit.Spades);

		public static readonly Card TwoOfHearts = new Card (Rank.Two, Suit.Hearts);
		public static readonly Card ThreeOfHearts = new Card (Rank.Three, Suit.Hearts);
		public static readonly Card FourOfHearts = new Card (Rank.Four, Suit.Hearts);
		public static readonly Card FiveOfHearts = new Card (Rank.Five, Suit.Hearts);
		public static readonly Card SixOfHearts = new Card (Rank.Six, Suit.Hearts);
		public static readonly Card SevenOfHearts = new Card (Rank.Seven, Suit.Hearts);
		public static readonly Card EightOfHearts = new Card (Rank.Eight, Suit.Hearts);
		public static readonly Card NineOfHearts = new Card (Rank.Nine, Suit.Hearts);
		public static readonly Card TenOfHearts = new Card (Rank.Ten, Suit.Hearts);
		public static readonly Card JackOfHearts = new Card (Rank.Jack, Suit.Hearts);
		public static readonly Card QueenOfHearts = new Card (Rank.Queen, Suit.Hearts);
		public static readonly Card KingOfHearts = new Card (Rank.King, Suit.Hearts);
		public static readonly Card AceOfHearts = new Card (Rank.Ace, Suit.Hearts);
		
		public static readonly Card TwoOfDiamonds = new Card (Rank.Two, Suit.Diamonds);
		public static readonly Card ThreeOfDiamonds = new Card(Rank.Three, Suit.Diamonds);
		public static readonly Card FourOfDiamonds = new Card(Rank.Four, Suit.Diamonds);
		public static readonly Card FiveOfDiamonds = new Card(Rank.Five, Suit.Diamonds);
		public static readonly Card SixOfDiamonds = new Card(Rank.Six, Suit.Diamonds);
		public static readonly Card SevenOfDiamonds = new Card(Rank.Seven, Suit.Diamonds);
		public static readonly Card EightOfDiamonds = new Card(Rank.Eight, Suit.Diamonds);
		public static readonly Card NineOfDiamonds = new Card(Rank.Nine, Suit.Diamonds);
		public static readonly Card TenOfDiamonds = new Card(Rank.Ten, Suit.Diamonds);
		public static readonly Card JackOfDiamonds = new Card(Rank.Jack, Suit.Diamonds);
		public static readonly Card QueenOfDiamonds = new Card(Rank.Queen, Suit.Diamonds);
		public static readonly Card KingOfDiamonds = new Card(Rank.King, Suit.Diamonds);
		public static readonly Card AceOfDiamonds = new Card(Rank.Ace, Suit.Diamonds);

		public static readonly Card TwoOfClubs = new Card (Rank.Two, Suit.Clubs);
		public static readonly Card ThreeOfClubs = new Card(Rank.Three, Suit.Clubs);
		public static readonly Card FourOfClubs = new Card(Rank.Four, Suit.Clubs);
		public static readonly Card FiveOfClubs = new Card(Rank.Five, Suit.Clubs);
		public static readonly Card SixOfClubs = new Card(Rank.Six, Suit.Clubs);
		public static readonly Card SevenOfClubs = new Card(Rank.Seven, Suit.Clubs);
		public static readonly Card EightOfClubs = new Card(Rank.Eight, Suit.Clubs);
		public static readonly Card NineOfClubs = new Card(Rank.Nine, Suit.Clubs);
		public static readonly Card TenOfClubs = new Card(Rank.Ten, Suit.Clubs);
		public static readonly Card JackOfClubs = new Card(Rank.Jack, Suit.Clubs);
		public static readonly Card QueenOfClubs = new Card(Rank.Queen, Suit.Clubs);
		public static readonly Card KingOfClubs = new Card(Rank.King, Suit.Clubs);
		public static readonly Card AceOfClubs = new Card(Rank.Ace, Suit.Clubs);

		#endregion

		private Queue<Card> _deck = new Queue<Card>(Size);

		public Deck ()
		{
			foreach (var suit in Suits)
				foreach (var rank in Ranks)
					_deck.Enqueue(new Card(rank,suit));
		}
		
		public void Shuffle()
		{
			//Algorithm: Pick two random cards in the deck and swap them.  Do this lots of times.
			var pile = _deck.ToArray();
			var randomizer = new Random(DateTime.Now.Millisecond);
			int swapCount = 0;
			do
			{
				int index1 = randomizer.Next(Deck.Size);
				int index2 = randomizer.Next(Deck.Size);
				var temp = pile[index2];
				pile[index2] = pile[index1];
				pile[index1] = temp;
				swapCount++;
			}
			while (swapCount < Deck.Size * 4);
			_deck = new Queue<Card>(pile);
		}
		
		public IEnumerable<Card> GetCards()
		{
			return _deck.ToArray();
		}
		
		public bool Empty
		{
			get { return _deck.Count == 0;}
		}
		
		public Card TakeTopCard()
		{
			if (_deck.Count == 0)
				throw new InvalidOperationException("Deck is empty.");
			
			return _deck.Dequeue();
		}
	}
}
