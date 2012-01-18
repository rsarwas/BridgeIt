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
Table Definition
Table holds the current state of the game for player inspection
Table does not provide (or maintain?) history of the state (a player
must save that themselves if they want to use it for decision making)

There is typically only one table, a game happens on a table.

The table is responsible for enforcing the rules of play.

Players interact with the table, not other players,
they receive messages from the table not other players.
The table should never provide a player with a reference
to another player, except when it provides the winning
declarer with a reference to dummy.  Then declarer can
then see dummies cards and play on behalf of dummy.

Seat (East, etc) is a name of a player, not an actual player.
The player objects are private to the creator and table,
this provides protection of the player's hand (and other state)
from prying eyes, and authenticates a Bid/Play, so that one
player cannot illegally pose as another.
The table uses the player object for authentication of actions,
and distributing messages.
The table does not change any player state (by convention).  


Key actions a player can make on a table:
  SitDown(aPlayer) => Seat
  Bid(aPlayer,aBid)
  Play(aPlayer,aCard)
  Quit(aPlayer)


Key messages a table will send to a player:
  Commands (methods that player must implement):
    Bid
    BidNow
    Play
    PlayNow
    PlayForDummy
    PlayForDummyNow
    
  Notifications (events):
    PlayerHasJoined => Seat
    TableIsFull
    CardsHaveBeenDealt => Hand
    TimeIsUp
    CallHasBeenMade => Seat,Bid
    BiddingIsComplete => Seat,Contract
    PartnerIsDummy => Dummy (a player not a seat)
    CardHasBeenPlayed => Seat,Card
    TrickHasBeenWon => Player,Trick
    PlayerHasQuit => Seat
    GameOver => WinningTeam,Score

State available to the public (all is readonly)
  Seat Declarer
  Suit Trump
  Contract Contract
  Trick CurrentTrick
  Bid CurrentBid
  Seat ActivePlayer
  State?
  Status? Empty, partially full, full, dealing, bidding, playing

State available to a player (all is readonly)
  Hand PlayersHand

The UI (whatever it is), owns one (or more) player(s), and
sees the world as the player sees it.  See Player

There is also needs to be an ability to send other
events to the from the human users.


Players and the table can be created on different threads
Since each player in a game references the same table, the
table must provide locking when changing state, or making
decisions about changing state. 

*/

using BridgeIt.Core;
using System;
using System.Collections.Generic;

namespace BridgeIt.Tables
{
	public class Table : ITable
	{
		
		public static readonly Seat[] Seats = new [] {Seat.South, Seat.West, Seat.North, Seat.East};
		
		private Dictionary<Seat,IPlayer> _players = new Dictionary<Seat, IPlayer>(Seats.Length);
		private Dictionary<IPlayer,Seat> _seats = new Dictionary<IPlayer, Seat>(Seats.Length);
		private Queue<Seat> _openSeats = new Queue<Seat>(Seats);
		private Dictionary<IPlayer, Hand> _hands = new Dictionary<IPlayer, Hand>();
		private int _passCount;
		
		public Table()
		{
			ResetState ();
		}

		//TODO - Consider using an interface or inheritance that the Player references??
		//     - may be nice if I want to have different tables with different rules ??
		
		//What should the value types return when they are "not set"
		public Seat Dealer {get; private set;}
		public Seat Declarer {get; private set;}
		public Seat Dummy {get; private set;}
		public Seat ActivePlayer {get; private set;}
		public Suit Trump {get; private set;}
		public Bid CurrentBid {get; private set;}
		public Bid Contract {get; private set;}
		public Trick CurrentTrick { get; private set; }
		public IEnumerable<Card> CardsOnTheTable { get; private set; }
		public IEnumerable<Call> LastThreeCalls {get; private set;}
		//public State? {get; private set;}
		//public Status? Empty, partially full, full, dealing, bidding, playing
		
		private void ResetState()
		{
			Dealer = Seat.None;
			Declarer = Seat.None;
			Dummy = Seat.None;
			ActivePlayer = Seat.None;
			//Trump = undefined
			CurrentBid = null;
			_passCount = 0;
			//Contract = undefined bid plus possible double??
			CurrentTrick = null;
		}
		
		#region Interface Methods that IPlayer is expecting
  		public IEnumerable<Card> GetHand(IPlayer player)
		{
			if (!_seats.ContainsKey(player))
				throw new Exception("Player is not sitting at this table.");
			return _hands[player].Cards;
		}

		public Seat SitDown (IPlayer player)
		{
			//FIXME - add locking
			if (_openSeats.Count == 0)
				throw new Exception("Table is full");
			if (_seats.ContainsKey(player))
				throw new Exception("Player is already seated at the table.");
			Seat seat = AddPlayer(player);
			OnPlayerHasJoined(new PlayerHasJoinedEventArgs(seat));
			return seat;
		}
		
