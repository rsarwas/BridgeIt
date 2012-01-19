#region License and Terms
// BridgeIt
// Copyright (c) 2011-2012 Regan Sarwas. All rights reserved.
//
// Licensed under the GNU General  License, Version 3.0 (the "License");
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

using System.Collections.Generic;

namespace BridgeIt.Core
{
    interface ITable
    {
        Seat Dealer { get; }

        Seat Declarer { get; }

        Seat Dummy { get; }

        Seat ActivePlayer { get; }

        Suit Trump { get; }

        Call CurrentCall { get; }  //TODO - verify this is being used

        Contract Contract { get; }

        Trick CurrentTrick { get; }
        
        IEnumerable<Card> CardsOnTheTable { get; }

        IEnumerable<Call> LastThreeCalls { get; }


        
        IEnumerable<Card> GetHand (IPlayer player);

        Seat SitDown (IPlayer player);

        void Start (Seat dealer = Seat.South);

        void Call (IPlayer player, Call call);

        void Play (IPlayer player, Card card);

        void Quit (IPlayer player);

        Seat NextSeat (Seat seat);
    }
}
