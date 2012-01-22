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
            Call call2 = new Call(Seat.East, CallType.Pass);
            Call call3 = new Call(Seat.East, CallType.Double);
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
		public void StringConstructionTest ()
        {
            Call call1 = new Call(Seat.West, CallType.Pass);
            Call call2 = Call.FromString(Seat.West, "P");
            Assert.AreNotEqual(call1, call2);

            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "PASS");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "pA");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "pas");
            Assert.AreEqual(call1.CallType, call2.CallType);

            call1 = new Call(Seat.West, CallType.Double);
            call2 = Call.FromString(Seat.West, "D");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "DouBLe");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "d");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "do");
            Assert.AreEqual(call1.CallType, call2.CallType);

            call1 = new Call(Seat.West, CallType.Redouble);
            call2 = Call.FromString(Seat.West, "R");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "ReDoublE");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "r");
            Assert.AreEqual(call1.CallType, call2.CallType);
            call2 = Call.FromString(Seat.West, "redo");
            Assert.AreEqual(call1.CallType, call2.CallType);

            Bid bid1 = new Bid(3, Suit.Hearts);
            call1 = new Call(Seat.West, CallType.Bid, bid1);
            call2 = Call.FromString(Seat.West, "3H");
            Assert.AreEqual(call1.CallType, call2.CallType);
            Assert.AreEqual(call1.Bid.Suit, Suit.Hearts);
            Assert.AreEqual(call1.Bid.Tricks, 3);
            //Other bid strings are in the bid tests

            try
            {
                Call.FromString(Seat.West, "dbl");
                Assert.Fail();
            }
            catch
            {
            }
            try
            {
                Call.FromString(Seat.West, "passer");
                Assert.Fail();
            }
            catch
            {
            }
            try
            {
                Call.FromString(Seat.West, "12H");
                Assert.Fail();
            }
            catch
            {
            }
        }

        [Test()]
        public void FourPassTest ()
        {
            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);

            List<Call> calls = new List<Call> {};
            Assert.IsFalse(calls.AreLastFourPasses());

            calls = new List<Call> {pass, pass, pass};
            Assert.IsFalse(calls.AreLastFourPasses());

            calls = new List<Call> {pass, pass, pass, pass};
            Assert.IsTrue(calls.AreLastFourPasses());

            calls = new List<Call> {pass, pass, pass, pass, pass};
            Assert.IsTrue(calls.AreLastFourPasses());

            calls = new List<Call> {dbl, pass, pass, pass, pass};
            Assert.IsTrue(calls.AreLastFourPasses());

            calls = new List<Call> {pass, pass, pass, pass, dbl};
            Assert.IsFalse(calls.AreLastFourPasses());
        }

        [Test()]
        public void HasBidAndLastThreeArePassesTest ()
        {
            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);
            var bid = new Call(Seat.North, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.IsFalse(calls.HasBidAndLastThreeArePasses());

            calls = new List<Call> {pass, pass, pass};
            Assert.IsFalse(calls.HasBidAndLastThreeArePasses());

            calls = new List<Call> {pass, pass, pass, bid};
            Assert.IsFalse(calls.HasBidAndLastThreeArePasses());

            calls = new List<Call> {bid, pass, pass, pass, bid};
            Assert.IsFalse(calls.HasBidAndLastThreeArePasses());

            calls = new List<Call> {bid, pass, pass, pass};
            Assert.IsTrue(calls.HasBidAndLastThreeArePasses());

            calls = new List<Call> {pass, bid, dbl, pass, pass, pass};
            Assert.IsTrue(calls.HasBidAndLastThreeArePasses());

            calls = new List<Call> {pass, bid, pass, bid, dbl, pass, pass};
            Assert.IsFalse(calls.HasBidAndLastThreeArePasses());

            calls = new List<Call> {pass, bid, pass, bid, dbl, pass, pass, pass};
            Assert.IsTrue(calls.HasBidAndLastThreeArePasses());
        }


        [Test()]
        public void DoubleValueTest ()
        {
            // get last double or redouble not followed by a bid
            // rely on other methods to ensure call list is properly formed

            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);
            var redbl = new Call(Seat.North, CallType.Redouble);
            var bid = new Call(Seat.North, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.AreEqual(calls.GetDoubles(), 0);

            calls = new List<Call> {pass, pass};
            Assert.AreEqual(calls.GetDoubles(), 0);

            calls = new List<Call> {pass, dbl, pass};
            Assert.AreEqual(calls.GetDoubles(), 1);

            calls = new List<Call> {bid, dbl, redbl, pass, pass, pass};
            Assert.AreEqual(calls.GetDoubles(), 2);

            calls = new List<Call> {bid, redbl, dbl, pass};
            Assert.AreEqual(calls.GetDoubles(), 1);

            calls = new List<Call> {bid, dbl, redbl, bid};
            Assert.AreEqual(calls.GetDoubles(), 0);

            calls = new List<Call> {bid, dbl, redbl, pass, bid, pass};
            Assert.AreEqual(calls.GetDoubles(), 0);
        }

        [Test()]
        public void IsDoubleValidNowTest ()
        {
            // Law 19 "A player may double only the last preceding bid, and then
            // only if it was made by an opponent and no calls other than pass
            // have intervened."
            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);
            var redbl = new Call(Seat.North, CallType.Redouble);
            var bid = new Call(Seat.North, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {pass};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {bid};
            Assert.IsTrue(calls.IsDoubleValidNow());

            calls = new List<Call> {bid, pass};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {bid, dbl};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {bid, bid};
            Assert.IsTrue(calls.IsDoubleValidNow());

            calls = new List<Call> {pass, bid};
            Assert.IsTrue(calls.IsDoubleValidNow());

            calls = new List<Call> {pass, pass};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {pass, pass, pass};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {bid, dbl, redbl};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {bid, pass, pass};
            Assert.IsTrue(calls.IsDoubleValidNow());

            calls = new List<Call> {bid, pass, bid};
            Assert.IsTrue(calls.IsDoubleValidNow());

            calls = new List<Call> {bid, dbl, bid};
            Assert.IsTrue(calls.IsDoubleValidNow());

            calls = new List<Call> {bid,pass, pass, pass};
            Assert.IsFalse(calls.IsDoubleValidNow());

            calls = new List<Call> {pass, bid, pass, pass};
            Assert.IsTrue(calls.IsDoubleValidNow());

        }

        [Test()]
        public void IsRedoubleValidNowTest ()
        {
            // Law 19 "A player may double only the last preceding double, and then
            // only if it was made by an opponent and no calls other than pass
            // have intervened."
            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);
            var redbl = new Call(Seat.North, CallType.Redouble);
            var bid = new Call(Seat.North, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid, pass};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid, bid};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid, dbl};
            Assert.IsTrue(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass, bid};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass, pass};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass, pass, pass};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass, pass, bid};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass, bid, dbl};
            Assert.IsTrue(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass, pass, dbl};
            Assert.IsTrue(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid, dbl, pass};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {dbl, pass, pass};
            Assert.IsTrue(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid, dbl, redbl};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {dbl, pass, bid};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid, dbl, bid};
            Assert.IsFalse(calls.IsRedoubleValidNow());

            calls = new List<Call> {bid, dbl, pass, pass};
            Assert.IsTrue(calls.IsRedoubleValidNow());

            calls = new List<Call> {pass, bid, bid, pass, pass, dbl};
            Assert.IsTrue(calls.IsRedoubleValidNow());
        }

        [Test()]
        public void FirstCallTest ()
        {
            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);
            var redbl = new Call(Seat.North, CallType.Redouble);
            var bid1 = new Call(Seat.North, CallType.Bid, new Bid(1, Suit.Spades));
            var bid2 = new Call(Seat.East, CallType.Bid, new Bid(2, Suit.Hearts));
            var bid3 = new Call(Seat.South, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.AreEqual(calls.First(CallType.Bid, Suit.Clubs, Side.NorthSouth), null);

            calls = new List<Call> {bid1, bid2, dbl, redbl, pass, pass, bid3};
            Assert.AreEqual(calls.First(CallType.Bid, Suit.Clubs, Side.NorthSouth), null);
            Assert.AreEqual(calls.First(CallType.Bid, Suit.Hearts, Side.NorthSouth), null);
            Assert.AreEqual(calls.First(CallType.Bid, Suit.Hearts, Side.WestEast).Bidder, bid2.Bidder);
            Assert.AreEqual(calls.First(CallType.Bid, Suit.Spades, Side.NorthSouth).Bidder, bid1.Bidder);
            Assert.AreEqual(calls.First(CallType.Pass, Suit.Clubs, Side.NorthSouth).Bidder, pass.Bidder);
            Assert.AreEqual(calls.First(CallType.Pass, Suit.Clubs, Side.WestEast), null);
            Assert.AreEqual(calls.First(CallType.Double, Suit.Clubs, Side.NorthSouth).Bidder, dbl.Bidder);
            Assert.AreEqual(calls.First(CallType.Double, Suit.Clubs, Side.WestEast), null);
            Assert.AreEqual(calls.First(CallType.Redouble, Suit.None, Side.NorthSouth).Bidder, redbl.Bidder);
            Assert.AreEqual(calls.First(CallType.Redouble, Suit.None, Side.WestEast), null);
        }

        [Test()]
        public void LastBidTest ()
        {
            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);
            var redbl = new Call(Seat.North, CallType.Redouble);
            var bid1 = new Call(Seat.North, CallType.Bid, new Bid(1, Suit.Spades));
            var bid2 = new Call(Seat.East, CallType.Bid, new Bid(2, Suit.Hearts));
            var bid3 = new Call(Seat.South, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.IsNull(calls.LastBid());

            calls = new List<Call> {bid1};
            Assert.AreEqual(calls.LastBid().Suit, Suit.Spades);
            Assert.AreEqual(calls.LastBid().Tricks, 1);
            calls = new List<Call> {bid1, pass};
            Assert.AreEqual(calls.LastBid().Suit, Suit.Spades);
            Assert.AreEqual(calls.LastBid().Tricks, 1);
            calls = new List<Call> {bid1, bid2, dbl, redbl, pass, pass, bid3};
            Assert.AreEqual(calls.LastBid().Suit, Suit.Spades);
            Assert.AreEqual(calls.LastBid().Tricks, 3);
            calls = new List<Call> {bid1, bid2, dbl, redbl, pass, pass, pass};
            Assert.AreEqual(calls.LastBid().Suit, Suit.Hearts);
            Assert.AreEqual(calls.LastBid().Tricks, 2);
        }

        [Test()]
        public void LastBidderTest ()
        {
            var pass = new Call(Seat.South, CallType.Pass);
            var dbl = new Call(Seat.West, CallType.Double);
            var redbl = new Call(Seat.North, CallType.Redouble);
            var bid1 = new Call(Seat.North, CallType.Bid, new Bid(1, Suit.Spades));
            var bid2 = new Call(Seat.East, CallType.Bid, new Bid(2, Suit.Hearts));
            var bid3 = new Call(Seat.South, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.AreEqual(calls.LastBidder(), Seat.None);

            calls = new List<Call> {pass, pass};
            Assert.AreEqual(calls.LastBidder(), Seat.None);

            calls = new List<Call> {bid1};
            Assert.AreEqual(calls.LastBidder(), Seat.North);

            calls = new List<Call> {bid1, pass};
            Assert.AreEqual(calls.LastBidder(), Seat.North);

            calls = new List<Call> {bid1, bid2, dbl, redbl, pass, pass, bid3};
            Assert.AreEqual(calls.LastBidder(), Seat.South);

            calls = new List<Call> {bid1, bid2, dbl, redbl, pass, pass, pass};
            Assert.AreEqual(calls.LastBidder(), Seat.East);
        }

        [Test()]
        public void IsCallLegalTest ()
        {
            var pass = new Call(Seat.North, CallType.Pass);
            var dbl = new Call(Seat.North, CallType.Double);
            var redbl = new Call(Seat.North, CallType.Redouble);
            var bid1 = new Call(Seat.North, CallType.Bid, new Bid(1, Suit.Spades));
            var bid2 = new Call(Seat.East, CallType.Bid, new Bid(2, Suit.Hearts));
            var bid3 = new Call(Seat.South, CallType.Bid, new Bid(3, Suit.Spades));

            List<Call> calls = new List<Call> {};
            Assert.IsTrue(calls.IsCallLegal(pass));
            Assert.IsTrue(calls.IsCallLegal(bid1));
            Assert.IsTrue(calls.IsCallLegal(bid2));
            Assert.IsFalse(calls.IsCallLegal(dbl));
            Assert.IsFalse(calls.IsCallLegal(redbl));

            calls = new List<Call> {pass};
            Assert.IsTrue(calls.IsCallLegal(pass));
            Assert.IsTrue(calls.IsCallLegal(bid1));
            Assert.IsTrue(calls.IsCallLegal(bid2));
            Assert.IsFalse(calls.IsCallLegal(dbl));
            Assert.IsFalse(calls.IsCallLegal(redbl));

            calls = new List<Call> {bid1};
            Assert.IsTrue(calls.IsCallLegal(pass));
            Assert.IsFalse(calls.IsCallLegal(bid1));
            Assert.IsTrue(calls.IsCallLegal(bid2));
            Assert.IsTrue(calls.IsCallLegal(dbl));
            Assert.IsFalse(calls.IsCallLegal(redbl));

            calls = new List<Call> {bid1, pass};
            Assert.IsTrue(calls.IsCallLegal(pass));
            Assert.IsFalse(calls.IsCallLegal(bid1));
            Assert.IsTrue(calls.IsCallLegal(bid2));
            Assert.IsFalse(calls.IsCallLegal(dbl));
            Assert.IsFalse(calls.IsCallLegal(redbl));

        }

    }
}