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

*/

namespace BridgeIt.Core
{
	public interface IPlayer
	{
		void PlaceBid();
		void PlaceBidNow(System.TimeSpan timelimit);
		void Play();
		void PlayNow(System.TimeSpan timelimit);
		void PlayForDummy(IPlayer dummy);
		void PlayForDummyNow(IPlayer dummy, System.TimeSpan timelimit);
	}
}