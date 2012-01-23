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
    DealOver => WinningTeam,Score

State available to the public (all is readonly)
  Seat Declarer
  Suit Trump
  Contract Contract
  Trick CurrentTrick
  Bid CurrentBid
  Seat HotSeat
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

/* Locking Strategy
   The table maintains several peices of inter-related state, and provides a few public methods
   for changing that state.  None of the state can be changed except through these public
   methods. And state provided through properties or return values is either immutable, or
   a copy.
   The table needs to assume that it's methods may be called by "independent" players.  These could
   humans or computers operating in thier own separate world, or AI threads in this application,
   pondering the next move while the UI is waiting for the human play to make his move.
   I cannot protect just the writes, because a decision to make allow a write may be based
   on a reading of the state that may change (by another thread on another public method)
   between reading and writing.
   To avoid teasing out the inter-related reads/writes, it is much simpler to wrap all public
   method in the same lock - the brute force approach.  This should work reasonably well, because
   none of the methods will take long to complete, and the nature of the play is linear.
   I.e. If a player wants to play a card, they need to wait for the previous player to play thier cards.
 */

/* Notes on the laws
   Law 7 - Change of Pack
   The table should have two decks of cards, and alternate these decks
   Our decks are a only good for one deal, so a new pack is se change
   packs on every deal instead of alternating
 */

using BridgeIt.Core;
using System;
using System.Linq;
using System.Collections.Generic;

//Table does not run in its own thread.  It only runs on a players thread when
//when one of the public methods/properties are called.  All messages sent from
//table are non-blocking async messages, so the player initiating the action is
//not blocked if one of the subscribers initiates a long running activity.

//TODO: Add a timer with stop/start method (on a new thread) to periodically poke
//players when it is there turn to bid/play, or to enforce time limits

namespace BridgeIt.Tables
{
	public class Table : ITable
	{

        public static readonly Seat[] Seats = new [] {Seat.South, Seat.West, Seat.North, Seat.East};

        private readonly object _tableLock = new object();

		private readonly Dictionary<Seat,IPlayer> _players = new Dictionary<Seat, IPlayer>(Seats.Length);
		private readonly Dictionary<IPlayer,Seat> _seats = new Dictionary<IPlayer, Seat>(Seats.Length);
        private readonly Dictionary<IPlayer, Hand> _hands = new Dictionary<IPlayer, Hand>(Seats.Length);
        private readonly Queue<Seat> _openSeats = new Queue<Seat>(Seats);
        private readonly List<Call> _calls = new List<Call>(4);
        private readonly List<Trick> _tricks = new List<Trick>(Deck.Size / Seats.Length);

		
		public Table()
		{
			//TODO - isn't this redundant if I am using logical init values
            ResetSession ();
		}

		public Seat Dealer {get; private set;}
		public Seat Declarer {get; private set;}
		public Seat Dummy {get; private set;}
		public Seat HotSeat {get; private set;}
		public Suit Trump {get; private set;}
		public Contract Contract {get; private set;}
		//public State? {get; private set;}
		//public Status? Empty, partially full, full, dealing, bidding, playing

        //Law 66 - Inspection of Tricks
        //Declarer or either defender may, until a member of his side has
        //led or played to the following trick, inspect a trick and inquire
        //what card each player has played to it. Thereafter, until play ceases,
        //quitted tricks may be inspected only to account for a missing or surplus card.
        //FIXME - CurrentTrick is hidden before the next trick begins.
        public Trick CurrentTrick
        {
            get { return _tricks.LastOrDefault(); }
        }

        public Call CurrentCall
        {
            get { return _calls.LastOrDefault(); }
        }


        public IEnumerable<Card> CardsOnTheTable
        {
            get
            {
                var trick = CurrentTrick;
                if (trick == null)
                    return new Card[0];
                return trick.Cards;
            }
        }


        public IEnumerable<Card> DummiesCards
        {
            get
            {
                if (_tricks.Count > 1 || (_tricks.Count == 1 && CurrentTrick.Cards.Any()))
                    return _hands[_players[Dummy]].Cards;
                return new Card[0];
            }
        }


        public IEnumerable<Call> LastThreeCalls
        {
            get { return _calls.Last(3);}
        }

        //TODO - Law41 - Declarer, before making any play, or either defender,
        // at his first turn to play, may require a restatement of the auction
        // in its entirety.

		#region Interface Methods that IPlayer is expecting

  		public IEnumerable<Card> GetHand (IPlayer player)
        {
            //by Law 8, a player should not (but can) look at his cards before the deal is complete.
            //A player is not entitled to a redeal if they have looked at their unfinished hand.
            //redeal are not going to happen, because we always deal fairly.
            //Therefore a play can look at his cards without consequence.
            try
            {
                return _hands[player].Cards;
            }
            catch (KeyNotFoundException ex)
            {
                throw new Exception("Player is not sitting at this table.", ex);
            }
        }

