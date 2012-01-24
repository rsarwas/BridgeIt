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
using System.Threading;

namespace ThreadingTest
{
    // Delegates
    public delegate void Message (string content);

    class Worker
    {

        // Properties
        public int Interval {get; set;}
        public Message EatNotice;
        public Message ShopNotice;
        public Message ExerciseNotice;

        // Private Data
        Thread _timerThread;
        bool _stop;
        bool _timeToRespond;
        readonly Random _random = new Random();

        //public Thread Start ()
        public void Start ()
        {
            Console.WriteLine("[Thread{0}] Told worker to start @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            _stop = false;
            _timeToRespond = false;
            _timerThread = new Thread(Run) {IsBackground = true};
            _timerThread.Start();
            //return _timerThread;
        }


        public void Stop ()
        {
            Console.WriteLine("[Thread{0}] Told worker to stop @ {1}!", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            // Request the loop to stop
            _stop = true;
        }


        private void Run ()
        {
            Console.WriteLine("[Thread{0}] Has Started @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            while (!_stop)
            {

                //Keep the time in DoSomeWork about 100 milleseconds or less for responsibveness
                //Do not let it get too low, or else we will waste a lot of CPU time spinning
                DoSomeWork();

                // check state to see what events to fire
                if (!_timeToRespond)
                    continue;
                switch (_random.Next(3))
                {
                    case 0:
                        Message handle1 = EatNotice;
                        if (handle1 != null)
                        {
                            handle1(_food);
                        }
                        break;
                    case 1:
                        Message handle2 = ShopNotice;
                        if (handle2 != null)
                        {
                            handle2(_item);
                        }
                        break;
                    case 2:
                        Message handle3 = ExerciseNotice;
                        if (handle3 != null)
                        {
                            handle3(_activity);
                        }
                        break;
                }
            }
            Console.WriteLine("[Thread{0}] Has Stopped @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            //Thread will stop when we fall off the end of this method
        }

        string _food;
        string _item;
        string _activity;
        readonly string[] _foods = {"cake", "soup", "salad"};
        readonly string[] _items = {"shoes", "skis","bike"};
        readonly string[] _activities = {"running","skiing","biking"};

        private void DoSomeWork ()
        {
            //try to keep the amount of work done here, small
            //Since we are not checking for interupts while working

            //If I have work to do
            //    do it
            //else
            //   Thread.Sleep(100);

            for (int i = 0; i < 1000; i++)
            {
                int x = _random.Next(3);
                _food = _foods[x];
                _item = _items[x];
                _activity = _activities[x];
            }
            Thread.Sleep(100);
            //Only respond about once per second
            //This will be driven by messages from the table
            _timeToRespond = (_random.Next(10) == 0);
        }

    }

    class MainClass
    {

        public static void Eat (string food)
        {
            Console.WriteLine("[Worker{0}] Told main to eat {1}!", Thread.CurrentThread.ManagedThreadId, food);
        }

        public static void Shop (string item)
        {
            Console.WriteLine("[Worker{0}] Told main to go buy {1}!", Thread.CurrentThread.ManagedThreadId, item);
        }

        public static void Exercise (string activity)
        {
            Console.WriteLine("[Worker{0}] Told main to go {1}.", Thread.CurrentThread.ManagedThreadId, activity);
        }


        public static void Main (string[] args)
        {
            Console.WriteLine("[Main{0}] Says: Hello World!", Thread.CurrentThread.ManagedThreadId);
            var w1 = new Worker {Interval = 1000};
            var w2 = new Worker {Interval = 2000};
            w1.EatNotice += Eat;
            w1.ShopNotice += Shop;
            w1.ExerciseNotice += Exercise;
            w2.EatNotice += Eat;
            w2.ShopNotice += Shop;
            w2.ExerciseNotice += Exercise;

            Console.WriteLine("[Main{0}] is starting worker 1 @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            w1.Start();
            Console.WriteLine("[Main{0}] is starting worker 2 @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            w2.Start();

            //Main loop
            int done = 3;
            do
            {
                Console.Write("[Main{0}] Enter some text or q to quit: ", Thread.CurrentThread.ManagedThreadId);
                var readLine = Console.ReadLine();
                if (readLine == null)
                    continue;

                string text = readLine.Trim().ToLower();
                if (string.IsNullOrEmpty(text) || !"quit".StartsWith(text))
                    continue;

                switch (done)
                {
                    case 3:
                        Console.WriteLine("[Main{0}] is stopping worker 1 @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                        w1.Stop();
                        done--;
                        break;
                    case 2:
                        Console.WriteLine("[Main{0}] is stopping worker 2 @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                        w2.Stop();
                        done--;
                        break;
                    default:
                        done--;
                        break;
                }
            }
            while (done > 0);
        }

    }
}
