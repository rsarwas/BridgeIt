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
using System.Linq;

namespace BridgeIt.Core
{
	public class Trick
	{

		private readonly List<Card> _cards;
		private readonly List<Seat> _players;
		private Seat _winner;
			

		public Trick(Suit trump)
		{
			Trump = trump;
			_cards = new List<Card>(4);
			_players = new List<Seat>(4);
			Suit = Suit.None;
		}

		public void AddCard (Card card, Seat player)
        {
            if (Done)
                throw new Exception("Trick is done, cannot add more cards.");
            if (_cards.Contains(card))
                throw new Exception("Card is already in the trick.");
			
            //FIXME - add check for player has already played
            //FIXME - add check for card is added by the right player (clockwise around table)
			
            _cards.Add(card);
            _players.Add(player);

            _winner = GetWinner();
            if (Suit == Suit.None)
                Suit = card.Suit;
        }

        public bool IsLegalPlay (Card card, Hand hand)
        {
            return !Done && (IsEmpty ||
                             Suit == card.Suit ||
                             hand.VoidOfSuit(Suit));
        }


        public bool IsEmpty
        {
            get { return _cards.Count == 0; }
        }

		public bool Done
		{
			get { return _cards.Count == 4; }
		}
	
		public Suit Trump { get; private set; }
		
		public Suit Suit { get; private set; }
		
		public IEnumerable<Card> Cards
		{
			get { return new List<Card>(_cards); }
		}
		
		
		public Seat Winner
		{
			get 
			{
				if (!Done)
					throw new Exception("Cannot determine the winner until all players have played");
				return _winner;
			}
		}
		
		//Law 44 - "A trick containing a trump is won by the player who has contributed to
        //it the highest trump. A trick that does not contain a trump is won by the player
        //who has contributed to it the highest card of the suit led."
        private Seat GetWinner ()
        {
            return WhoPlayed(GetHighestCard());
        }

        public Seat WhoPlayed(Card card)
		{
			if (!_cards.Contains(card))
				throw new Exception("Card is not in this trick");
			return _players[_cards.IndexOf(card)];
		}
		
		
		private Card GetHighestCard ()
		{
			if (_cards.Count == 0)
				throw new Exception ("There are no cards in the trick");

			Card highest = _cards[0];
			foreach (Card card in _cards)
			{
				if (card == highest)
					continue;
				if (card.Beats(highest,Trump))
					highest = card;
			}
			return highest;
		}
		
		override public string ToString()
		{
			//TODO - use stringbuilder
            string msg = Cards.Aggregate("[", (current, card) => current + card.ToGlyphString() + ", ");
		    msg = msg.Substring(0, msg.Length-2) + "]";
			return msg;
		}
	}
}