        //FIXME - allow the player to choose his seat
        //        will need a property to see the open seats.
		public Seat SitDown (IPlayer player)
        {
            lock (_tableLock)
            {
                if (_openSeats.Count == 0)
                    throw new Exception("Table is full");
                if (_seats.ContainsKey(player))
                    throw new Exception("Player is already seated at the table.");
                Seat seat = AddPlayer(player);
                OnPlayerHasJoined(new PlayerHasJoinedEventArgs(seat));
                return seat;
            }
		}
		
        public void StartSession (Seat dealer = Seat.South)
        {
            lock (_tableLock)
            {
                if (_openSeats.Count > 0)
                    throw new Exception("Table is not full");

                //TODO - do we need to send the dealer here?, we send it at the start of the game.
                //Is there anything else we should announce at the start of a sesssion?
                OnSessionHasBegun(new SessionHasBegunEventArgs(dealer));
                StartDeal(dealer);
            }
        }


        private void StartDeal (Seat dealer)
        {
            ResetDeal(dealer);
            OnDealHasBegun(new DealHasBegunEventArgs(dealer));
            var deck = new Deck();
            deck.Shuffle();
            Deal(deck);
            OnCardsHaveBeenDealt();
            _players[dealer].PlaceBid();
        }


		public void MakeCall (IPlayer player, Call call)
        {
            lock (_tableLock)
            {
                if (player == null)
                    throw new ArgumentNullException("player");

                if (call == null)
                    throw new ArgumentNullException("call");

                if (!_seats.ContainsKey(player))
                    throw new CallException("Player is not seated at the table");
			
                if (player != _players[HotSeat])
                    throw new CallException("It is not players turn to make a bid");
			
                if (call.Bidder != HotSeat)
                    throw new CallException("You cannot make a bid for another player.");

                if (Contract != null)
                    throw new CallException("You cannot bid once a contract has been established.");

                if (!_calls.IsCallLegal(call))
                {
                    switch (call.CallType)
                    {
                        case CallType.Redouble:
                            throw new CallException("Cannot redouble without a prior bid having been doubled by opponent.");
                        case CallType.Double:
                            throw new CallException("Cannot double without a prior bid by opponent.");
                        case CallType.Bid:
                            throw new CallException("New bid must be higher than previous bid.");
                        case CallType.Pass:
                            throw new Exception("WTF. A pass call is always legal.");
                        default:
                            throw new ArgumentException("Call type '" + call.CallType + "' not recognized.");
                    }
                }

                //Call is legal
                _calls.Add(call);
                OnCallHasBeenMade(new CallHasBeenMadeEventArgs(call));

                //If the first four calls are all pass then abort
                if (_calls.AreLastFourPasses())
                {
                    AbandonDeal();
                    return;
                }

                //else if three passes with a bid then goto play mode
                //Even with a maximum bid (7NT), we need to wait for 3 passes, due to potential for doubling
                if (_calls.HasBidAndLastThreeArePasses())
                {
                    EnterPlayPhase();
                    return;
                }

                //We aren't done yet, request the next bid
                HotSeat = NextSeat(HotSeat);
                //_players[HotSeat].PlaceBid();
            }
        }

        //player unique identifies and validates the entity making the play
        //if it is dummy, it must be rejected.  If it is Declarer, then it
        //is valid when it is dummy or declareres turn.
        public void PlayCard (IPlayer player, Card card)
        {
            lock (_tableLock)
            {
                if (!_seats.ContainsKey(player))
                    throw new PlayException("Player is not seated at the table.");

                if (Contract == null || CurrentTrick == null)
                    throw new PlayException("You cannot play a card until bidding is done.");

                if (_seats[player] == Dummy)
                    throw new PlayException("Dummy is not allowed to play his cards.");

                bool correctPlayer = player == _players[HotSeat] ||
                                     (HotSeat == Dummy && player == _players[Declarer]);
                if (!correctPlayer)
                    throw new PlayException("It is not your turn to play a card.");

                //Make sure I get Dummies hand when appropriate
                Hand hand = _hands[_players[HotSeat]];
                if (hand.Count == 0)
                    throw new PlayException("You have no cards to play.");

                if (!hand.Contains(card))
                    throw new PlayException("The " + card + " is not yours to play.");

                if (!CurrentTrick.IsLegalPlay(card, hand))
                {
                    if (CurrentTrick.Done)
                        throw new PlayException("The current trick is finished.");
                    else
                        throw new PlayException(CurrentTrick.Suit + " were lead. You can and must follow suit.");
                }

                //We have a legal card from the correct player
                hand.Remove(card);
                CurrentTrick.AddCard(card, HotSeat);
                OnCardHasBeenPlayed(new Table.CardHasBeenPlayedEventArgs(HotSeat, card));
                if (IsOpeningLead())
                {
                    OnDummyHasExposedHand(new Table.DummyHasExposedHandEventArgs(DummiesCards));
                }
                if (CurrentTrick.Done)
                {
                    OnTrickHasBeenWon(new Table.TrickHasBeenWonEventArgs(CurrentTrick.Winner, CurrentTrick));
                    if (_tricks.Count == 13)
                    {
                        FinishDeal();
                    }
                    else
                    {
                        // Law 44 - "The player who has won the trick leads to the next trick."
                        HotSeat = CurrentTrick.Winner;
                        _tricks.Add(new Trick(Trump));
                        //_players[HotSeat].Play();
                    }
                }
                else
                {
                    HotSeat = NextSeat(HotSeat);
//                    if (HotSeat == Dummy)
//                        _players[Declarer].PlayForDummy(_players[Dummy]);
//                    else
//                        _players[HotSeat].Play();
                }
            }
        }

