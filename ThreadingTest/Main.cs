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
        Random r = new Random();

        //public Thread Start ()
        public void Start ()
        {
            Console.WriteLine("[Thread{0}] Told worker to start @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            _stop = false;
            _timeToRespond = false;
            _timerThread = new Thread(new ThreadStart(Run));
            _timerThread.IsBackground = true;
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
                if (_timeToRespond)
                {
                    switch (r.Next(3))
                    {
                        case 0:
                            Message handle1 = EatNotice;
                            if (handle1 != null)
                            {
                                handle1(food);
                            }
                            break;
                        case 1:
                            Message handle2 = ShopNotice;
                            if (handle2 != null)
                            {
                                handle2(item);
                            }
                            break;
                        case 2:
                            Message handle3 = ExerciseNotice;
                            if (handle3 != null)
                            {
                                handle3(activity);
                            }
                            break;
                        default:
                        //Fire no events
                            break;
                    }
                }
            }
            Console.WriteLine("[Thread{0}] Has Stopped @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            //Thread will stop when we fall off the end of this method
        }

        string food;
        string item;
        string activity;
        string[] foods = {"cake", "soup", "salad"};
        string[] items = {"shoes", "skis","bike"};
        string[] activities = {"running","skiing","biking"};

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
                int x = r.Next(3);
                food = foods[x];
                item = items[x];
                activity = activities[x];
            }
            Thread.Sleep(100);
            //Only respond about once per second
            //This will be driven by messages from the table
            _timeToRespond = (r.Next(10) == 0);
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
            Worker w1 = new Worker(){Interval = 1000};
            Worker w2 = new Worker(){Interval = 2000};
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
                string text = Console.ReadLine().Trim().ToLower();
                if (!string.IsNullOrEmpty(text) && "quit".StartsWith(text))
                {
                    if (done == 3)
                    {
                        Console.WriteLine("[Main{0}] is stopping worker 1 @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                        w1.Stop();
                        done--;
                    }
                    else if (done == 2)
                        {
                            Console.WriteLine("[Main{0}] is stopping worker 2 @ {1}", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
                            w2.Stop();
                            done--;
                        }
                        else
                            done--;
                }
            }
            while (done > 0);
        }

    }
}
