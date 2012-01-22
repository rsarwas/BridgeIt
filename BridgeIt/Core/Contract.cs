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

namespace BridgeIt.Core
{
    /// <summary>
    /// The undertaking by declarer’s side to win, at the denomination named, the number odd tricks specified in the final bid, whether undoubled, doubled or redoubled.
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
    /// </exception>
    /// <exception cref='ArgumentOutOfRangeException'>
    /// Is thrown when an argument passed to a method is invalid because it is outside the allowable range of values as
    /// specified by the method.
    /// </exception>
    public class Contract
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BridgeIt.Core.Contract"/> class.
        /// </summary>
        /// <param name='bid'>
        /// Bid.
        /// </param>
        /// <param name='doubles'>
        /// Doubles.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        /// <exception cref='ArgumentOutOfRangeException'>
        /// Is thrown when an argument passed to a method is invalid because it is outside the allowable range of values
        /// as specified by the method.
        /// </exception>
        public Contract (Bid bid, int doubles)
        {
            if (bid == null)
                throw new ArgumentNullException("bid");
            if (doubles < 0 || 2 < doubles)
                throw new ArgumentOutOfRangeException("doubles", "The number of doubles can only be 0, 1, or 2");

            Bid = bid;
            Doubles = doubles;
        }

        /// <summary>
        /// Gets the the number of doubles on the final bid.
        /// </summary>
        /// <value>
        /// The doubles (0 = undoubled, 1 = doubled, 2 = redoubled).
        /// </value>
        public int Doubles { get; private set; }

        /// <summary>
        /// Gets or sets the bid.
        /// </summary>
        /// <value>
        /// The final <see cref="BridgeIt.Core.Bid" />.
        /// </value>
        public Bid Bid { get; private set; }

        public override string ToString ()
        {
            return Bid.ToString() + (Doubles == 0 ? "" : (Doubles == 1 ? " Doubled" : " Redoubled"));
        }
    }
}
