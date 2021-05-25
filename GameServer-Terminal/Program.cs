using System;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static bool isrunning = false;
        static void Main(string[] args)
        {
            Console.Title = "GameServer";
            isrunning = true;
            Thread mainthread = new Thread(new ThreadStart(MainThread));
            mainthread.Start();
            Server.Start(20, 26950);  
        }
        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextloop = DateTime.Now;

            while(isrunning)
            {
                while(nextloop<DateTime.Now)
                {
                    Gamelogic.Update();
                    nextloop = nextloop.AddMilliseconds(Constants.MS_PER_SEC);

                    if(nextloop>DateTime.Now)
                    {
                        Thread.Sleep(nextloop - DateTime.Now);
                    }
                }
            }
        }
    }
}