		public void Start (Seat dealer = Seat.South)
		{
			if (_openSeats.Count > 0)
				throw new Exception("Table is not full");
			
			ResetState();
			Dealer = dealer;
			ActivePlayer = dealer;
			OnMatchHasBegun(new Table.MatchHasBegunEventArgs(dealer));
			OnGameHasBegun(new Table.GameHasBegunEventArgs(dealer));
			Deck deck = new Deck();
			deck.Shuffle();
			Deal(deck);
			OnCardsHaveBeenDealt();
			_players[dealer].PlaceBid();
		}

		public void Call(IPlayer player, Call call)
		{
			if (!_seats.ContainsKey(player))
				throw new Exception("Player is not seated at the table");
			
			IPlayer expectedPlayer = _players[ActivePlayer];
			if (player != expectedPlayer)
				throw new Exception("It is not players turn to make a bid");
			
			if (call == null)
				throw new ArgumentNullException("call");
			if (call.Bidder != ActivePlayer)
				throw new Exception("You cannot make a bid for another player.");
			
			switch (call.CallType) {
			case CallType.Pass:
				_passCount++;
				break;
			case CallType.Double:
			break;
			case CallType.ReDouble:
			break;
			case CallType.Bid:
				//if (call
				
				break;
			default:
				throw new ArgumentException("CallType '"+call.CallType+"' not supported.");
			}
			
			//Add to bid list
			OnCallHasBeenMade(new Table.CallHasBeenMadeEventArgs(call));
			//if four passes, then abort the game and start a new game
			//
			//else if three passes with a bid then goto play mode
			//else get next bid
			ActivePlayer = NextSeat(ActivePlayer);
			_players[ActivePlayer].PlaceBid();
		}
		
		public void Play (IPlayer player, Card card)
		{
			throw new NotImplementedException();
		}

		public void Quit (IPlayer player)
		{
			//ignore players who are not at the table
			if (!_seats.ContainsKey(player))
				return;
			
			Seat seat = _seats[player];
			RemoveSeat(seat);
			OnPlayerHasQuit(new PlayerHasQuitEventArgs(seat));
			//FIXME - pause or abort the current game
			return;
		}
		
		public Seat NextSeat (Seat seat)
		{
			//Seat 0 is reserved for the null seat
			seat++;
			if ((int)seat > Seats.Length)
				seat = (Seat)1;
			return seat;
		}
		#endregion

		private Seat AddPlayer (IPlayer player)
		{
			Seat seat = _openSeats.Dequeue();
			_players.Add(seat, player);
			_seats.Add(player, seat);
			return seat;
		}

		private void RemoveSeat (Seat seat)
		{
			_seats.Remove(_players[seat]);
			_players.Remove(seat);
			_openSeats.Enqueue(seat);
		}
		
		private void RemovePlayer (IPlayer player)
		{
			Seat seat = _seats[player];
			_players.Remove(seat);
			_seats.Remove(player);
			_openSeats.Enqueue(seat);
		}
		
		private void Deal (Deck deck)
		{
			Dictionary<Seat, List<Card>> hands = new Dictionary<Seat, List<Card>>();
			foreach (var place in Seats)
				hands[place] = new List<Card>(Hand.MaxSize);
			
			Seat seat = ActivePlayer; //Active player is the dealer when dealing
			foreach (Card card in deck.GetCards()) {
				seat = NextSeat(seat);
				hands[seat].Add(card);
			}
			foreach (var place in Seats)
				_hands[_players[place]] = new Hand(hands[place]);
		}
		
		#region Events

		#region PlayerHasJoined Event
		public class PlayerHasJoinedEventArgs : System.EventArgs
		{
		    public PlayerHasJoinedEventArgs(Seat seat)
		    {
		        Player = seat;
		    }

		    public Seat Player { get; private set; }
		}

		public event EventHandler<PlayerHasJoinedEventArgs> PlayerHasJoined;

		protected virtual void OnPlayerHasJoined(PlayerHasJoinedEventArgs e)
		{
			var handler = PlayerHasJoined;
			if (handler != null)
			    handler(this, e);
		}
		#endregion
		
		#region MatchHasBegun Event
		public class MatchHasBegunEventArgs : System.EventArgs
		{
		    public MatchHasBegunEventArgs(Seat dealer)
		    {
		        Dealer = dealer;
		    }

		    public Seat Dealer { get; private set; }
		}

		public event EventHandler<MatchHasBegunEventArgs> MatchHasBegun;

		protected virtual void OnMatchHasBegun(MatchHasBegunEventArgs e)
		{
			var handler = MatchHasBegun;
			if (handler != null)
			    handler(this, e);
		}
		#endregion
		
		#region GameHasBegun Event
		public class GameHasBegunEventArgs : System.EventArgs
		{
		    public GameHasBegunEventArgs(Seat dealer)
		    {
		        Dealer = dealer;
		    }

		    public Seat Dealer { get; private set; }
		}

