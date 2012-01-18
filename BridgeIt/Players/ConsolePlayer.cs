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
			_table = table;
			SignupForMessages(table);
			_seat = table.SitDown(this);
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
			table.GameHasBeenWon += GameHasBeenWon;
			table.MatchHasBegun += MatchHasBegun;
			table.GameHasBegun += GameHasBegun;
			table.MatchHasBeenWon += MatchHasBeenWon;
			table.PlayerHasJoined += PlayerHasJoined;
			table.PlayerHasQuit += PlayerHasQuit;
			table.CardHasBeenPlayed += CardHasBeenPlayed;
			table.TrickHasBeenWon += TrickHasBeenWon;
		}

		private void UnsubscribeToMessages (Table table)
		{
			table.BiddingIsComplete -= BiddingIsComplete;
			table.CallHasBeenMade -= CallHasBeenMade;
			table.CardsHaveBeenDealt -= CardsHaveBeenDealt;
			table.GameHasBeenWon -= GameHasBeenWon;
			table.MatchHasBegun -= MatchHasBegun;
			table.GameHasBegun -= GameHasBegun;
			table.MatchHasBeenWon -= MatchHasBeenWon;
			table.PlayerHasJoined -= PlayerHasJoined;
			table.PlayerHasQuit -= PlayerHasQuit;
			table.CardHasBeenPlayed -= CardHasBeenPlayed;
			table.TrickHasBeenWon -= TrickHasBeenWon;
		}
		#endregion
		
		#region IPlayer methods
		public void PlaceBid()
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
					_table.Call(this,call);
					badBid = false;
				}
				catch (Exception ex) {
					Out.WriteLine(ex.Message + " Try again.");	
				}
			}
			while (badBid);
		}
		
		public void PlaceBidNow(TimeSpan timelimit)
		{
			Out.WriteLine("{1}: You have {0} seconds to place your bid, or you will forfeit this game/match and your seat at the table", timelimit.Seconds, _seat);
			PlaceBid();
		}
		
		public void Play()
		{
			if (_table == null || _seat == Seat.None)
				throw new InvalidOperationException("You must join a table before playing a card.");
			
			Out.WriteLine("{1}: Your hand is {0}", _table.GetHand(this).PrintFormat(), _seat);
		    bool badCard = true;
			do
			{
				Out.Write("{0}: Enter a card to play :", _seat);
				try {				
					
					Card card = Card.FromString(In.ReadLine());
					_table.Play(this,card);
					badCard = false;
				}
				catch (Exception ex) {
					Out.WriteLine(ex.Message + ", Try again");	
				}
			}
			while (badCard);
		}
		
		public void PlayNow(TimeSpan timelimit)
		{
			Out.WriteLine("{1}: You have {0} seconds to place your bid, or you will forfeit this game/match and your seat at the table", timelimit.Seconds, _seat);
			Play();
		}
		
		public void PlayForDummy(IPlayer dummy)
		{
			if (dummy == null)
				throw new ArgumentNullException("dummy");
					
			if (_table == null || _seat == Seat.None)
				throw new InvalidOperationException("You must join a table before playing a card for dummy.");
			
			Out.WriteLine("Dummies hand is {0}", _table.GetHand(dummy));
		    bool badCard = true;
			do
			{
				Out.WriteLine("Enter a card to play :");
				try {				
					Card card = Card.FromString(In.ReadLine());
					_table.Play(dummy,card);
					badCard = false;
				}
				catch (Exception ex) {
					Out.WriteLine(ex.Message + ", Try again.");	
				}
			}
			while (!badCard);
		}
		
		public void PlayForDummyNow(IPlayer dummy, TimeSpan timelimit)
		{
			Out.WriteLine("{1}: You have {0} seconds to play for dummy, or you will forfeit this game/match and your seat at the table", timelimit.Seconds, _seat);
			PlayForDummy(dummy);
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
		
		void MatchHasBegun (object sender, Table.MatchHasBegunEventArgs e)
		{
			Out.WriteLine("{1}: Match has begun. Dealer:{0}", e.Dealer, _seat);
		}
		
		void GameHasBegun (object sender, Table.GameHasBegunEventArgs e)
		{
			Out.WriteLine("{1}: Game has begun. Dealer:{0}", e.Dealer, _seat);
		}
		
		void CardsHaveBeenDealt (object sender, EventArgs e)
		{
			Out.WriteLine("{1}: Cards have been dealt. Hand:{0}", ((Table)sender).GetHand(this).PrintFormat(), _seat);
		}
		
		void CallHasBeenMade (object sender, Table.CallHasBeenMadeEventArgs e)
		{
			if (e.Call.CallType == CallType.Bid)
				Out.WriteLine("{2}: A bid has been made. Seat:{0}, Bid:{1}", e.Call.Bidder, e.Call.Bid, _seat);
			else
				Out.WriteLine("{2}: A call has been made. Seat:{0}, Call:{1}", e.Call.Bidder, e.Call.CallType, _seat);
		}
		
		void BiddingIsComplete (object sender, Table.BiddingIsCompleteEventArgs e)
		{
			Out.WriteLine("{2}: Bidding is complete. Declarer:{0}, Contract:{1}", e.Declarer, e.Contract, _seat);
		}
		
		void CardHasBeenPlayed(object sender, Table.CardHasBeenPlayedEventArgs e)
		{
			Out.WriteLine("{2}: Card has been played. Player:{0}, Card:{1}", e.Player, e.Card, _seat);
		}
		
		void TrickHasBeenWon(object sender, Table.TrickHasBeenWonEventArgs e)
		{
			Out.WriteLine("{2}: Trick has been won. Winner:{0}, Trick:{1}", e.Winner, e.Trick, _seat);
		}
		
		void GameHasBeenWon (object sender, Table.GameHasBeenWonEventArgs e)
		{
			Out.WriteLine("{2}: Game has been won.  Winning Team:{0}, Score:{1}", e.Winners, e.Score, _seat);
		}
		
		void MatchHasBeenWon (object sender, Table.MatchHasBeenWonEventArgs e)
		{
			Out.WriteLine("{2}: Match has been won.  Winning Team:{0}, Score:{1}", e.Winners, e.Score, _seat);
		}
		
		void PlayerHasQuit(object sender, Table.PlayerHasQuitEventArgs e)
		{
			Out.WriteLine("{1}: A player has quit the game.  Player:{0}", e.Player, _seat);
		}
		
		#endregion
	}
}

