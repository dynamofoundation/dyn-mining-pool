using System;
using System.Threading;

namespace dyn_mining_pool
{
    class Program
    {
        static void Main(string[] args)
        {
            uint loops = 0;

            Database.CreateOrOpenDatabase();

            Console.WriteLine("Starting RPC server...");
            RPCServer server = new RPCServer();
            Thread t1 = new Thread(new ThreadStart(server.run));
            t1.Start();

            Console.WriteLine("Starting distributor...");
            Distributor distributor = new Distributor();
            Thread t2 = new Thread(new ThreadStart(distributor.run));
            t2.Start();

            while (!Global.Shutdown)
            {
                Thread.Sleep(100);
                loops++;
                Global.updateRand(loops);
            }

        }
    }
}
