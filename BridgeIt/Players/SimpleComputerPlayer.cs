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


namespace BridgeIt.Players
{
    public class SimpleComputerPlayer : IPlayer
    {
        // Private Data
        Thread _thread;
        bool _stop;
        bool _timeToBid;
        bool _timeToPlay;
        bool _timeToPlayForDummy;
        bool _manageDummy;
        Random r = new Random();
        Seat _mySeat;
        Table _table;

        public Thread Thread
        {
            get { return _thread;}
        }

        public SimpleComputerPlayer ()
        {
        }

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
            catch (Exception ex)
            {
                UnsubscribeToMessages(table);
                throw ex;
            }
            _table = table;
        }


        public void Start ()
        {
            if (_table == null)
                throw new InvalidOperationException("You can't start until you sitting at a table");

            Console.WriteLine("Simple Computer Player started by thread {0} @ {2} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            _thread = new Thread(new ThreadStart(Run));
            _thread.IsBackground = true;
            _thread.Start();
        }


        public void Stop ()
        {
            if (_thread == null)
                throw new InvalidOperationException("This player has not been started");

            if (!_thread.IsAlive)
                throw new InvalidOperationException("This player has not been stopped and cannot be started");

            Console.WriteLine("SCP at seat {2} stopped by thread {0} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            _stop = true;
        }

        #region Methods required for interface
        public void PlayNow (System.TimeSpan timelimit)
        {
            throw new NotImplementedException();
        }
        public void PlayForDummyNow (IPlayer dummy, System.TimeSpan timelimit)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void Run ()
        {
            Console.WriteLine("SCP at seat {2} has entered the run loop on thread {0} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            while (!_stop)
            {
                //Keep the time in DoSomeWork about 100 milleseconds or less for responsibveness
                //Do not let it get too low, or else we will waste a lot of CPU time spinning this loop
                WorkOnGameTree();

                //These flags will be set by the delegates from the table messages
                //these flags are cleared on success, but not on failure in the hopes that I might
                //succeed after working on the game tree some more.
                if (_timeToBid)
                {
                    Call call = GetBestCall();
                    Console.WriteLine("SCP at Seat {0} called: {1}", _mySeat, call);
                    try
                    {
                        _table.MakeCall(this, call);
                        _timeToBid = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Call failed for SCP at Seat {0}: {1}", _mySeat, ex);
                        throw ex;
                    }
                }
                if (_timeToPlay)
                {
                    Card card = GetBestCard();
                    Console.WriteLine("SCP at Seat {0} played {1}", _mySeat, card);
                    try
                    {
                        _table.PlayCard(this, card);
                        _timeToPlay = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Play failed for SCP at Seat {0}: {1}", _mySeat, ex);
                        throw ex;
                    }
                }
                if (_timeToPlayForDummy)
                {
                    Card card = GetBestCard();
                    Console.WriteLine("SCP at Seat {0} played {1} For Dummy", _mySeat, card);
                    try
                    {
                        _table.PlayCard(this, card);
                        _timeToPlayForDummy = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Play failed for SCP at Seat {0}: {1}", _mySeat, ex);
                        throw ex;
                    }
                }
            }
            Console.WriteLine("SCP at seat {2} has left the run loop on thread {0} @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now, _mySeat);
            LeaveTable();
        }

        void LeaveTable ()
        {
             UnsubscribeToMessages(_table);
             _table.Quit(this);
             Console.WriteLine("SCP at Seat {0} quit the table", _mySeat);
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
            return new Call(_mySeat, CallType.Pass);
        }

        Card GetBestCard ()
        {
            //Play highest card from hand in suit that lead
            //otherwise play highest trump
            //otherwise play a random card
            IEnumerable<Card> hand;
            if (_timeToPlayForDummy)
                    hand = _table.DummiesCards;
            else
                    hand = _table.GetHand(this);

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
            Console.WriteLine("{1}: Cards have been dealt. Hand:{0}", ((Table)sender).GetHand(this).PrintFormat(), _mySeat);
            if (_mySeat == _table.Dealer) _timeToBid = true;
        }


        void CallHasBeenMade (object sender, Table.CallHasBeenMadeEventArgs e)
        {
            if (e.Call.Bidder == _mySeat.GetRightHandOpponent())
                _timeToBid = true;
            //Note if the call made completed bidding, I may try to bid (and get an exception) before I get the bidding complete message
         }


        void BiddingIsComplete (object sender, Table.BiddingIsCompleteEventArgs e)
        {
            _timeToBid = false;
            _manageDummy = e.Declarer == _mySeat;
            if (e.Declarer == _mySeat.GetRightHandOpponent())
                _timeToPlay = true;
        }


        void CardHasBeenPlayed (object sender, Table.CardHasBeenPlayedEventArgs e)
        {
            if (_table.CurrentTrick.Done)
                return; //wait for TrickHasBeenWon

            if (e.Player == _mySeat.GetRightHandOpponent())
                _timeToPlay = true;

            if (_manageDummy && e.Player == _table.Dummy.GetRightHandOpponent())
                _timeToPlayForDummy = true;
        }

        void TrickHasBeenWon (object sender, Table.TrickHasBeenWonEventArgs e)
        {
            //This could cause trouble, because a new deal may already be starting
            // and the old deal is over, but the new deal just began.
            // If I am wrong, then my play will fail, and it will get sorted out when
            //I get the dealHasBeen Won Message
            //May be cleaner for the table to maintain a table state enumeration.
            if (_table.DealOver)
                return;
            if (e.Winner == _mySeat)
                _timeToPlay = true;
            if (_manageDummy && e.Winner == _table.Dummy)
                _timeToPlayForDummy = true;
        }


        void DealHasBeenWon (object sender, Table.DealHasBeenWonEventArgs e)
        {
            _timeToPlay = false;
            _timeToPlayForDummy = false;
        }

     
        void SessionHasEnded (object sender, Table.SessionHasEndedEventArgs e)
        {
            _stop = true;
        }


        void PlayerHasQuit (object sender, Table.PlayerHasQuitEventArgs e)
        {
            _timeToBid = false;
            _timeToPlay = false;
            _timeToPlayForDummy = false;
        }

     #endregion
    }
}

