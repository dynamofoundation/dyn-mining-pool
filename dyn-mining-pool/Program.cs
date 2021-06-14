﻿using System;
using System.Threading;

namespace dyn_mining_pool
{
    class Program
    {
        static void Main(string[] args)
        {

            RPCServer server = new RPCServer();
            Thread t1 = new Thread(new ThreadStart(server.run));
            t1.Start();

            while (!Global.Shutdown)
            {
                Thread.Sleep(100);
            }

        }
    }
}