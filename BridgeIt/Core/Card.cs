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
Card is an immutable value object having
a rank (2..A), and a suit (S,H,D,C)
Cards support equality.
Cards of the same suit support ordering
It is an exception to order cards of different suits
*/

using System;

namespace BridgeIt.Core
{
	public struct Card
	{

		#region static conversion members
 
		public static Card FromString (string card)
        {
            //Fixme - this should do better
            if (string.IsNullOrEmpty(card))
                throw new ArgumentException("Card must be written as 'Rank <space> Suit'", "card");
            string[] parts = card.Split();
            if (parts.Length != 2)
                throw new ArgumentException("Card must be written as 'Rank <space> Suit'", "card");
            Rank rank = RankFromString(parts[0]);
            Suit suit = SuitFromString(parts[1]);
            return new Card(rank, suit);
        }
		
		public static Suit SuitFromString (string suitString)
		{
			string suit = suitString.ToUpper();
			if (suit == "S" || suit == "SPADES")
				return Suit.Spades;
			if (suit == "H" || suit == "HEARTS")
				return Suit.Hearts;
			if (suit == "D" || suit == "DIAMONDS")
				return Suit.Diamonds;
			if (suit == "C" || suit == "CLUBS")
				return Suit.Clubs;
			if (suit == "NT" || suit == "NO TRUMP" || suit == "NOTRUMP" || suit == "NONE")
				return Suit.None;
			throw new ArgumentException ("'" + suitString + "' is not a valid card suit.");
		}
		
		
		public static Rank RankFromString (string rankString)
		{
			string rank = rankString.ToUpper ();
			if (rank == "2" || rank == "TWO")
				return Rank.Two;
			if (rank == "3" || rank == "THREE")
				return Rank.Three;
			if (rank == "4" || rank == "FOUR")
				return Rank.Four;
			if (rank == "5" || rank == "FIVE")
				return Rank.Five;
			if (rank == "6" || rank == "SIX")
				return Rank.Six;
			if (rank == "7" || rank == "SEVEN")
				return Rank.Seven;
			if (rank == "8" || rank == "EIGHT")
				return Rank.Eight;
			if (rank == "9" || rank == "NINE")
				return Rank.Nine;
			if (rank == "10" || rank == "TEN")
				return Rank.Ten;
			if (rank == "J" || rank == "JACK")
				return Rank.Jack;
			if (rank == "Q" || rank == "QUEEN")
				return Rank.Queen;
			if (rank == "K" || rank == "KING")
				return Rank.King;
			if (rank == "A" || rank == "ACE")
				return Rank.Ace;
			throw new ArgumentException("'" + rankString + "' is not a valid card rank");
		}


		public static string SuitToString (Suit suit)
		{
			switch (suit)
			{
				case Suit.None: return "NT";
				case Suit.Spades: return "S";
				case Suit.Hearts: return "H";
				case Suit.Diamonds: return "D";
				case Suit.Clubs: return "C";
				default:
					throw new ArgumentException ("'" + suit + "' is not a recognized card suit.");
			}
		}


		public static string SuitToGlyph (Suit suit)
		{
			switch (suit)
			{
				case Suit.None: return "NT";
				case Suit.Spades: return "\u2660";
				case Suit.Hearts: return "\u2665";
				case Suit.Diamonds: return "\u2666";
				case Suit.Clubs: return "\u2663";
				default:
					throw new ArgumentException ("'" + suit + "' is not a recognized card suit.");
			}
		}


		public static string RankToString (Rank rank)
		{
			switch (rank)
			{
				case Rank.Ace: return "A";
				case Rank.King: return "K";
				case Rank.Queen: return "Q";
				case Rank.Jack: return "J";
				case Rank.Ten: return "10";
				case Rank.Nine: return "9";
				case Rank.Eight: return "8";
				case Rank.Seven: return "7";
				case Rank.Six: return "6";
				case Rank.Five: return "5";
				case Rank.Four: return "4";
				case Rank.Three: return "3";
				case Rank.Two: return "2";
				default:
					throw new ArgumentException ("'" + rank + "' is not a recognized card rank.");
			}
		}

		#endregion

		public Card (Rank rank, Suit suit) :this()
		{
			if (suit == Suit.None)
				throw new ArgumentException("Suit cannot be none", "suit");
			this.Rank = rank;
			this.Suit = suit;
		}


		public Card (string rank, string suit) :
			this(Card.RankFromString(rank),Card.SuitFromString(suit))
		{
		}


		public Rank Rank { get; private set; }


		public Suit Suit { get; private set; }


		public bool Beats (Card other, Suit trump)
		{
			return this.Trumps(other, trump) ||
			      (this.HasSameSuitAs(other) && this.HasGreaterRankThan(other));
		}


		public bool Trumps (Card other, Suit trump)
		{
			//a trump doesn't trump a trump (two trumps will be compared by rank)
			return this.Suit == trump && other.Suit != trump;
		}


		public bool HasSameSuitAs (Card other)
		{
			return this.Suit == other.Suit;
		}


		public bool HasGreaterRankThan (Card other)
		{
			return this.Rank > other.Rank;
		}


		public int Index
		{
			get {
				return (int)Rank + (int)Suit * (((int)Rank.Ace) + 1);
			}
		}


		public override bool Equals (Object obj)
		{
			return obj is Card && this == (Card)obj;
		}


		public override int GetHashCode ()
		{
			return Suit.GetHashCode() ^ Rank.GetHashCode();
		}


		public static bool operator == (Card x, Card y)
		{
			return x.Suit == y.Suit && x.Rank == y.Rank;
		}


		public static bool operator != (Card x, Card y)
		{
			return !(x == y);
		}


		override public string ToString ()
		{
			return ToLongString();
		}
		
		public string ToShortString()
		{
			return SuitToString(Suit) + RankToString(Rank);
		}
		
		public string ToGlyphString()
		{
			return SuitToGlyph(Suit) + RankToString(Rank);
		}
		
		public string ToLongString()
		{
			return Rank + " of " + Suit;
		}
	}
}