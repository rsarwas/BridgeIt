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
using System;
using System.Threading;
using BridgeIt.Core;
using BridgeIt.Tables;  //FIXME remove this by adding events to ITable
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeIt.Players
{
    public class SimpleComputerPlayer : IPlayer
    {
        // Private Data
        bool _stop;
        //bool _timeToBid;
        //bool _timeToPlay;
        //bool _timeToPlayForDummy;
        bool _manageDummy;
        Seat _mySeat;
        Table _table;
        bool _tableStateHasChanged;

        private Thread Thread { get; set; }

        //FIXME Use ITable this by adding events to ITable
        public void JoinTable (Table table)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            SignupForMessages(table);
            try
            {
                _mySeat = table.SitDown(this);
            }
            catch (Exception)
            {
                UnsubscribeToMessages(table);
                throw;
            }
            _table = table;
        }


        public Thread Start ()
        {
            if (_table == null)
                throw new InvalidOperationException("You can't start until you sitting at a table");

            Debug.Print("Simple Computer Player started by thread {0} @ {2} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            Thread = new Thread(Run) {IsBackground = true};
            Thread.Start();
            return Thread;
        }


        public void Stop ()
        {
            if (Thread == null)
                throw new InvalidOperationException("This player has not been started");

            if (!Thread.IsAlive)
                throw new InvalidOperationException("This player has not been stopped and cannot be started");

            Debug.Print("SCP at seat {2} stopped by thread {0} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            _stop = true;
        }

        
        #region Methods required for interface
        //FIXME - rethink the strategy for forcing a move
        public void PlaceBidNow (TimeSpan timelimit)
        {
            throw new NotImplementedException();
        }
        public void PlayNow (TimeSpan timelimit)
        {
            throw new NotImplementedException();
        }
        public void PlayForDummyNow (TimeSpan timelimit)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void Run ()
        {
            Debug.Print("SCP at seat {2} has entered the run loop on thread {0} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            while (!_stop)
            {
                //Keep the time in DoSomeWork about 100 milleseconds or less for responsibveness
                //Do not let it get too low, or else we will waste a lot of CPU time spinning this loop
                WorkOnGameTree();

                //These flags will be set by the delegates from the table messages
                //these flags are cleared on success, but not on failure in the hopes that I might
                //succeed after working on the game tree some more.

                if (_tableStateHasChanged)
                {
                    //?? Do this after I have placed my bid/played my card, to hopefully cancel the triggered state change
                    //TODO - validate this.
                    //Could I check, not do anything, then stop checking, and miss my turn?
                    _tableStateHasChanged = false;

                    //_table.HotSeat won't change if I'm in the hotseat, until I do something.
                    if (_table.AllowingBidFrom(_mySeat))
                    {
                        Call call = GetBestCall();
                        //Debug.Print("SCP at Seat {0} called: {1}", _mySeat, call);
                        try
                        {
                            _table.MakeCall(this, call);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print("Call failed for SCP at Seat {0}: {1}", _mySeat, ex);
                            //throw;
                        }
                        continue;  //DO NOT try to play a card after we have bid
                    }

                    if (_table.AllowingCardFrom(_mySeat))
                    {
                        Card card = GetBestCard();
                        //Debug.Print("SCP at Seat {0} played {1}", _mySeat, card);
                        try
                        {
                            _table.PlayCard(this, card);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print("Play failed for SCP at Seat {0}: {1}", _mySeat, ex);
                            //throw;
                        }
                    }

                    //_manageDummy will not change
                    //And Hotseat will not change if dummy is in the hotseat and I am managing dummy
                    if (_table.AllowingCardFromDummyBy(_mySeat))
                    {
                        Card card = GetBestCard();
                        //Debug.Print("SCP at Seat {0} played {1} For Dummy", _mySeat, card);
                        try
                        {
                            _table.PlayCard(this, card);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print("Play failed for SCP at Seat {0}: {1}", _mySeat, ex);
                            //throw;
                        }
                    }
                }
            }
            Debug.Print("SCP at seat {2} has left the run loop on thread {0} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            LeaveTable();
        }

        void LeaveTable ()
        {
             UnsubscribeToMessages(_table);
             _table.Quit(this);
             //Debug.Print("SCP at Seat {0} quit the table", _mySeat);
             _table = null;
             _mySeat = Seat.None;
        }

        private void WorkOnGameTree ()
        {
            //try to keep the amount of work done here, small
            //Since we are not checking for interupts while working

            //If I have work to do
            //    do it
            //else
               Thread.Sleep(100);
        }


        Call GetBestCall ()
        {
            //FIXME - every player can't return a pass every time, or we go into a loop
            bool noBid = _table.LastThreeCalls.Count(c => c.CallType == CallType.Bid) == 0;
            IEnumerable<Card> hand = _table.GetHand(this);
            if (noBid && hand.HighCardPoints() > 12)
                return new Call(_mySeat, CallType.Bid, new Bid(1, hand.LongestSuit()));
            return new Call(_mySeat, CallType.Pass);
        }

        Card GetBestCard ()
        {
            //Play highest card from hand in suit that lead
            //otherwise play highest trump
            //otherwise play a random card
            IEnumerable<Card> hand = _table.HotSeat == _table.Dummy ? _table.DummiesCards : _table.GetHand(this);
            //FIXME - multiple enumerations of hand
            //FIXME - Current Trick could be null after game has ended, and I am still thinking game on.
            if (!_table.CurrentTrick.IsEmpty)
            {
                var suitLead = _table.CurrentTrick.Cards.First().Suit;
                if (!hand.VoidOfSuit(suitLead))
                    return hand.GetHighestInSuit(suitLead);

                if (!hand.VoidOfSuit(_table.Trump))
                    return hand.GetHighestInSuit(_table.Trump);
            }
            return hand.First();
        }


        //FIXME Use ITable this by adding events to ITable
        private void SignupForMessages (Table table)
        {
            table.BiddingIsComplete += BiddingIsComplete;
            table.CallHasBeenMade += CallHasBeenMade;
            table.CardsHaveBeenDealt += CardsHaveBeenDealt;
            //table.PlayerHasJoined += PlayerHasJoined;
            //table.DealHasBeenAbandoned += DealHasBeenAbandoned;
            table.DealHasBeenWon += DealHasBeenWon;
            //table.SessionHasBegun += SessionHasBegun;
            //table.DealHasBegun += DealHasBegun;
            table.SessionHasEnded += SessionHasEnded;
            table.CardHasBeenPlayed += CardHasBeenPlayed;
            table.TrickHasBeenWon += TrickHasBeenWon;
            //table.DummyHasExposedHand += DummyHasExposedHand;
        }


        //FIXME Use ITable this by adding events to ITable
        private void UnsubscribeToMessages (Table table)
        {
            table.BiddingIsComplete -= BiddingIsComplete;
            table.CallHasBeenMade -= CallHasBeenMade;
            table.CardsHaveBeenDealt -= CardsHaveBeenDealt;
            //table.DealHasBeenAbandoned -= DealHasBeenAbandoned;
            table.DealHasBeenWon -= DealHasBeenWon;
            //table.SessionHasBegun -= SessionHasBegun;
            //table.DealHasBegun -= DealHasBegun;
            table.SessionHasEnded -= SessionHasEnded;
            //table.PlayerHasJoined -= PlayerHasJoined;
            table.PlayerHasQuit -= PlayerHasQuit;
            table.CardHasBeenPlayed -= CardHasBeenPlayed;
            table.TrickHasBeenWon -= TrickHasBeenWon;
            //table.DummyHasExposedHand -= DummyHasExposedHand;
        }

                #region Messages from Table
        //In real AI, I will update the game tree with info from these messages.
        void CardsHaveBeenDealt (object sender, EventArgs e)
        {
            //Debug.Print("{1}: Cards have been dealt. Hand:{0}", ((Table)sender).GetHand(this).PrintFormat(), _mySeat);
            //if (_mySeat == _table.Dealer) _timeToBid = true;
            _tableStateHasChanged = true;
        }


        void CallHasBeenMade (object sender, Table.CallHasBeenMadeEventArgs e)
        {
//            if (e.Call.Bidder == _mySeat.GetRightHandOpponent())
//                _timeToBid = true;
            //Note if the call made completed bidding, I may try to bid (and get an exception) before I get the bidding complete message
            _tableStateHasChanged = true;
         }


        void BiddingIsComplete (object sender, Table.BiddingIsCompleteEventArgs e)
        {
//            _timeToBid = false;
            _manageDummy = e.Declarer == _mySeat;
//            if (e.Declarer == _mySeat.GetRightHandOpponent())
//                _timeToPlay = true;
            _tableStateHasChanged = true;
        }


        void CardHasBeenPlayed (object sender, Table.CardHasBeenPlayedEventArgs e)
        {
//            if (_table.CurrentTrick.Done)
//                return; //wait for TrickHasBeenWon
//
//            if (e.Player == _mySeat.GetRightHandOpponent() && _mySeat != _table.Dummy)
//                _timeToPlay = true;
//
//            if (_manageDummy && e.Player == _table.Dummy.GetRightHandOpponent())
//                _timeToPlayForDummy = true;
            _tableStateHasChanged = true;
        }

        void TrickHasBeenWon (object sender, Table.TrickHasBeenWonEventArgs e)
        {
            //This could cause trouble, because a new deal may already be starting
            // and the old deal is over, but the new deal just began.
            // If I am wrong, then my play will fail, and it will get sorted out when
            //I get the dealHasBeen Won Message
            //May be cleaner for the table to maintain a table state enumeration.

//            if (_table.DealOver)
//                return;
//            if (e.Winner == _mySeat)
//                _timeToPlay = true;
//            if (_manageDummy && e.Winner == _table.Dummy)
//                _timeToPlayForDummy = true;
            _tableStateHasChanged = true;
        }


        void DealHasBeenWon (object sender, Table.DealHasBeenWonEventArgs e)
        {
//            _timeToPlay = false;
//            _timeToPlayForDummy = false;
            _tableStateHasChanged = true;

            //Debug.Print("\n\nGame Over!\n\n");
        }

     
        void SessionHasEnded (object sender, Table.SessionHasEndedEventArgs e)
        {
            _stop = true;
        }


        void PlayerHasQuit (object sender, Table.PlayerHasQuitEventArgs e)
        {
//            _timeToBid = false;
//            _timeToPlay = false;
//            _timeToPlayForDummy = false;
            _tableStateHasChanged = true;
        }

     #endregion
    }
}

