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
	public class ScoreTest
	{
        private static List<Trick> TestTricks (Suit trump, Seat declarer)
        {
            //D = declarer's team, d = defender's team
            var tricks = new List<Trick>(13);
            Seat lead = declarer.GetNextSeat(); //Lead:d, Win: D
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.TwoOfClubs, Deck.FiveOfClubs, Deck.FourOfClubs, Deck.ThreeOfClubs}));
            lead = lead.GetNextSeat();  // Lead:D, Win:d
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.EightOfClubs, Deck.NineOfClubs, Deck.SevenOfClubs, Deck.SixOfClubs}));
            lead = lead.GetNextSeat();  // Lead:d, Win:d
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.AceOfClubs, Deck.KingOfClubs, Deck.QueenOfClubs, Deck.JackOfClubs}));
            //lead = lead.GetNextSeat();  // Lead:d, Win:d
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.TenOfClubs, Deck.TwoOfDiamonds, Deck.ThreeOfDiamonds, Deck.FourOfDiamonds}));
            //lead = lead.GetNextSeat();  // Lead:d, Win:D
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.FiveOfDiamonds, Deck.EightOfDiamonds, Deck.SixOfDiamonds, Deck.SevenOfDiamonds}));
            lead = lead.GetNextSeat();  // Lead:D, Win:D
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.KingOfDiamonds, Deck.QueenOfDiamonds, Deck.JackOfDiamonds, Deck.TenOfDiamonds}));
            //lead = lead.GetNextSeat();  // Lead:D, Win:d
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.TwoOfHearts, Deck.FourOfHearts, Deck.AceOfDiamonds, Deck.ThreeOfHearts}));
            lead = lead.GetNextSeat();  // Lead:d, Win:D
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.FiveOfHearts, Deck.EightOfHearts, Deck.SixOfHearts, Deck.SevenOfHearts}));
            lead = lead.GetNextSeat();  // Lead:D, Win:d
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.NineOfHearts, Deck.QueenOfHearts, Deck.TenOfHearts, Deck.JackOfHearts}));
            lead = lead.GetNextSeat();  // Lead:d, Win:D
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.KingOfHearts, Deck.AceOfHearts, Deck.AceOfSpades, Deck.KingOfSpades}));
            lead = lead.GetNextSeat();  // Lead:D, Win:D
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.NineOfDiamonds, Deck.QueenOfSpades, Deck.JackOfSpades, Deck.TenOfSpades}));
            //lead = lead.GetNextSeat();  // Lead:D, Win:d
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.EightOfSpades, Deck.NineOfSpades, Deck.SevenOfSpades, Deck.SixOfSpades}));
            lead = lead.GetNextSeat();  // Lead:d, Win:D
            tricks.Add(Trick.FromCards(trump, lead, new[] {Deck.FourOfSpades, Deck.FiveOfSpades, Deck.ThreeOfSpades, Deck.TwoOfSpades}));
            //Score D: 7, d: 6
            return tricks;
        }



        [Test()]
		public void Test1 ()
        {
            Contract contract = new Contract(new Bid(3, Suit.Hearts), 0);
            List<Trick> tricks = TestTricks(Suit.Hearts, Seat.East);
            Score score = new Score(Seat.East, contract, tricks, Vulnerability.EastWest);;
            //int score1 = score.GetDeclarersGameScore();
            //int score2 = score.GetDeclarersBonusScore();
            //int score3 = score.GetDefendersScore();
            //bool game = score.DidDeclarerMakeGame;
            score.ToString();
        }

		[Test()]
		public void Test2 ()
		{
		}
	}
}