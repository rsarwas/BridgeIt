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

The player is responsible for managing the decision
making of a participant in the game.
The player listens for messages from the table and
acts preforms actions on the table

The player has a User Input object which implements
a (TBD) interface.  The UI object could be the
computer, a terminal console, windowed application
web page, or a network stream to a CRAY supercomputer.

A UI may be a type of player, but I need to switch,
that dummy may be bidding as a computer, but then belongs
to 

The player and the UI are joined when they are created.
The UI get the state of the game for decision making,
or display to a human from the player.  The UI can
preemtively command the player to make an action on
the table, or the player can, in response to an
internal timer, or a reequest from the table, request
that the UI provide a decision.

The UI could begin considering options and building
a play strategy as soon as the hand is received.

Player is an abstract superclass. Possible sub classes: 
  HumanPlayer, ComputerPlayer, TestPlayer
*/

using BridgeIt.Tables;
using BridgeIt.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

//FIXME - allow for q (quit) and ?/h (help) at all request for input

namespace BridgeIt.Players
{
	public class ConsolePlayer : IPlayer
	{
		private Table _table;
		private Seat _seat;
		
		public ConsolePlayer(TextReader input, TextWriter output)
		{
			if (input == null)
				throw new ArgumentNullException("input");
			if (output == null)
				throw new ArgumentNullException("output");
			In = input;
			Out = output;			
		}
		
		public TextReader In {get; set;}
		public TextWriter Out {get; set;}
		
		public void JoinTable (Table table)
		{
			if (table == null)
				throw new ArgumentNullException("table");
			
			try {
				Join(table);
				Out.WriteLine("You have joined the table as {0}", _seat);
			} catch (Exception ex) {
				Out.WriteLine("Unable to join the table. {0}", ex.Message);
				Leave(table);
			}
		}
		
		#region Private Methods
		private void Join (Table table)
		{
			//Once you sit down, table will start sending you messages.
			SignupForMessages(table);
			_seat = table.SitDown(this);
            _table = table;
		}
		
		private void Leave (Table table)
		{
			if (table != _table)
				return; // we are not at this table
			
			//Don't leave while you are still responding to messages.
			UnsubscribeToMessages(table);
			table.Quit(this);
			_table = null;
			_seat = Seat.None;
		}

		private void SignupForMessages (Table table)
        {
            table.BiddingIsComplete += BiddingIsComplete;
            table.CallHasBeenMade += CallHasBeenMade;
            table.CardsHaveBeenDealt += CardsHaveBeenDealt;
            table.DealHasBeenAbandoned += DealHasBeenAbandoned;
            table.DealHasBeenWon += DealHasBeenWon;
            table.SessionHasBegun += SessionHasBegun;
            table.DealHasBegun += DealHasBegun;
            table.SessionHasEnded += SessionHasEnded;
            table.PlayerHasJoined += PlayerHasJoined;
            table.PlayerHasQuit += PlayerHasQuit;
            table.CardHasBeenPlayed += CardHasBeenPlayed;
            table.TrickHasBeenWon += TrickHasBeenWon;
            table.DummyHasExposedHand += DummyHasExposedHand;
        }

		private void UnsubscribeToMessages (Table table)
		{
			table.BiddingIsComplete -= BiddingIsComplete;
			table.CallHasBeenMade -= CallHasBeenMade;
			table.CardsHaveBeenDealt -= CardsHaveBeenDealt;
            table.DealHasBeenAbandoned -= DealHasBeenAbandoned;
			table.DealHasBeenWon -= DealHasBeenWon;
			table.SessionHasBegun -= SessionHasBegun;
			table.DealHasBegun -= DealHasBegun;
			table.SessionHasEnded -= SessionHasEnded;
			table.PlayerHasJoined -= PlayerHasJoined;
			table.PlayerHasQuit -= PlayerHasQuit;
			table.CardHasBeenPlayed -= CardHasBeenPlayed;
			table.TrickHasBeenWon -= TrickHasBeenWon;
            table.DummyHasExposedHand -= DummyHasExposedHand;
		}
		#endregion
		
