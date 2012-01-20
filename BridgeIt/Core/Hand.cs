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


/*
Hand holds the cards belonging to a player.
A hand should be nearly immutable, only allowing
removal of cards by the table when a legal move
is made.
To enforce this, the table creates all hands,
and does not provide references to them.  It
will only provide a readonly copy.

A player receives it's hand from the table.
A player should not be able to get another player's
hand unless a player has a reference to another player
(i.e. when declarer can see dummy's hand).

A hand does not allow adding or changing a card,
A hand does allow removing a card.

When a player plays a card, the table validates
the play, and removes the card from the hand.
the player is responsible for updating it's
view of the hand.
*/


using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeIt.Core
{
	public class Hand
	{
		public const int MaxSize = 13;
		
		private readonly List<Card> _cards = new List<Card>();
		
		//test arg must not be null, must be cards, must be unique, must be thirteen
		//must return what was added
		public Hand (IEnumerable<Card> cards)
		{
			//Make Add private, remove default constructor, add constructor that requires 13 unique cards
			if (cards == null)
				throw new ArgumentNullException("cards");
			if (cards.Count() != MaxSize)
				throw new ArgumentException("You must provide "+MaxSize+" cards for the hand.");
			foreach (Card card in cards)
				Add (card);
		}
		
		
		//tests: Must be a card (enforced by compiler), must be unique, cannot add more than 13 
		private void Add(Card card)
		{
			if (_cards.Contains(card))
				throw new ArgumentException("The card is already in the hand","card");
			if (_cards.Count == MaxSize)
				throw new InvalidOperationException("Cannot add more than "+MaxSize+" cards to the hand.");
			_cards.Add(card);
		}
		
		public void Remove(Card card)
		{
			if (!_cards.Contains(card))
				throw new ArgumentException("The card is not in the hand","card");
			_cards.Remove(card);
		}

        public bool VoidOfSuit (Suit suit)
        {
            return !_cards.Any(c => c.Suit == suit);
        }
        
		public int Count { get {return _cards.Count; }}
		
		public int GetCount(Suit suit)
		{
			return _cards.Count(c => c.Suit == suit);
		}
		
		public bool Contains(Card card)
		{
			return _cards.Contains(card);
		}
		
		//Test: must not allow modifing the original
		public IEnumerable<Card> Cards
		{
			get { return new List<Card>(_cards); }
		}
		
		//Test: must not allow modifing the original
		//Test: must only return cards in the correct suit
		//Test: empty list is always returned with Suit.None
		public IEnumerable<Card> GetCards(Suit suit)
		{
			return _cards.Where(c => c.Suit == suit);
		}
		
		//Print in rows by suit
	}
}

