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

using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeIt.Core
{
	public class Score
	{

        private Seat _declarer;
        private Contract _contract;
        private List<Trick> _tricks;
        private bool _vulnerable;

        public Score (Seat declarer, Contract contract, IEnumerable<Trick> tricks, bool vulnerable)
        {
            //FIXME add error checking
            _declarer = declarer;
            _contract = contract;
            _tricks = tricks.ToList();
            _vulnerable = vulnerable;
            TricksTaken = GetTricksTaken();
            TricksDefeated = contract.Bid.Tricks - TricksTaken; //todo limit to positive numbers ??
            ContractScore = GetContractScore();
        }

        public int TricksTaken { get; private set; }

        public int TricksDefeated { get; private set; }

        public int ContractScore { get; private set; }


        private int GetTricksTaken ()
        {
            int count = _tricks.Sum(t => t.Winner.GetSide() == _declarer.GetSide() ? 1 : 0);
            return count - 6;
            //Todo: limit to positive numbers ???
        }

        private int GetContractScore ()
        {
            int score = 0;
            score += 20 * TricksBidAndMadeInMinorSuitContract * Doubler;
            score += 30 * TricksBidAndMadeInMajorSuitContract * Doubler;
            score += 30 * TricksBidAndMadeInNoTrumpContract * Doubler;
            score += 10 * FirstTrickBidAndMadeInNoTrumpContract * Doubler;
            return score;
        }


        private int GetLevelBonus ()
        {
            if (PartScore)
                return 50;
            int score = 0;
            if (Game)
                score += _vulnerable ? 500 : 300;
            if (SmallSlam)
                score += _vulnerable ? 750 : 500;
            if (GrandSlam)
                score += _vulnerable ? 1500 : 1000;;
            return score;
        }


        public int GetInsult ()
        {
            if (MadeContract)
                return  _contract.Doubles * 50;
            return 0;
        }


        public int GetOverTrickPoints ()
        {
            switch (_contract.Doubles)
            {
                case 0:
                    int score = 0;
                    score += 20 * TricksOverMinorSuitContract;
                    score += 30 * TricksOverMajorSuitContract;
                    score += 30 * TricksOverNoTrumpContract;
                    return score;
                case 1:
                    if (_vulnerable)
                        return 200 * TricksOverContract;
                    return 100 * TricksOverContract;
                case 2:
                    if (_vulnerable)
                        return 400 * TricksOverContract;
                    return 200 * TricksOverContract;
                default:
                    throw new InvalidOperationException("contract (re)double value out of range 0..2");
            }
        }

        public int GetPenalties ()
        {
            if (TricksUnderContract <= 0)
                return 0;

            int score = 0;
            switch (_contract.Doubles)
            {
                case 0:
                    if (_vulnerable)
                        return 100 * TricksUnderContract;
                    return 50 * TricksUnderContract;
                case 1:
                    if (_vulnerable)
                    {
                        score += 200 * FirstUnderTrick;
                        score += 300 * SecondAndThirdUnderTricks;
                        score += 300 * FourthAndFurtherUnderTricks;
                        return score;
                    }
                    score += 100 * FirstUnderTrick;
                    score += 200 * SecondAndThirdUnderTricks;
                    score += 300 * FourthAndFurtherUnderTricks;
                    return score;
                case 2:
                    if (_vulnerable)
                    {
                        score += 400 * FirstUnderTrick;
                        score += 600 * SecondAndThirdUnderTricks;
                        score += 600 * FourthAndFurtherUnderTricks;
                        return score;
                    }
                    score += 200 * FirstUnderTrick;
                    score += 400 * SecondAndThirdUnderTricks;
                    score += 600 * FourthAndFurtherUnderTricks;
                    return score;
                default:
                    throw new InvalidOperationException("contract (re)double value out of range 0..2");
            }
        }

        private int Doubler
        {
            get
            {
                return _contract.Doubles == 0 ? 1 : 2 * _contract.Doubles;
            }
        }


        private int TricksBidAndMadeInMinorSuitContract
        {
            get
            {
                if (_contract.Bid.Suit.IsMinor())
                    return TricksBidAndMade;
                return 0;
            }
        }


        private int TricksBidAndMadeInMajorSuitContract
        {
            get
            {
                if (_contract.Bid.Suit.IsMajor())
                    return TricksBidAndMade;
                return 0;
            }
        }

        private int TricksBidAndMadeInNoTrumpContract
        {
            get
            {
                if (_contract.Bid.Suit == Suit.NoTrump)
                    return TricksBidAndMade;
                return 0;
            }
        }

        private int FirstTrickBidAndMadeInNoTrumpContract
        {
            get
            {
                if (_contract.Bid.Suit != Suit.NoTrump)
                    return 0;
                return TricksBidAndMade > 1 ? 1 : 0;
            }
        }

        private int TricksBidAndMade
        {
            get
            {
                if (MadeContract)
                    return _contract.Bid.Tricks;
                else
                    return TricksTaken;
            }
        }

        private int TricksOverMinorSuitContract
        {
            get
            {
                if (_contract.Bid.Suit.IsMinor())
                    return TricksOverContract;
                return 0;
            }
        }


        private int TricksOverMajorSuitContract
        {
            get
            {
                if (_contract.Bid.Suit.IsMajor())
                    return TricksOverContract;
                return 0;
            }
        }

        private int TricksOverNoTrumpContract
        {
            get
            {
                if (_contract.Bid.Suit == Suit.NoTrump)
                    return TricksOverContract;
                return 0;
            }
        }


        private int TricksOverContract
        {
            get
            {
                if (MadeContract)
                    return TricksTaken - _contract.Bid.Tricks;
                else
                    return 0;
            }
        }


        private int TricksUnderContract
        {
            get
            {
                return MadeContract ? 0 : _contract.Bid.Tricks - TricksTaken;
            }
        }

        private int FirstUnderTrick
        {
            get
            {
                return 0 < TricksUnderContract ? 1 : 0;
            }
        }

        private int SecondAndThirdUnderTricks
        {
            get
            {
                return 1 < TricksUnderContract ? (TricksUnderContract < 4 ? TricksUnderContract - 1 : 2): 0;
            }
        }

        private int FourthAndFurtherUnderTricks
        {
            get
            {
                return 3 < TricksUnderContract ? TricksUnderContract - 3 : 0;
            }
        }


        public bool MadeContract { get { return _contract.Bid.Tricks <= TricksTaken; } }

        public bool PartScore { get { return ContractScore < 100; } }

        public bool Game { get { return 100 <= ContractScore; } }

        public bool SmallSlam {get { return TricksTaken == 6; } }

        public bool GrandSlam { get { return TricksTaken == 7; } }

        public override string ToString ()
        {
            return string.Format ("[Score]");
        }
	}
}