		#region IPlayer methods
		private void PlaceBid()
		{
			if (_table == null || _seat == Seat.None)
				throw new InvalidOperationException("You must join a table before placing a bid.");
			
			Out.WriteLine("{1}: Your hand is {0}", _table.GetHand(this).PrintFormat(), _seat);
			bool badBid = true;
			do
			{
				Out.Write("{0}: Enter your Bid :", _seat);
				try {				
					Call call = Call.FromString(_seat, In.ReadLine());
					_table.MakeCall(this,call);
					badBid = false;
				}
				catch (Exception ex) {
					Out.WriteLine(ex.Message + "  Try again.");
				}
			}
			while (badBid);
		}
		
		public void PlaceBidNow(TimeSpan timelimit)
		{
			Out.WriteLine("{1}: You have {0} seconds to place your bid, or you will forfeit this hand/game and your seat at the table", timelimit.Seconds, _seat);
			PlaceBid();
		}
		
		private void Play()
		{
			if (_table == null || _seat == Seat.None)
				throw new InvalidOperationException("You must join a table before playing a card.");
			
			Out.WriteLine("{1}: Your hand is {0}", _table.GetHand(this).PrintFormat(), _seat);
		    bool badCard = true;
			do
			{
                //Out.Write("{0}: Enter a card to play :", _seat);
                Out.Write("" +
                 "   Enter a card to play :", _seat);
				try {
					
					Card card = Card.FromString(In.ReadLine());
					_table.PlayCard(this,card);
					badCard = false;
				}
				catch (Exception ex) {
					Out.WriteLine(ex.Message + "  Try again.");
				}
			}
			while (badCard);
		}
		
		public void PlayNow(TimeSpan timelimit)
		{
			Out.WriteLine("{1}: You have {0} seconds to place your bid, or you will forfeit this hand/game and your seat at the table", timelimit.Seconds, _seat);
			Play();
		}
		
		private void PlayForDummy()
		{
			if (_table == null || _seat == Seat.None)
				throw new InvalidOperationException("You must join a table before playing a card for dummy.");
			
			Out.WriteLine("{1}: Dummies hand is {0}", _table.DummiesCards.PrintFormat(), _seat);
		    bool badCard = true;
			do
			{
				Out.Write("Enter a card to play :");
				try {				
					Card card = Card.FromString(In.ReadLine());
					_table.PlayCard(this,card);
					badCard = false;
				}
				catch (Exception ex) {
					Out.WriteLine(ex.Message + "  Try again.");
				}
			}
			while (badCard);
		}
		
		public void PlayForDummyNow(TimeSpan timelimit)
		{
			Out.WriteLine("{1}: You have {0} seconds to play for dummy, or you will forfeit this hand/game and your seat at the table", timelimit.Seconds, _seat);
			PlayForDummy();
		}
		#endregion
		
		#region Messages from Table
		
		void PlayerHasJoined(object sender, Table.PlayerHasJoinedEventArgs e)
		{
			//This message will be sent to us when we are joining the table
			//We may or may not have our seat assingment yet.  Ignore it either way.

			if (_seat == Seat.None || _seat == e.Player)
				return;
			Out.WriteLine("{1}: A player has joined the game.  Player:{0}", e.Player, _seat);
		}
		
		void SessionHasBegun (object sender, Table.SessionHasBegunEventArgs e)
		{
			Out.WriteLine("{1}: Session has begun. Dealer:{0}", e.Dealer, _seat);
		}
		
		void DealHasBegun (object sender, Table.DealHasBegunEventArgs e)
		{
			Out.WriteLine("{1}: Deal has begun. Dealer:{0}", e.Dealer, _seat);
		}
		