        private bool IsOpeningLead ()
        {
            return _tricks.Count == 1 && CurrentTrick.Cards.Count() == 1;
        }

		public void Quit (IPlayer player)
        {
            lock (_tableLock)
            {
                //ignore players who are not at the table
                if (!_seats.ContainsKey(player))
                    return;
			
                Seat seat = _seats[player];
                RemovePlayer(player);
                OnPlayerHasQuit(new PlayerHasQuitEventArgs(seat));
                //FIXME - pause or abort the current game
                AbandonDeal ();
            }
		}
		
		#endregion

        private void ResetSession ()
        {
            ResetDeal();
        }


        private void ResetDeal (Seat dealer = Seat.None)
        {
            Dealer = dealer;
            Declarer = Seat.None;
            Dummy = Seat.None;
            HotSeat = Dealer;
            Trump = Suit.None;
            Contract = null;
            _calls.Clear();
            _tricks.Clear();
        }

        private Seat GetDeclarer (IEnumerable<Call> calls, Contract contract)
        {
            Suit winningSuit = contract.Bid.Suit;
            Side winningSide = calls.LastBidder().GetSide();
            Call firstCall = calls.First(CallType.Bid, winningSuit, winningSide);
            return firstCall.Bidder;
        }


        private static Seat NextSeat (Seat seat)
        {
            //Seat 0 is reserved for the null seat
            seat++;
            if ((int)seat > Seats.Length)
                seat = (Seat)1;
            return seat;
        }

        private void AbandonDeal ()
        {
            OnDealHasBeenAbandoned();
            StartDeal(NextSeat(Dealer));
        }

        private void AbandonSession ()
        {
            OnDealHasBeenAbandoned();
            OnSessionHasEnded(new Table.SessionHasEndedEventArgs(Side.None, new Score()));
            ResetSession();
        }


        private void FinishDeal ()
        {
            //Fixme - finish
            //determine winning team, and score
            OnDealHasBeenWon(new Table.DealHasBeenWonEventArgs(Side.None, new Score()));
            if (true)
                StartDeal(NextSeat(Dealer));
            else
                FinishSession ();
        }


        private void FinishSession ()
        {
            //Fixme - finish
            OnSessionHasEnded(new Table.SessionHasEndedEventArgs(Side.None, new Score()));
            ResetSession();
        }


        private void EnterPlayPhase ()
        {
            Contract = new Contract(_calls.LastBid(), _calls.GetDoubles());
            Declarer = GetDeclarer(_calls, Contract);
            Trump = Contract.Bid.Suit;
            OnBiddingIsComplete(new Table.BiddingIsCompleteEventArgs(Declarer, Contract));
            HotSeat = NextSeat(Declarer);
            Dummy = NextSeat(HotSeat);
            _calls.Clear();
            _tricks.Add(new Trick(Trump));
            //_players[HotSeat].Play();
        }


		private Seat AddPlayer (IPlayer player)
		{
			Seat seat = _openSeats.Dequeue();
			_players.Add(seat, player);
			_seats.Add(player, seat);
			return seat;
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
			var hands = new Dictionary<Seat, List<Card>>();
			foreach (var place in Seats)
				hands[place] = new List<Card>(Hand.MaxSize);
			
			Seat seat = HotSeat; //Active player is the dealer when dealing
			foreach (Card card in deck.GetCards()) {
				seat = NextSeat(seat);
				hands[seat].Add(card);
			}
			foreach (var place in Seats)
				_hands[_players[place]] = new Hand(hands[place]);
		}
		
		#region Events

        //All of these are Fire and Forget messages
        //They use BeginInvoke on the delegate without a callback function.
        //This guards against a long running mis-behaving delegate blocking.

