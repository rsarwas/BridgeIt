// #region License and Terms
// // BridgeIt
// // Copyright (c) 2011-2012 Regan Sarwas. All rights reserved.
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

namespace BridgeIt.Core
{
    [Serializable]
    public class BridgeException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BridgeException"/> class
        /// </summary>
        public BridgeException ()
        {
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BridgeException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        public BridgeException (string message) : base (message)
        {
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BridgeException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public BridgeException (string message, Exception inner) : base (message, inner)
        {
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BridgeException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected BridgeException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
        {
        }
    }

    [Serializable]
    public class CallException : BridgeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CallException"/> class
        /// </summary>
        public CallException ()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CallException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        public CallException (string message) : base (message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CallException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public CallException (string message, Exception inner) : base (message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CallException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected CallException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
        {
        }
    }


    [Serializable]
    public class PlayException : BridgeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PlayException"/> class
        /// </summary>
        public PlayException ()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PlayException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        public PlayException (string message) : base (message)
        {
        }
 
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PlayException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public PlayException (string message, Exception inner) : base (message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PlayException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected PlayException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
        {
        }
    }
}