        void CardsHaveBeenDealt (object sender, EventArgs e)
         {
            var table = (Table)sender;
            Out.WriteLine("{1}: Cards have been dealt. Hand:{0}", table.GetHand(this).PrintFormat(), _seat);
            if (table.AllowingBidFrom(_seat))
                PlaceBid();
         }

        void DealHasBeenAbandoned (object sender, EventArgs e)
         {
             Out.WriteLine("{0}: Deal has been Abandoned", _seat);
         }

		void CallHasBeenMade (object sender, Table.CallHasBeenMadeEventArgs e)
		{
            if (e.Call.Bidder == _seat)
                return;
            if (e.Call.CallType == CallType.Bid)
                //Out.WriteLine("{2}: A bid has been made. Seat:{0}, Bid:{1}", e.Call.Bidder, e.Call.Bid, _seat);
                Out.WriteLine("{0} bid: {1}", e.Call.Bidder, e.Call.Bid, _seat);
			else
                //Out.WriteLine("{2}: A call has been made. Seat:{0}, Call:{1}", e.Call.Bidder, e.Call.CallType, _seat);
                Out.WriteLine("{0}: {1}", e.Call.Bidder, e.Call.CallType, _seat);
            var table = (Table)sender;
            if (table.AllowingBidFrom(_seat))
                PlaceBid();
		}
		
		void BiddingIsComplete (object sender, Table.BiddingIsCompleteEventArgs e)
		{
            //Out.WriteLine("{2}: Bidding is complete. Declarer:{0}, Contract:{1}", e.Declarer, e.Contract, _seat);
            Out.WriteLine("Bidding is complete: {1} by {0}", e.Declarer, e.Contract, _seat);
            var table = (Table)sender;
            if (table.AllowingCardFrom(_seat))
                Play();
            if (table.AllowingCardFromDummyBy(_seat))
                PlayForDummy();
            cards.Clear();
		}

        private readonly List<Card> cards = new List<Card>(4);

        void CardHasBeenPlayed (object sender, Table.CardHasBeenPlayedEventArgs e)
        {
            //Out.WriteLine("{2}: Card has been played. Player:{0}, Card:{1}", e.Player, e.Card, _seat);
            cards.Add(e.Card);
            if (cards.Count == 1)
            Out.WriteLine("{0} Lead {1}", e.Player, e.Card.ToGlyphString(), _seat);
else
            Out.WriteLine("{0} played {1} - Trick: {2}", e.Player, e.Card.ToGlyphString(), cards.Print());

            var table = (Table)sender;
            if (table.AllowingCardFrom(_seat))
                Play();
            if (table.AllowingCardFromDummyBy(_seat))
                PlayForDummy();
        }


        void DummyHasExposedHand (object sender, Table.DummyHasExposedHandEventArgs e)
        {
            //Out.WriteLine("{1}: Dummies cards have been shown, they are:{0}", e.DummiesCards.PrintFormat(), _seat);
            Out.WriteLine("Dummies cards:{0}", e.DummiesCards.PrintFormat(), _seat);
        }


		void TrickHasBeenWon(object sender, Table.TrickHasBeenWonEventArgs e)
		{
            //Out.WriteLine("{2}: Trick has been won. Winner:{0}, Trick:{1}", e.Winner, e.Trick, _seat);
            Out.WriteLine("{0} won trick {1}", e.Winner, e.Trick, _seat);
            cards.Clear();
		}
		
		void DealHasBeenWon (object sender, Table.DealHasBeenWonEventArgs e)
		{
			Out.WriteLine("Deal is finished.  The score is:\n{1}", e.Score);
		}
		
		void SessionHasEnded (object sender, Table.SessionHasEndedEventArgs e)
		{
			Out.WriteLine("{Session has been won.  Winning Team:{0}, Score:{1}", e.Winners, e.Score);
		}
		
		void PlayerHasQuit(object sender, Table.PlayerHasQuitEventArgs e)
		{
			Out.WriteLine("{0} has left the table and quit the game.", e.Player);
		}
		
		#endregion
	}
}

