using System;
using System.Collections.Generic;
using System.Text;

namespace dyn_mining_pool
{
    public class Global
    {

        public static bool Shutdown = false;
        public static string FullNodeRPC = "http://192.168.1.62:6433/";
        public static string FullNodeUser = "user";
        public static string FullNodePass = "123456";

        public static string AlgoProgram;
        public static string PrevBlockHash;
        public static string CurrPoolTarget;
        public static string CurrBlockTarget;


    }
}