		#region PlayerHasJoined Event
		public class PlayerHasJoinedEventArgs : EventArgs
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
			    handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
		#region SessionHasBegun Event
		public class SessionHasBegunEventArgs : EventArgs
		{
		    public SessionHasBegunEventArgs(Seat dealer)
		    {
		        Dealer = dealer;
		    }

		    public Seat Dealer { get; private set; }
		}

		public event EventHandler<SessionHasBegunEventArgs> SessionHasBegun;

		protected virtual void OnSessionHasBegun(SessionHasBegunEventArgs e)
		{
			var handler = SessionHasBegun;
			if (handler != null)
			    handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
        #region DealHasBegun Event
        public class DealHasBegunEventArgs : EventArgs
        {
            public DealHasBegunEventArgs (Seat dealer)
            {
                Dealer = dealer;
            }


            public Seat Dealer { get; private set; }
        }

        public event EventHandler<DealHasBegunEventArgs> DealHasBegun;


        protected virtual void OnDealHasBegun (DealHasBegunEventArgs e)
        {
            var handler = DealHasBegun;
            if (handler != null)
                handler.BeginInvoke(this, e, null, null);
        }
        #endregion

		#region CardsHaveBeenDealt Event
		public event EventHandler<EventArgs> CardsHaveBeenDealt;

		protected virtual void OnCardsHaveBeenDealt()
		{
			var handler = CardsHaveBeenDealt;
			if (handler != null)
			    handler.BeginInvoke(this, new EventArgs(), null, null);
		}
		#endregion
		
        #region DealHasBeenAbandoned Event
        public event EventHandler<EventArgs> DealHasBeenAbandoned;


        protected virtual void OnDealHasBeenAbandoned ()
        {
            var handler = DealHasBeenAbandoned;
            if (handler != null)
                handler.BeginInvoke(this, new EventArgs(), null, null);
        }
        #endregion

		#region CallHasBeenMade Event
		public class CallHasBeenMadeEventArgs : EventArgs
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
				handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
		#region BiddingIsComplete Event
		public class BiddingIsCompleteEventArgs : EventArgs
		{
			public BiddingIsCompleteEventArgs(Seat declarer, Contract contract)
			{
				Declarer = declarer;
				Contract = contract;
			}
			
			public Seat Declarer { get; private set; }
			public Contract Contract { get; private set; }
		}

		public event EventHandler<BiddingIsCompleteEventArgs> BiddingIsComplete;

		protected virtual void OnBiddingIsComplete(BiddingIsCompleteEventArgs e)
		{
			var handler = BiddingIsComplete;
			if (handler != null)
				handler.BeginInvoke(this, e, null, null);
		}
		#endregion

        #region DummyHasExposedHand Event
         public class DummyHasExposedHandEventArgs : EventArgs
         {
             public DummyHasExposedHandEventArgs(IEnumerable<Card> dummiesCards)
             {
                 DummiesCards = dummiesCards;
             }
    
             public IEnumerable<Card> DummiesCards { get; private set; }
         }
    
         public event EventHandler<DummyHasExposedHandEventArgs> DummyHasExposedHand;
    
         protected virtual void OnDummyHasExposedHand(DummyHasExposedHandEventArgs e)
         {
             var handler = DummyHasExposedHand;
             if (handler != null)
                 handler.BeginInvoke(this, e, null, null);
         }
         #endregion

		#region CardHasBeenPlayed Event
		public class CardHasBeenPlayedEventArgs : EventArgs
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
				handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
		#region TrickHasBeenWon Event
		public class TrickHasBeenWonEventArgs : EventArgs
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
				handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
		#region PlayerHasQuit Event
		public class PlayerHasQuitEventArgs : EventArgs
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
			    handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
		#region DealHasBeenWon Event
		public class DealHasBeenWonEventArgs : EventArgs
		{
			public DealHasBeenWonEventArgs(Side team, Score score)
			{
				Winners = team;
				Score = score;
			}
			
			public Side Winners { get; private set; }
			public Score Score { get; private set; }
		}

		public event EventHandler<DealHasBeenWonEventArgs> DealHasBeenWon;

		protected virtual void OnDealHasBeenWon(DealHasBeenWonEventArgs e)
		{
			var handler = DealHasBeenWon;
			if (handler != null)
				handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
		#region SessionHasEnded Event
		public class SessionHasEndedEventArgs : EventArgs
		{
			public SessionHasEndedEventArgs(Side team, Score score)
			{
				Winners = team;
				Score = score;
			}
			
			public Side Winners { get; private set; }
			public Score Score { get; private set; }
		}

		public event EventHandler<SessionHasEndedEventArgs> SessionHasEnded;

		protected virtual void OnSessionHasEnded(SessionHasEndedEventArgs e)
		{
			var handler = SessionHasEnded;
			if (handler != null)
				handler.BeginInvoke(this, e, null, null);
		}
		#endregion
		
		#endregion		
	}
}