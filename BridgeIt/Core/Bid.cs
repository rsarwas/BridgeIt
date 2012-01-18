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

namespace BridgeIt.Core
{
	//Bid is an immutable.  I use a class and not a struct, because
	//there is no logical default (0) value for a bid, and I need to
	//represent a lack of a bid (null) in other objects (i.e. Calls)
	
	public class Bid
	{
		public Bid (int tricks, Suit suit)
		{
			if (tricks < 1 || tricks > 7)
				throw new ArgumentOutOfRangeException ("Number of tricks must in the range 1 to 7");
			Tricks = tricks;
			Suit = suit;
		}
		
		public bool Beats(Bid other)
		{
			if (other.Tricks < this.Tricks)
				return true;
			if (other.Tricks == this.Tricks && other.Suit < this.Suit)
				return true;
			return false;
		}
		
		public int Tricks  { get; private set; }
		
		public Suit Suit  { get; private set; }
		
		public static Bid FromString (string bidString)
		{
			if (bidString == null)
				throw new ArgumentNullException ("bidString");
			if (bidString.Trim ().Length < 2)
				throw new ArgumentException ("Bid must be in the form NumberOfTricks Suit");
			string trickString = bidString.TrimStart ().Substring (0, 1);
			string suitString = bidString.TrimStart ().Substring (1).Trim ();
			int tricks;
			if (!int.TryParse (trickString, out tricks))
				throw new ArgumentException ("First part of bid must be an integer");
			Suit suit = Card.SuitFromString (suitString);
			return new Bid (tricks, suit);
		}
		
		//FIXME implement Iequality and Icompare
		
		public override string ToString ()
		{
			return string.Format ("{0}{1}", Tricks, Card.SuitToGlyph(Suit));
		}
	}
}

