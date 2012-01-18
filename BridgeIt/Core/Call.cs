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
	public class Call
	{
		public Call (Seat bidder, CallType type, Bid bid = null)
		{
			if (bidder == Seat.None)
				throw new ArgumentException("Bidder must not be none.");
			if (type == CallType.None)
				throw new ArgumentException("Call type must not be none.");
			if (type == CallType.Bid && bid == null)
				throw new ArgumentException("Bid must not be null when call type is bid.");
				
			Bidder = bidder;
			CallType = type;
			Bid = bid;					
		}
		
		public Seat Bidder { get; private set; }
		public CallType CallType { get; private set; }
		public Bid Bid { get; private set; }
		
		public static Call FromString(Seat bidder, string s)
		{
			string lower = s.Trim().ToLower();
			if (lower == "p" || lower == "pass")
				return new Call(bidder, CallType.Pass);
			if (lower == "d" || lower == "double")
				return new Call(bidder, CallType.Double);
			if (lower == "r" || lower == "redouble")
				return new Call(bidder, CallType.ReDouble);
			return new Call(bidder, CallType.Bid, Bid.FromString(s));
		}
		
		public override string ToString ()
		{
			return string.Format ("[Call: Bidder={0}, CallType={1}, Bid={2}]", Bidder, CallType, Bid);
		}
	}
}