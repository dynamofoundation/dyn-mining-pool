using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace dyn_mining_pool
{
    public class Distributor
    {

        public void run()
        {
            while (!Global.Shutdown)
            {



                Thread.Sleep(1000);
            }
        }
    }
}
