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

using System;
using BridgeIt.Core;
using BridgeIt.Players;
using BridgeIt.Tables;
using System.Threading;
using System.Collections.Generic;

namespace ConsoleApp
{
	class MainClass
	{
		public static void Main (string[] args)
        {
            //PlayGame();
            TestScore();
        }

        private static void PlayGame ()
        {
            //List<SimpleComputerPlayer> players = new List<SimpleComputerPlayer>();
            var threads = new List<Thread>();
            Table table = new ContractTable();
            for (int i = 0; i < Table.Seats.Length-1; i++)
            //foreach (Seat seat in Table.Seats)
            {
                //seat is not used
                //(new ConsolePlayer(Console.In, Console.Out)).JoinTable(table);
                var p = new SimpleComputerPlayer();
                p.JoinTable(table);
                threads.Add(p.Start());
            }
            var ph = new ConsolePlayer(System.Console.In, System.Console.Out);
            ph.JoinTable(table);
            table.StartSession();
            
            foreach (var t in threads)
                t.Join();
        }

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



        private static void TestScore ()
        {
            Contract contract = new Contract(new Bid(3, Suit.Hearts), 0);
            List<Trick> tricks = TestTricks(Suit.Hearts, Seat.East);
            Score score = new Score(Seat.East, contract, tricks, Vulnerability.EastWest);

            Console.WriteLine("Declarer:{0}, Contract:{1}, ContractScore:{2}, MadeContract:{3}", score.Declarer, score.Contract, score.ContractScore, score.MadeContract);
            Console.WriteLine("Game:{0}, SmallSlam:{1}, GrandSlam:{2}", score.Game, score.SmallSlam, score.GrandSlam);
            Console.WriteLine("TricksDefeated:{0}, TricksTaken:{1}, Vulnerability:{2}, PartScore:{3}", score.TricksDefeated, score.TricksTaken, score.Vulnerability, score.PartScore);
            Console.WriteLine("Overtrick:{0}, Penalties:{1}, Bonus:{2}, Insult:{3}", score.GetOverTrickPoints(), score.GetPenalties(), score.GetLevelBonus(), score.GetInsult());
            Console.WriteLine(score.ToString());
            Console.ReadLine();
        }
	}
}
