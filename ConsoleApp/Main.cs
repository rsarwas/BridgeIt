// #region License and Terms
// // GnuBridge (C#)
// // Copyright (c) 2011-2012 Regan Sarwas. All rights reserved.
// //
// // Modeled after GNUBridge 0.1.19 (java) by Paul Slusarz (http://gnubridge.org)
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
using System.Collections.Generic;
using BridgeIt.Core;
using BridgeIt.Players;
using BridgeIt.Tables;

namespace ConsoleApp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Table table = new ContractTable();
			ConsolePlayer player1 = new ConsolePlayer(Console.In, Console.Out);
			ConsolePlayer player2 = new ConsolePlayer(Console.In, Console.Out);
			ConsolePlayer player3 = new ConsolePlayer(Console.In, Console.Out);
			ConsolePlayer player4 = new ConsolePlayer(Console.In, Console.Out);
			player1.JoinTable(table);
			player2.JoinTable(table);
			player3.JoinTable(table);
			player4.JoinTable(table);
			table.Start();
		}
	}
}
