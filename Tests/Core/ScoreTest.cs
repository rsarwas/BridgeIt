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
		[Test()]
		public void Test1 ()
        {
            Score score = new Score(contract, tricks, vulnerability);
            int score1 = score.GetDeclarersGameScore();
            int score2 = score.GetDeclarersBonusScore();
            int score3 = score.GetDefendersScore();
            bool game = score.DidDeclarerMakeGame;
            score.ToString();
        }

		[Test()]
		public void Test2 ()
		{
		}
	}
}