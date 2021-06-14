using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace dyn_mining_pool
{
    class RPCServer
    {

        public void run()
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("HTTP Listener not supported");
                return;
            }

            HttpListener listener = new HttpListener();

            listener.Prefixes.Add("http://127.0.0.1:6434/");

            listener.Start();
            Console.WriteLine("Listening...");

            while (!Global.Shutdown)
            {
                HttpListenerContext context = listener.GetContext();
                RPCWorker worker = new RPCWorker();
                worker.context = context;
                Thread t1 = new Thread(new ThreadStart(worker.run));
                t1.Start();
            }

            listener.Stop();
        }
    }
}
