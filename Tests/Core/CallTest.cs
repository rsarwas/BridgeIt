#region License and Terms
// BridgeIt
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

using BridgeIt.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests.Core
{
	[TestFixture()]
	public class CallTest
	{
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void BadCallConstructionSeatNone ()
        {
            new Call(Seat.None, CallType.Double, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void BadCallConstructionTypeNone ()
        {
            new Call(Seat.West, CallType.None, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void BadCallConstructionBidNull ()
        {
            new Call(Seat.West, CallType.Bid, null);
        }

        [Test]
        public void CallConstructionAndPropertyAccess ()
        {
            Bid bid1 = new Bid(3, Suit.Hearts);
            Bid bid2 = new Bid(3, Suit.NoTrump);

            Call call1 = new Call(Seat.West, CallType.Pass, null);
            Call call2 = new Call(Seat.East, CallType.Pass, null);
            Call call3 = new Call(Seat.East, CallType.Double, null);
            Call call4 = new Call(Seat.East, CallType.Redouble, null);
            Call call5 = new Call(Seat.East, CallType.Bid, bid1);
            Call call6 = new Call(Seat.East, CallType.Pass, bid2);
            Call call7 = new Call(Seat.North, CallType.Pass, bid2);

            Assert.AreEqual(Seat.West, call1.Bidder);
            Assert.AreNotEqual(call1.Bidder, call2.Bidder);
            Assert.AreEqual(call2.Bidder, call3.Bidder);

            Assert.AreEqual(CallType.Pass, call1.CallType);
            Assert.AreEqual(call1.CallType, call2.CallType);
            Assert.AreNotEqual(call2.CallType, call3.CallType);
            Assert.AreNotEqual(call3.CallType, call4.CallType);
            Assert.AreNotEqual(call4.CallType, call5.CallType);
            Assert.AreNotEqual(call5.CallType, call6.CallType);
            Assert.AreEqual(call6.CallType, call2.CallType);

            Assert.AreEqual(null, call4.Bid);
            Assert.AreEqual(call3.Bid, call4.Bid);
            Assert.AreNotEqual(call5.Bid, call6.Bid);
            Assert.AreEqual(call6.Bid, call7.Bid);
            Assert.AreNotEqual(call7.Bid, call1.Bid);
        }


		[Test()]
		public void Test2 ()
		{
		}
	}
}