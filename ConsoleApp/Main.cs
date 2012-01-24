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

//using System;
//using BridgeIt.Core;
using BridgeIt.Players;
using BridgeIt.Tables;
//using System.Threading;
using System.Collections.Generic;

namespace ConsoleApp
{
	class MainClass
	{
		public static void Main (string[] args)
        {
            //List<SimpleComputerPlayer> players = new List<SimpleComputerPlayer>();
            var players = new List<SimpleComputerPlayer>();
            Table table = new ContractTable();
            for (int i = 0; i < Table.Seats.Length; i++ )
            //foreach (Seat seat in Table.Seats)
            {
                //seat is not used
                //(new ConsolePlayer(Console.In, Console.Out)).JoinTable(table);
                var p = new SimpleComputerPlayer();
                p.JoinTable(table);
                p.Start();
                players.Add(p);
            }

            table.StartSession();
            
            foreach (var p in players)
                p.Thread.Join();
        }
	}
}