		public event EventHandler<GameHasBegunEventArgs> GameHasBegun;

		protected virtual void OnGameHasBegun(GameHasBegunEventArgs e)
		{
			var handler = GameHasBegun;
			if (handler != null)
			    handler(this, e);
		}
		#endregion
		
		#region CardsHaveBeenDealt Event
		public event EventHandler<EventArgs> CardsHaveBeenDealt;

		protected virtual void OnCardsHaveBeenDealt()
		{
			var handler = CardsHaveBeenDealt;
			if (handler != null)
			    handler(this, new EventArgs());
		}
		#endregion
		
		#region CallHasBeenMade Event
		public class CallHasBeenMadeEventArgs : System.EventArgs
		{
			public CallHasBeenMadeEventArgs(Call call)
			{
				Call = call;
			}
			
			public Call Call { get; private set; }
		}

		public event EventHandler<CallHasBeenMadeEventArgs> CallHasBeenMade;

		protected virtual void OnCallHasBeenMade(CallHasBeenMadeEventArgs e)
		{
			var handler = CallHasBeenMade;
			if (handler != null)
				handler(this, e);
		}
		#endregion
		
		#region BiddingIsComplete Event
		public class BiddingIsCompleteEventArgs : System.EventArgs
		{
			public BiddingIsCompleteEventArgs(Seat seat, Bid contract)
			{
				Declarer = seat;
				Contract = contract;
			}
			
			public Seat Declarer { get; private set; }
			public Bid Contract { get; private set; }
		}

		public event EventHandler<BiddingIsCompleteEventArgs> BiddingIsComplete;

		protected virtual void OnBiddingIsComplete(BiddingIsCompleteEventArgs e)
		{
			var handler = BiddingIsComplete;
			if (handler != null)
				handler(this, e);
		}
		#endregion
		
		#region CardHasBeenPlayed Event
		public class CardHasBeenPlayedEventArgs : System.EventArgs
		{
			public CardHasBeenPlayedEventArgs(Seat player, Card card)
			{
				Player = player;
				Card = card;
			}
			
			public Seat Player { get; private set; }
			public Card Card { get; private set; }
		}

		public event EventHandler<CardHasBeenPlayedEventArgs> CardHasBeenPlayed;

		protected virtual void OnCardHasBeenPlayed(CardHasBeenPlayedEventArgs e)
		{
			var handler = CardHasBeenPlayed;
			if (handler != null)
				handler(this, e);
		}
		#endregion
		
		#region TrickHasBeenWon Event
		public class TrickHasBeenWonEventArgs : System.EventArgs
		{
			public TrickHasBeenWonEventArgs(Seat player, Trick trick)
			{
				Winner = player;
				Trick = trick;
			}
			
			public Seat Winner { get; private set; }
			public Trick Trick { get; private set; }
		}

		public event EventHandler<TrickHasBeenWonEventArgs> TrickHasBeenWon;

		protected virtual void OnTrickHasBeenWon(TrickHasBeenWonEventArgs e)
		{
			var handler = TrickHasBeenWon;
			if (handler != null)
				handler(this, e);
		}
		#endregion
		
		#region PlayerHasQuit Event
		public class PlayerHasQuitEventArgs : System.EventArgs
		{
		    public PlayerHasQuitEventArgs(Seat player)
		    {
		        Player = player;
		    }

		    public Seat Player { get; private set; }
		}

		public event EventHandler<PlayerHasQuitEventArgs> PlayerHasQuit;

		protected virtual void OnPlayerHasQuit(PlayerHasQuitEventArgs e)
		{
			var handler = PlayerHasQuit;
			if (handler != null)
			    handler(this, e);
		}
		#endregion
		
		#region GameHasBeenWon Event
		public class GameHasBeenWonEventArgs : System.EventArgs
		{
			public GameHasBeenWonEventArgs(Team team, Score score)
			{
				Winners = team;
				Score = score;
			}
			
			public Team Winners { get; private set; }
			public Score Score { get; private set; }
		}

		public event EventHandler<GameHasBeenWonEventArgs> GameHasBeenWon;

		protected virtual void OnGameHasBeenWon(GameHasBeenWonEventArgs e)
		{
			var handler = GameHasBeenWon;
			if (handler != null)
				handler(this, e);
		}
		#endregion
		
		#region MatchHasBeenWon Event
		public class MatchHasBeenWonEventArgs : System.EventArgs
		{
			public MatchHasBeenWonEventArgs(Team team, Score score)
			{
				Winners = team;
				Score = score;
			}
			
			public Team Winners { get; private set; }
			public Score Score { get; private set; }
		}

		public event EventHandler<MatchHasBeenWonEventArgs> MatchHasBeenWon;

		protected virtual void OnMatchHasBeenWon(MatchHasBeenWonEventArgs e)
		{
			var handler = MatchHasBeenWon;
			if (handler != null)
				handler(this, e);
		}
		#endregion
		
		#endregion		
	}
}