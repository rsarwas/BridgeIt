// #region License and Terms
// // BridgeIt
// // Copyright (c) 2011-2012 Regan Sarwas. All rights reserved.
// //
// // Licensed under the GNU General Public License, Version 3.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// // 
// //     http://www.gnu.org/licenses/gpl-3.0.html
// // 
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// #endregion
// 

using System.Collections.Generic;
using System.Linq;

namespace BridgeIt.Core
{
    public static class CallExtensions
    {

        public static bool AreLastFourPasses (this IEnumerable<Call> calls)
        {
            return calls.Last(4).Count(c => c.CallType == CallType.Pass) == 4;
        }


        public static bool HasBidAndLastThreeArePasses (this IEnumerable<Call> calls)
        {
            //FIXME - multiple enumerations
            Bid lastBid = calls.LastBid();
            return lastBid != null && calls.Last(3).Count(c => c.CallType == CallType.Pass) == 3;
        }


        public static int GetDoubles (this IEnumerable<Call> calls)
        {
            // Law 19 - "All doubles and redoubles are superseded by a
            // subsequent legal bid. If there is no subsequent bid,
            // scoring values are increased as provided in Law 81."
            //foreach (Call call in calls.Reverse<Call>())
            foreach (Call call in calls.Reverse())
                {
                if (call.CallType == CallType.Redouble)
                    return 2;
                if (call.CallType == CallType.Double)
                    return 1;
                if (call.CallType == CallType.Bid)
                    return 0;
            }
            return 0;
        }


        public static Call First (this IEnumerable<Call> calls, CallType type, Suit suit, Side side)
        {
            return calls.FirstOrDefault(c => c.CallType == type &&
                                             (c.CallType != CallType.Bid || c.Bid.Suit == suit) &&
                                             c.Bidder.GetSide() == side);
        }


        public static Bid LastBid (this IEnumerable<Call> calls)
        {
            Call lastBidCall = calls.LastOrDefault(c => c.CallType == CallType.Bid);
            return (lastBidCall == null) ? null : lastBidCall.Bid;
        }


        public static Seat LastBidder (this IEnumerable<Call> calls)
        {
            Call lastBidCall = calls.LastOrDefault(c => c.CallType == CallType.Bid);
            return (lastBidCall == null) ? Seat.None : lastBidCall.Bidder;
        }


        public static bool IsCallLegal (this IEnumerable<Call> calls, Call call)
        {
            //FIXME - insufficent bids may be acceptable - see Law 27
            switch (call.CallType)
            {
                case CallType.Redouble:
                    return calls.IsRedoubleValidNow();

                case CallType.Double:
                    return calls.IsDoubleValidNow();

                case CallType.Bid:
                    Bid thisBid = call.Bid;
                    Bid lastBid = calls.LastBid();
                    return thisBid.IsSufficient(lastBid);

                case CallType.Pass:
                    return true;

                default:
                    return false;
            }
        }


        public static bool IsRedoubleValidNow (this IEnumerable<Call> calls)
        {
            // Law 19 "A player may double only the last preceding double, and then
            // only if it was made by an opponent and no calls other than pass
            // have intervened."

            List<Call> lastCalls = (List<Call>)calls.Last(3);
            int countOfCalls = lastCalls.Count;

            if (countOfCalls < 2)
                return false;
            if (countOfCalls == 2)
            {
                return lastCalls[0].CallType == CallType.Bid &&
                               lastCalls[1].CallType == CallType.Double;
            }
            // countOfCalls > 2
            return lastCalls[2].CallType == CallType.Double ||
                           (lastCalls[0].CallType == CallType.Double &&
                            lastCalls[1].CallType == CallType.Pass &&
                            lastCalls[2].CallType == CallType.Pass);

        }


        public static bool IsDoubleValidNow (this IEnumerable<Call> calls)
        {
            // Law 19 "A player may double only the last preceding bid, and then
            // only if it was made by an opponent and no calls other than pass
            // have intervened."

            List<Call> lastCalls = (List<Call>)calls.Last(3);
            int countOfCalls = lastCalls.Count;

            if (countOfCalls < 1)
                return false;
            if (countOfCalls == 1)
            {
                return lastCalls[0].CallType == CallType.Bid;
            }
            if (countOfCalls == 2)
            {
                return lastCalls[1].CallType == CallType.Bid;
            }
            // countOfCalls > 2
            return lastCalls[2].CallType == CallType.Bid ||
                           (lastCalls[0].CallType == CallType.Bid &&
                            lastCalls[1].CallType == CallType.Pass &&
                            lastCalls[2].CallType == CallType.Pass);
        }
    }
}

